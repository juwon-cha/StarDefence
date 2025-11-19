using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[System.Serializable]
public class TileMapping
{
    public string tileKey; // "S", "B", "X", "F", "H", "E"
    public GameObject tilePrefab; // 연결할 프리팹
}

public class GridManager : Singleton<GridManager>
{
    [Header("Map Data")]
    [SerializeField] private string mapFileName;

    [SerializeField] private List<TileMapping> tileMappings;

    public Transform SpawnPoint { get; private set; }
    public Transform EndPoint { get; private set; }
    
    private BoxCollider2D bgCollider;
    private Dictionary<string, GameObject> tilePrefabDict;
    private Tile[,] tileGrid; // 길찾기를 위한 타일 그리드

    protected override void Awake()
    {
        base.Awake();

        // 태그로 배경 오브젝트를 찾고 BoxCollider2D를 가져옴
        GameObject backgroundGO = GameObject.FindGameObjectWithTag("Background");
        if (backgroundGO == null)
        {
            Debug.LogError("[GridManager] 'Background' 태그를 가진 오브젝트를 찾을 수 없습니다!");
            return;
        }
        
        bgCollider = backgroundGO.GetComponent<BoxCollider2D>();
        if (bgCollider == null)
        {
            Debug.LogError($"[GridManager] 배경 오브젝트 '{backgroundGO.name}'에 BoxCollider2D가 없습니다!");
            return;
        }

        tilePrefabDict = new Dictionary<string, GameObject>();
        foreach (var mapping in tileMappings)
        {
            if (!tilePrefabDict.ContainsKey(mapping.tileKey))
            {
                tilePrefabDict.Add(mapping.tileKey, mapping.tilePrefab);
            }
        }

        GenerateMapFromFile();
    }

    void GenerateMapFromFile()
    {
        TextAsset textAsset = Resources.Load<TextAsset>($"MapData/{mapFileName}");
        if (textAsset == null)
        {
            Debug.LogError($"[GridManager] 맵 파일을 찾을 수 없습니다: MapData/{mapFileName}");
            return;
        }

        List<string[]> parsedRows = textAsset.text.Split('\n')
            .Where(row => !string.IsNullOrWhiteSpace(row) && !row.StartsWith("#"))
            .Select(row => row.Trim().Split(','))
            .ToList();

        if (parsedRows.Count == 0)
        {
            Debug.LogError("[GridManager] 맵 데이터가 비어있습니다.");
            return;
        }

        int gridHeight = parsedRows.Count;
        int gridWidth = parsedRows.Max(cols => cols.Length);
        tileGrid = new Tile[gridWidth, gridHeight];

        Bounds bounds = bgCollider.bounds;
        float cellWidth = bounds.size.x / gridWidth;
        float cellHeight = bounds.size.y / gridHeight;
        Vector3 startPos = bounds.min;

        for (int y = 0; y < gridHeight; y++)
        {
            string[] cols = parsedRows[y];
            for (int x = 0; x < cols.Length; x++)
            {
                string key = cols[x].Trim();

                float posX = startPos.x + x * cellWidth + (cellWidth / 2);
                float posY = startPos.y + (gridHeight - 1 - y) * cellHeight + (cellHeight / 2);
                Vector3 position = new Vector3(posX, posY, 0);

                GameObject tileGO;
                if (tilePrefabDict.TryGetValue(key, out GameObject prefab))
                {
                    tileGO = Instantiate(prefab, position, Quaternion.identity);
                }
                else
                {
                    tileGO = new GameObject($"Tile_{x}_{y}");
                    tileGO.transform.position = position;
                }
                
                tileGO.transform.SetParent(this.transform);
                
                Tile tileComponent = tileGO.AddComponent<Tile>();
                bool isWalkable = (key == "S" || key == "H" || key == "P");
                tileComponent.SetTileData(key, isWalkable, position, x, y);
                tileGrid[x, y] = tileComponent;

                SpriteRenderer sr = tileGO.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingOrder = y * 10;
                }

                if (key == "S")
                {
                    SpawnPoint = tileGO.transform;
                }
                else if (key == "H")
                {
                    EndPoint = tileGO.transform;
                }
            }
        }

        if (SpawnPoint == null) Debug.LogError("[GridManager] 맵에 스폰 지점(S)이 없습니다!");
        if (EndPoint == null) Debug.LogError("[GridManager] 맵에 목표 지점(E)이 없습니다!");

        // 생성된 그리드와 경계 정보를 Pathfinding에 전달
        Pathfinding.Instance.SetGrid(tileGrid, bgCollider.bounds);
    }
}
