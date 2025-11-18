using UnityEngine;

public class Tile : MonoBehaviour
{
    // Grid Info
    public bool isWalkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;

    // A* Costs
    public int gCost; // 시작점에서 현재 타일까지의 비용
    public int hCost; // 현재 타일에서 목적지까지의 예상 비용
    
    // fCost = gCost + hCost
    public int fCost => gCost + hCost;

    // Path
    public Tile parent; // 이 타일로 오기 직전의 부모 타일

    public void SetTileData(bool isWalkable, Vector3 worldPosition, int gridX, int gridY)
    {
        this.isWalkable = isWalkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}
