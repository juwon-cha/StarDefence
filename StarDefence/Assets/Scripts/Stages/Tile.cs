using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    // 정적 이벤트
    public static event System.Action<Tile> OnTileClicked;

    // Grid Info
    public bool IsWalkable;
    public Vector3 WorldPosition;
    public int GridX;
    public int GridY;

    // 영웅 배치 관련
    public bool IsPlaceable { get; private set; }
    public bool IsFixable { get; private set; }
    public Hero PlacedHero { get; private set; }

    // 버프 관련
    public BuffDataSO CurrentBuff { get; private set; }
    private Color originalColor;

    private string tileKey;
    private SpriteRenderer spriteRenderer;

    // A* Costs
    public int gCost;
    public int hCost; 
    public int fCost => gCost + hCost;

    // Path
    public Tile parent; 

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color; // 원래 색상 저장
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
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

        IsPlaceable = (tileKey == "B");
        IsFixable = (tileKey == "F");
    }

    /// <summary>
    /// 타일에 버프를 설정하고 시각적으로 표시
    /// </summary>
    public void SetBuff(BuffDataSO buff)
    {
        CurrentBuff = buff;
        if (spriteRenderer != null)
        {
            // 버프가 있으면 해당 색상으로, 없으면 원래 색상으로 변경
            spriteRenderer.color = (buff != null) ? buff.tileColor : originalColor;
        }
    }

    /// <summary>
    /// 타일을 수리하여 배치 가능한 상태로 만들기
    /// </summary>
    public void Repair()
    {
        if (!IsFixable) return;

        IsFixable = false;
        IsPlaceable = true;
        tileKey = "B";

        if (spriteRenderer != null)
        {
            Sprite placeableSprite = GridManager.Instance.GetSpriteForTileKey("B");
            if (placeableSprite != null)
            {
                spriteRenderer.sprite = placeableSprite;
            }
        }
    }
    
    /// <summary>
    /// 타일에 영웅을 배치하거나 제거하고 버프 적용/제거
    /// </summary>
    public void SetHero(Hero hero)
    {
        // 영웅 제거
        if (hero == null)
        {
            if (PlacedHero != null && CurrentBuff != null)
            {
                PlacedHero.RemoveBuff(CurrentBuff);
            }
            ClearHero();
        }
        // 영웅 배치
        else
        {
            PlacedHero = hero;
            IsPlaceable = false;
            
            if (CurrentBuff != null)
            {
                PlacedHero.ApplyBuff(CurrentBuff);
            }
        }
    }

    public void ClearHero()
    {
        PlacedHero = null;
        IsPlaceable = (tileKey == "B");
    }
}
