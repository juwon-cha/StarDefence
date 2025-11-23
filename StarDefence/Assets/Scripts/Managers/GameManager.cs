using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameStatus
{
    Ready,
    Build,
    Wave,
    GameOver,
    Victory
}

public class GameManager : Singleton<GameManager>
{
    [Header("Game Resources")]
    [SerializeField] private int initialGold = 100;
    [SerializeField] private int initialMinerals = 0;
    public int Gold { get; private set; }
    public int Minerals { get; private set; }
    public event System.Action<int> OnGoldChanged;
    public event System.Action<int> OnMineralsChanged;

    [Header("Commander Data")]
    public List<CommanderDataSO> commanders;

    [Header("Hero Data")]
    public List<HeroDataSO> tier1Heroes;

    public Commander Commander { get; private set; }
    public Hero SelectedHero { get; private set; }
    private List<Hero> placedHeroes = new List<Hero>();

    public GameStatus Status { get; private set; }
    public event System.Action<GameStatus> OnStatusChanged;

    protected override void Awake()
    {
        base.Awake();
        Status = GameStatus.Ready;
        Gold = initialGold;
        Minerals = initialMinerals;
    }

    private void OnEnable()
    {
        Tile.OnTileClicked += HandleTileClicked;
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    private void OnDisable()
    {
        Tile.OnTileClicked -= HandleTileClicked;
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        SpawnCommander();
        ChangeStatus(GameStatus.Build);
        // UI를 위한 초기값 통지
        OnGoldChanged?.Invoke(Gold);
        OnMineralsChanged?.Invoke(Minerals);
    }

    private void SpawnCommander()
    {
        if (commanders == null || !commanders.Any())
        {
            Debug.LogError("생성할 수 있는 지휘관이 없습니다.");
            return;
        }

        CommanderDataSO commanderData = commanders[0];
        GameObject commanderGO = PoolManager.Instance.Get(commanderData.FullPrefabPath);
        if (commanderGO == null) return;

        Transform endPoint = GridManager.Instance.EndPoint;
        if (endPoint == null)
        {
            Debug.LogError("GridManager에 EndPoint가 설정되지 않았습니다.");
            return;
        }

        commanderGO.transform.position = endPoint.position;
        commanderGO.transform.rotation = Quaternion.identity;
    }

    private void HandleTileClicked(Tile tile)
    {
        if (tile == null) return;

        if (tile.PlacedHero != null)
        {
            SetSelectedHero(tile.PlacedHero);
            
            // 최종 티어 영웅인지 확인
            if (tile.PlacedHero.HeroData.nextTierHero == null)
            {
                // 초월하지 않은 최종 티어 영웅이면 초월 UI를 띄움
                if (!tile.PlacedHero.IsTranscended)
                {
                    Debug.Log("최종 티어 영웅입니다. 초월 업그레이드를 시도합니다.");
                    // 초월 업그레이드 데이터가 할당되어 있는지 확인
                    if (tile.PlacedHero.HeroData.transcendenceUpgrade != null)
                    {
                        var confirmUI = UIManager.Instance.ShowWorldSpacePopup<PlaceHeroConfirmUI>(Constants.PLACE_HERO_CONFIRM_UI_NAME);
                        if (confirmUI != null)
                        {
                            confirmUI.SetDataForTranscendence(tile.PlacedHero, tile.PlacedHero.HeroData.transcendenceUpgrade);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"영웅 {tile.PlacedHero.HeroData.heroName}의 HeroDataSO에 초월 업그레이드 데이터가 할당되지 않았습니다.");
                    }
                }
                else
                {
                    Debug.Log("이미 초월한 신화 등급 영웅입니다.");
                }
            }
            else
            {
                // 최종 티어가 아니면 기존 융합 업그레이드 시도
                TryUpgradeHero(tile.PlacedHero);
            }
        }
        else 
        {
            DeselectHero(); // 빈 타일 클릭 시 영웅 선택 해제
            
            if (tile.IsPlaceable)
            {
                var confirmUI = UIManager.Instance.ShowWorldSpacePopup<PlaceHeroConfirmUI>(Constants.PLACE_HERO_CONFIRM_UI_NAME);
                if (confirmUI != null)
                {
                    confirmUI.SetDataForPlacement(tile, tier1Heroes.Any() ? tier1Heroes[0].placementCost : 0);
                }
            }
            else if (tile.IsFixable)
            {
                var confirmUI = UIManager.Instance.ShowWorldSpacePopup<PlaceHeroConfirmUI>(Constants.PLACE_HERO_CONFIRM_UI_NAME);
                if (confirmUI != null)
                {
                    confirmUI.SetDataForRepair(tile, CalculateCurrentRepairCost());
                }
            }
        }
    }

    #region 재화 관리
    public void AddGold(int amount)
    {
        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
    }

    public bool SpendGold(int amount)
    {
        if (Gold >= amount)
        {
            Gold -= amount;
            OnGoldChanged?.Invoke(Gold);
            return true;
        }

        Debug.Log("골드가 부족합니다.");
        return false;
    }

    public void AddMinerals(int amount)
    {
        Minerals += amount;
        OnMineralsChanged?.Invoke(Minerals);
    }

    public bool SpendMinerals(int amount)
    {
        if (Minerals >= amount)
        {
            Minerals -= amount;
            OnMineralsChanged?.Invoke(Minerals);
            return true;
        }

        Debug.Log("미네랄이 부족합니다.");
        return false;
    }

    private void HandleEnemyDestroyed(Enemy enemy)
    {
        // 파괴된 적의 골드 보상을 GameManager에 추가
        if (enemy.CreatureData is EnemyDataSO enemyData)
        {
            AddGold(enemyData.goldReward);
        }
    }

    public void TryPurchaseProbe()
    {
        if (ProbeManager.Instance.CurrentProbeCount >= ProbeManager.Instance.MaxProbeCount)
        {
            Debug.Log("프로브 생성 실패: 최대 인구수에 도달했습니다.");
            return;
        }

        int cost = ProbeManager.Instance.GetCurrentProbeCost();
        if (SpendGold(cost))
        {
            ProbeManager.Instance.CreateProbe();
            Debug.Log("프로브 생성 완료!");
        }
        else
        {
            Debug.Log("프로브 생성 실패: 골드가 부족합니다.");
        }
    }
    #endregion

    #region 타일 수리
    [Header("Tile Repair Costs")]
    [SerializeField] private int baseRepairCost = 20;
    [SerializeField] private int repairCostIncrease = 10;
    private int repairCount = 0;

    private int CalculateCurrentRepairCost()
    {
        return baseRepairCost + (repairCount * repairCostIncrease);
    }

    public void ConfirmRepairTile(Tile tile)
    {
        int cost = CalculateCurrentRepairCost();
        if (SpendMinerals(cost))
        {
            tile.Repair();
            repairCount++;
            Debug.Log("타일 수리 완료!");
        }
        else
        {
            Debug.Log("타일 수리 실패: 미네랄이 부족합니다.");
        }
    }
    #endregion

    #region 영웅 배치 및 업그레이드

    /// <summary>
    /// 지정된 HeroDataSO를 사용하여 특정 타일에 영웅을 생성하고 초기화
    /// </summary>
    /// <param name="heroData">생성할 영웅의 데이터</param>
    /// <param name="targetTile">영웅을 배치할 타일</param>
    /// <returns>생성된 Hero 인스턴스 또는 실패 시 null</returns>
    public Hero SpawnHeroOnTile(HeroDataSO heroData, Tile targetTile)
    {
        if (heroData == null || targetTile == null)
        {
            Debug.LogError("SpawnHeroOnTile: HeroData 또는 TargetTile이 null입니다.");
            return null;
        }
        if (string.IsNullOrEmpty(heroData.FullHeroPrefabPath))
        {
            Debug.LogError($"SpawnHeroOnTile: 영웅 '{heroData.heroName}'의 HeroDataSO에 유효한 프리팹 경로가 없습니다!");
            return null;
        }

        GameObject heroGO = PoolManager.Instance.Get(heroData.FullHeroPrefabPath);
        if (heroGO == null) return null;

        heroGO.transform.position = targetTile.transform.position;
        heroGO.transform.rotation = Quaternion.identity;

        Hero newHero = heroGO.GetComponent<Hero>();
        if (newHero == null)
        {
            Debug.LogError($"SpawnHeroOnTile: 프리팹 '{heroData.FullHeroPrefabPath}'에서 Hero 컴포넌트를 찾을 수 없습니다.");
            PoolManager.Instance.Release(heroGO);
            return null;
        }

        newHero.Init(heroData, targetTile);
        targetTile.SetHero(newHero);
        placedHeroes.Add(newHero);

        Debug.Log($"Hero spawned: {heroData.heroName}(T{heroData.tier}) on tile {targetTile.name}. Total heroes: {placedHeroes.Count}");
        return newHero;
    }

    public void SetSelectedHero(Hero hero)
    {
        SelectedHero = hero;
        Debug.Log($"영웅 선택됨: {hero.HeroData.heroName}");
        // TODO: 선택된 영웅을 시각적으로 표시하는 UI 로직 추가
    }

    public void DeselectHero()
    {
        if (SelectedHero != null)
        {
            Debug.Log($"영웅 선택 해제: {SelectedHero.HeroData.heroName}");
            SelectedHero = null;
            // TODO: 영웅 선택 표시 UI 해제
        }
    }

    public void ConfirmPlaceHero(Tile tile)
    {
        if (!tier1Heroes.Any())
        {
            Debug.LogWarning("1티어 영웅 목록이 비어있습니다! 영웅을 배치할 수 없습니다.");
            return;
        }

        int placementCost = tier1Heroes[0].placementCost;
        if (!SpendGold(placementCost))
        {
            Debug.Log("배치 실패: 골드가 부족합니다.");
            return;
        }
        
        HeroDataSO heroData = UpgradeManager.Instance.GetRandomHeroForSummon();
        if (heroData == null)
        {
            Debug.LogError("UpgradeManager가 소환할 영웅을 반환하지 않았습니다!");
            AddGold(placementCost); 
            return;
        }
        
        SpawnHeroOnTile(heroData, tile);
    }

    private void TryUpgradeHero(Hero heroToUpgrade)
    {
        if (heroToUpgrade.HeroData.nextTierHero == null)
        {
            Debug.Log("이 영웅은 최고 등급입니다.");
            return;
        }

        Hero mergePartner = placedHeroes.FirstOrDefault(h =>
            h != heroToUpgrade && h.HeroData == heroToUpgrade.HeroData);

        if (mergePartner == null)
        {
            Debug.Log("융합할 같은 종류의 다른 영웅이 없습니다.");
            return;
        }

        var confirmUI = UIManager.Instance.ShowWorldSpacePopup<PlaceHeroConfirmUI>(Constants.PLACE_HERO_CONFIRM_UI_NAME);
        if (confirmUI != null)
        {
            confirmUI.SetDataForUpgrade(heroToUpgrade, mergePartner);
        }
    }

    public void ConfirmUpgradeHero(Hero heroToUpgrade, Hero mergePartner)
    {
        if (!SpendMinerals(heroToUpgrade.HeroData.upgradeCost))
        {
            Debug.Log("업그레이드 실패: 미네랄이 부족합니다.");
            return;
        }

        HeroDataSO nextTierHeroData = heroToUpgrade.HeroData.nextTierHero;
        Tile targetTile = heroToUpgrade.placedTile;

        RemoveHero(heroToUpgrade);
        RemoveHero(mergePartner);
        
        SpawnHeroOnTile(nextTierHeroData, targetTile); // 헬퍼 메서드 사용
    }
    
    public void ConfirmTranscendence(Hero hero, UpgradeDataSO transcendenceUpgrade)
    {
        if (hero == null || transcendenceUpgrade == null)
        {
            Debug.LogError("ConfirmTranscendence: 영웅 또는 초월 업그레이드 데이터가 null입니다.");
            return;
        }
        
        // 비용 확인 및 지불
        int cost = UpgradeManager.Instance.GetCurrentCost(transcendenceUpgrade.upgradeType);
        bool success = transcendenceUpgrade.useGold ? SpendGold(cost) : SpendMinerals(cost);

        if (success)
        {
            UpgradeManager.Instance.PurchaseUpgrade(transcendenceUpgrade.upgradeType);
            
            if (hero.HeroData.mythicHeroData != null)
            {
                Tile currentTile = hero.placedTile; // 기존 영웅의 타일 저장
                RemoveHero(hero); // 기존 영웅 제거
                
                // 신화 등급 영웅 생성 및 배치
                Hero newMythicHero = SpawnHeroOnTile(hero.HeroData.mythicHeroData, currentTile);
                if (newMythicHero != null)
                {
                    Debug.Log($"영웅 {hero.HeroData.heroName}이(가) 성공적으로 초월하여 {newMythicHero.HeroData.heroName}으로 변신했습니다!");
                }
            }
            else
            {
                Debug.LogWarning($"초월했지만 영웅 {hero.HeroData.heroName}의 HeroDataSO에 mythicHeroData가 할당되지 않아 변신하지 못했습니다.");
            }
        }
        else
        {
            Debug.LogWarning($"초월 실패: {hero.HeroData.heroName} 초월에 필요한 재화가 부족합니다.");
        }
        
        DeselectHero();
    }

    private void RemoveHero(Hero hero)
    {
        if (hero == null)
        {
            return;
        }

        hero.placedTile.ClearHero();
        placedHeroes.Remove(hero);
        hero.Cleanup();
        PoolManager.Instance.Release(hero.gameObject);
    }

    #endregion

    public void ChangeStatus(GameStatus newStatus)
    {
        if (Status == newStatus)
        {
            return;
        }

        Status = newStatus;
        OnStatusChanged?.Invoke(Status);
    }

    public void SetCommander(Commander commander)
    {
        Commander = commander;
    }

    public void GameOver()
    {
        if (Status == GameStatus.GameOver)
        {
            return;
        }

        ChangeStatus(GameStatus.GameOver);
        Time.timeScale = 0f;

        var resultUI = UIManager.Instance.ShowPopup<GameResultUI>("GameResultUI");
        if (resultUI != null)
        {
            resultUI.SetTitle("GAME OVER");
        }

        Debug.Log("게임 오버!");
    }

    public void GameVictory()
    {
        if (Status == GameStatus.Victory)
        {
            return;
        }

        ChangeStatus(GameStatus.Victory);
        Time.timeScale = 0f;

        var resultUI = UIManager.Instance.ShowPopup<GameResultUI>("GameResultUI");
        if (resultUI != null)
        {
            resultUI.SetTitle("VICTORY");
        }

        Debug.Log("모든 웨이브 클리어! 승리!");
    }

    public void RestartGame()
    {
        // 게임 시간 원래대로
        Time.timeScale = 1f;

        // UI를 먼저 모두 정리
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ClearAllUI();
        }

        // 모든 영속성 싱글톤 인스턴스 정리
        // 순서는 크게 중요하지 않지만 참조가 덜한 것부터 정리하는 것이 일반적
        ProbeManager.Cleanup();
        PoolManager.Cleanup();
        UIManager.Cleanup();
        GameManager.Cleanup(); // 자기 자신도 정리

        // 현재 씬을 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
