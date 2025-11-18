using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : Singleton<Pathfinding>
{
    private Tile[,] grid;
    private Bounds gridBounds;

    /// <summary>
    /// GridManager가 생성한 타일 그리드와 경계 정보 설정
    /// </summary>
    public void SetGrid(Tile[,] grid, Bounds gridBounds)
    {
        this.grid = grid;
        this.gridBounds = gridBounds;
    }

    /// <summary>
    /// 시작점에서 목적지까지의 경로 찾기
    /// </summary>
    /// <returns>경로 타일 리스트. 경로가 없으면 null 반환</returns>
    public List<Tile> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        if (grid == null)
        {
            Debug.LogError("[Pathfinding] Grid가 설정되지 않았습니다!");
            return null;
        }

        Tile startTile = TileFromWorldPoint(startPos);
        Tile targetTile = TileFromWorldPoint(targetPos);

        if (startTile == null || targetTile == null || !targetTile.isWalkable)
        {
            Debug.LogWarning("[Pathfinding] 시작 또는 목적지 타일을 찾을 수 없거나, 목적지가 이동 불가능한 지역입니다.");
            return null;
        }

        List<Tile> openSet = new List<Tile>();
        HashSet<Tile> closedSet = new HashSet<Tile>();
        openSet.Add(startTile);

        while (openSet.Count > 0)
        {
            Tile currentTile = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentTile.fCost || (openSet[i].fCost == currentTile.fCost && openSet[i].hCost < currentTile.hCost))
                {
                    currentTile = openSet[i];
                }
            }

            openSet.Remove(currentTile);
            closedSet.Add(currentTile);

            if (currentTile == targetTile)
            {
                return RetracePath(startTile, targetTile);
            }

            foreach (Tile neighbour in GetNeighbours(currentTile))
            {
                if (!neighbour.isWalkable || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentTile.gCost + GetDistance(currentTile, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetTile);
                    neighbour.parent = currentTile;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }

        return null; // 경로를 찾지 못함
    }

    /// <summary>
    /// 최종 경로를 역추적하여 리스트 만듦
    /// </summary>
    private List<Tile> RetracePath(Tile startTile, Tile endTile)
    {
        List<Tile> path = new List<Tile>();
        Tile currentTile = endTile;

        while (currentTile != startTile)
        {
            path.Add(currentTile);
            currentTile = currentTile.parent;
        }
        path.Reverse();
        return path;
    }

    /// <summary>
    /// 특정 타일의 이웃 타일(상하좌우)들 가져오기
    /// </summary>
    private List<Tile> GetNeighbours(Tile tile)
    {
        List<Tile> neighbours = new List<Tile>();
        int gridWidth = grid.GetLength(0);
        int gridHeight = grid.GetLength(1);

        // 상, 하, 좌, 우
        int[] xOffsets = { 0, 0, -1, 1 };
        int[] yOffsets = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int checkX = tile.gridX + xOffsets[i];
            int checkY = tile.gridY + yOffsets[i];

            if (checkX >= 0 && checkX < gridWidth && checkY >= 0 && checkY < gridHeight)
            {
                neighbours.Add(grid[checkX, checkY]);
            }
        }

        return neighbours;
    }

    /// <summary>
    /// 두 타일 사이의 거리(H 코스트) 계산(맨해튼 거리)
    /// </summary>
    private int GetDistance(Tile tileA, Tile tileB)
    {
        int dstX = Mathf.Abs(tileA.gridX - tileB.gridX);
        int dstY = Mathf.Abs(tileA.gridY - tileB.gridY);
        return 10 * (dstX + dstY);
    }

    /// <summary>
    /// 월드 좌표로부터 해당하는 타일 찾기
    /// </summary>
    public Tile TileFromWorldPoint(Vector3 worldPosition)
    {
        if (grid == null || grid.GetLength(0) == 0) return null;
        
        int gridWidth = grid.GetLength(0);
        int gridHeight = grid.GetLength(1);

        float percentX = Mathf.Clamp01((worldPosition.x - gridBounds.min.x) / gridBounds.size.x);
        float percentY = Mathf.Clamp01((worldPosition.y - gridBounds.min.y) / gridBounds.size.y);

        int x = Mathf.FloorToInt(gridWidth * percentX);
        int y = Mathf.FloorToInt(gridHeight * percentY);
        
        x = Mathf.Clamp(x, 0, gridWidth - 1);
        y = Mathf.Clamp(y, 0, gridHeight - 1);

        // GridManager의 생성 로직에 따라 월드 y좌표가 클수록 그리드 y좌표는 작아진다
        // 이 관계를 역으로 계산하여 정확한 그리드 y 인덱스를 찾는다
        int gridY = gridHeight - 1 - y;

        return grid[x, gridY];
    }
}
