using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    [Header("Commander Data")]
    public List<CommanderDataSO> commanders;

    [Header("Hero Data")]
    public List<HeroDataSO> tier1Heroes;

    public Commander Commander { get; private set; }

    public GameStatus Status { get; private set; }
    public event System.Action<GameStatus> OnStatusChanged;

    protected override void Awake()
    {
        base.Awake();
        Status = GameStatus.Ready;
    }

    private void OnEnable()
    {
        Tile.OnTileClicked += HandleTileClicked;
    }

    private void OnDisable()
    {
        Tile.OnTileClicked -= HandleTileClicked;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        SpawnCommander();
        ChangeStatus(GameStatus.Build);
    }
    
    private void SpawnCommander()
    {
        if (commanders == null || !commanders.Any())
        {
            Debug.LogError("생성할 수 있는 지휘관이 없습니다.");
            return;
        }

        CommanderDataSO commanderData = commanders[0]; // 기본적으로 첫 번째 지휘관 생성
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

    // 타일이 클릭되었을 때 호출될 이벤트 핸들러
    private void HandleTileClicked(Tile tile)
    {
        // 타일이 유효하지 않으면 아무것도 하지 않음
        if (tile == null || !tile.IsPlaceable || tile.PlacedHero != null)
        {
            return;
        }
        
        var confirmUI = UIManager.Instance.ShowWorldSpacePopup<PlaceHeroConfirmUI>(Constants.PLACE_HERO_CONFIRM_UI_NAME);
        if (confirmUI != null)
        {
            confirmUI.SetData(tile);
        }
    }

    public void ConfirmPlaceHero(Tile tile)
    {
        if (!tier1Heroes.Any())
        {
            Debug.LogWarning("Tier 1 Heroes list is empty! Cannot place hero.");
            return;
        }

        HeroDataSO heroData = tier1Heroes[Random.Range(0, tier1Heroes.Count)];

        if (string.IsNullOrEmpty(heroData.FullHeroPrefabPath))
        {
            Debug.LogError($"Selected hero '{heroData.heroName}' has no valid prefab path in its HeroDataSO!");
            return;
        }

        GameObject heroGO = PoolManager.Instance.Get(heroData.FullHeroPrefabPath);
        if (heroGO == null) return;
        
        // 위치와 회전 초기화
        heroGO.transform.position = tile.transform.position;
        heroGO.transform.rotation = Quaternion.identity;

        // 영웅 초기화
        Hero hero = heroGO.GetComponent<Hero>();
        hero.Init(heroData, tile);

        // 타일에 영웅 배치 정보 설정
        tile.SetHero(hero);

        Debug.Log($"{heroData.heroName} placed!");
    }

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
            return; // 중복 호출 방지
        }

        ChangeStatus(GameStatus.GameOver);
        Time.timeScale = 0f; // 게임 일시정지
        
        var resultUI = UIManager.Instance.ShowPopup<GameResultUI>("GameResultUI");
        if (resultUI != null)
        {
            resultUI.SetTitle("GAME OVER");
        }
        Debug.Log("Game Over!");
    }

    public void GameVictory()
    {
        if (Status == GameStatus.Victory)
        {
            return;
        }

        ChangeStatus(GameStatus.Victory);
        Time.timeScale = 0f; // 게임 일시정지

        var resultUI = UIManager.Instance.ShowPopup<GameResultUI>("GameResultUI");
        if (resultUI != null)
        {
            resultUI.SetTitle("VICTORY");
        }
        Debug.Log("All waves cleared! Victory!");
    }
}