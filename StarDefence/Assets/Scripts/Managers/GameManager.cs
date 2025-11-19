using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameStatus
{
    Ready,
    Build,
    Wave,
    GameOver,
}

public class GameManager : Singleton<GameManager>
{
    [Header("Hero Data")]
    public List<HeroDataSO> tier1Heroes;

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
        // 영웅을 배치할 수 있는 빌드 모드로 전환
        ChangeStatus(GameStatus.Build);
    }
    
    // 타일이 클릭되었을 때 호출될 이벤트 핸들러
    private void HandleTileClicked(Tile tile)
    {
        // 타일이 유효하지 않으면 아무것도 하지 않음
        if (tile == null || !tile.IsPlaceable || tile.PlacedHero != null)
        {
            return;
        }
        
        var confirmUI = UIManager.Instance.ShowPopup<PlaceHeroConfirmUI>();
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

        // heroData.FullHeroPrefabPath가 비어있는지 체크
        if (string.IsNullOrEmpty(heroData.FullHeroPrefabPath))
        {
            Debug.LogError($"Selected hero '{heroData.heroName}' has no valid prefab path in its HeroDataSO!");
            return;
        }

        // PoolManager를 사용하여 영웅 오브젝트를 가져옴 (완전한 경로 사용)
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
}