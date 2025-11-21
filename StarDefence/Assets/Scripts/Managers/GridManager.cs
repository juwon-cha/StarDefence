using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[System.Serializable]
public class TileMapping
{
    // "S(적 스폰)", "B(배치 가능한 타일)", "X(배치 불가 타일)", "F(수리 가능 타일)", "H(지휘관 스폰)"
    public string tileKey;
    public GameObject tilePrefab; // 연결할 프리팹
}

public class GridManager : Singleton<GridManager>
{
    [Header("Map Data")]
    [SerializeField] private string mapFileName;
    [SerializeField] private List<TileMapping> tileMappings;

    [Header("Buff Tile Settings")]
    [SerializeField] private int numberOfBuffTiles = 3; // 생성할 강화 타일의 수
    [SerializeField] private List<BuffDataSO> possibleBuffs; // 사용 가능한 버프 목록

    public Transform SpawnPoint { get; private set; }
    public Transform EndPoint { get; private set; }
    
    private BoxCollider2D bgCollider;
    private Dictionary<string, GameObject> tilePrefabDict;
    private Dictionary<string, Sprite> tileSpriteCache;
    private Tile[,] tileGrid; // 길찾기를 위한 타일 그리드

    protected override void Awake()
    {
        base.Awake();

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
        
        tileSpriteCache = new Dictionary<string, Sprite>();

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

        Pathfinding.Instance.SetGrid(tileGrid, bgCollider.bounds);
        
        GenerateBuffTiles(); // 맵 생성 후 강화 타일 생성
    }

    /// <summary>
    /// 무작위로 배치 가능한 타일 중 일부에 버프 적용
    /// </summary>
    void GenerateBuffTiles()
    {
        if (possibleBuffs == null || possibleBuffs.Count == 0)
        {
            Debug.LogWarning("[GridManager] 적용할 버프가 없습니다. 'Possible Buffs' 리스트를 확인하세요.");
            return;
        }

        // 모든 배치 가능한("B") 타일을 찾음
        List<Tile> placeableTiles = new List<Tile>();
        foreach (var tile in tileGrid)
        {
            if (tile != null && tile.IsPlaceable)
            {
                placeableTiles.Add(tile);
            }
        }

        if (placeableTiles.Count < numberOfBuffTiles)
        {
            Debug.LogWarning($"[GridManager] 버프를 적용할 타일이 부족합니다. 필요한 타일: {numberOfBuffTiles}, 배치 가능한 타일: {placeableTiles.Count}");
            numberOfBuffTiles = placeableTiles.Count;
        }

        // 타일 리스트를 무작위로 섞음
        var shuffledTiles = placeableTiles.OrderBy(t => Random.value).ToList();

        // 정해진 수만큼의 타일에 버프 적용
        for (int i = 0; i < numberOfBuffTiles; i++)
        {
            Tile tileToBuff = shuffledTiles[i];
            
            // 사용 가능한 버프 중 하나를 무작위로 선택
            BuffDataSO randomBuff = possibleBuffs[Random.Range(0, possibleBuffs.Count)];
            
            tileToBuff.SetBuff(randomBuff);
        }
    }

    public Sprite GetSpriteForTileKey(string key)
    {
        if (tileSpriteCache.TryGetValue(key, out Sprite sprite))
        {
            return sprite;
        }
        
        if (tilePrefabDict.TryGetValue(key, out GameObject prefab))
        {
            SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                tileSpriteCache.Add(key, sr.sprite);
                return sr.sprite;
            }
        }

        Debug.LogWarning($"[GridManager] '{key}' 키에 해당하는 타일 프리팹이나 SpriteRenderer를 찾을 수 없습니다.");
        return null;
    }
}
