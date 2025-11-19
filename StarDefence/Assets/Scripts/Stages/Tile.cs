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
    public Hero PlacedHero { get; private set; }

    // A* Costs
    public int gCost; // 시작점에서 현재 타일까지의 비용
    public int hCost; // 현재 타일에서 목적지까지의 예상 비용
    
    // fCost = gCost + hCost
    public int fCost => gCost + hCost;

    // Path
    public Tile parent; // 이 타일로 오기 직전의 부모 타일
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // 좌클릭 시에만 이벤트 발생
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnTileClicked?.Invoke(this);
        }
    }

    public void SetTileData(string tileKey, bool isWalkable, Vector3 worldPosition, int gridX, int gridY)
    {
        this.IsWalkable = isWalkable;
        this.WorldPosition = worldPosition;
        this.GridX = gridX;
        this.GridY = gridY;

        IsPlaceable = (tileKey == "B"); // "B" 타일만 배치 가능
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
        IsPlaceable = true;
    }
}
