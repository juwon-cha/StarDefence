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
        // 빌드 모드가 아니거나 타일이 유효하지 않으면 아무것도 하지 않음
        if (Status != GameStatus.Build)
        {
            return;
        }
        if (tile == null || !tile.IsPlaceable || tile.PlacedHero != null)
        {
            return;
        }
        
        // 새로운 범용 ShowPopup 메서드 사용
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

        if (heroData.heroPrefab == null)
        {
            Debug.LogError($"Selected hero '{heroData.heroName}' has no prefab!");
            return;
        }

        // Instantiate and initialize the hero
        GameObject heroGO = Instantiate(heroData.heroPrefab, tile.transform.position, Quaternion.identity);
        Hero hero = heroGO.GetComponent<Hero>();
        hero.Init(heroData, tile);

        // Update the tile
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