using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    // 타일 클릭 시 호출될 정적 이벤트 정의
    public static event System.Action<Tile> OnTileClicked;

    // Grid Info
    public bool IsWalkable;
    public Vector3 WorldPosition;
    public int GridX;
    public int GridY;

    // 영웅 배치
    public bool IsPlaceable { get; private set; }
    public bool IsFixable { get; private set; }
    public Hero PlacedHero { get; private set; }

    private string tileKey;
    private SpriteRenderer spriteRenderer;

    // A* Costs
    public int gCost; // 시작점에서 현재 타일까지의 비용
    public int hCost; // 현재 타일에서 목적지까지의 예상 비용
    
    // fCost = gCost + hCost
    public int fCost => gCost + hCost;

    // Path
    public Tile parent; // 이 타일로 오기 직전의 부모 타일

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 좌클릭 시에만 이벤트 발생
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnTileClicked?.Invoke(this);
        }
    }

    public void SetTileData(string key, bool isWalkable, Vector3 worldPosition, int gridX, int gridY)
    {
        this.tileKey = key;
        this.IsWalkable = isWalkable;
        this.WorldPosition = worldPosition;
        this.GridX = gridX;
        this.GridY = gridY;

        IsPlaceable = (tileKey == "B"); // "B" 타일만 배치 가능
        IsFixable = (tileKey == "F");   // "F" 타일은 수리 가능
    }

    /// <summary>
    /// 타일을 수리하여 배치 가능한 상태로 만들기
    /// </summary>
    public void Repair()
    {
        if (!IsFixable) return;

        IsFixable = false;
        IsPlaceable = true;
        tileKey = "B"; // 내부 상태도 변경

        // 시각적으로 수리되었음을 표시하기 위해 스프라이트를 'B'타일의 것으로 교체
        if (spriteRenderer != null)
        {
            Sprite placeableSprite = GridManager.Instance.GetSpriteForTileKey("B");
            if (placeableSprite != null)
            {
                spriteRenderer.sprite = placeableSprite;
            }
        }
    }
    
    public void SetHero(Hero hero)
    {
        if (hero == null)
        {
            ClearHero();
            return;
        }

        PlacedHero = hero;
        IsPlaceable = false;
    }

    public void ClearHero()
    {
        PlacedHero = null;
        
        // 수리된 타일은 영웅이 사라져도 계속 배치 가능 상태를 유지해야 함
        IsPlaceable = (tileKey == "B");
    }
}
