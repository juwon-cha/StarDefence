using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[System.Serializable]
public class TileMapping
{
    public string tileKey; // "S", "B", "X", "F", "H"
    public GameObject tilePrefab; // 연결할 프리팹
}

public class GridManager : Singleton<GridManager>
{
    [Header("Map Settings")]
    [SerializeField] private GameObject background;

    [Header("Map Data")]
    [SerializeField] private string mapFileName;

    [SerializeField] private List<TileMapping> tileMappings;

    public Transform SpawnPoint => spawnPoint;
    private Transform spawnPoint;
    private BoxCollider2D bgCollider;

    // 프리팹을 빠르게 찾기 위한 딕셔너리
    private Dictionary<string, GameObject> tilePrefabDict;

    protected override void Awake()
    {
        base.Awake();

        // 배경 및 콜라이더 유효성 검사
        if (background == null)
        {
            Debug.LogError("[GridManager] 배경(background) 오브젝트가 할당되지 않았습니다!");
            return;
        }
        bgCollider = background.GetComponent<BoxCollider2D>();
        if (bgCollider == null)
        {
            Debug.LogError($"[GridManager] 배경 오브젝트 '{background.name}'에 BoxCollider2D가 없습니다!");
            return;
        }

        // 프리팹 딕셔너리 초기화
        tilePrefabDict = new Dictionary<string, GameObject>();
        foreach (var mapping in tileMappings)
        {
            if (!tilePrefabDict.ContainsKey(mapping.tileKey))
            {
                tilePrefabDict.Add(mapping.tileKey, mapping.tilePrefab);
            }
        }

        // 맵 생성
        GenerateMapFromFile();
    }

    /// <summary>
    /// CSV 파일을 읽고 배경의 BoxCollider2D에 맞춰 타일 동적 생성
    /// </summary>
    void GenerateMapFromFile()
    {
        // 맵 데이터 로드
        TextAsset textAsset = Resources.Load<TextAsset>($"MapData/{mapFileName}");
        if (textAsset == null)
        {
            Debug.LogError($"[GridManager] 맵 파일을 찾을 수 없습니다: MapData/{mapFileName}");
            return;
        }

        // CSV 데이터 파싱 및 그리드 크기 결정
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

        // 그리드 셀 크기 및 시작 위치 계산
        Bounds bounds = bgCollider.bounds;
        float cellWidth = bounds.size.x / gridWidth;
        float cellHeight = bounds.size.y / gridHeight;
        Vector3 startPos = bounds.min; // 경계의 좌측 하단

        // 타일 생성
        for (int y = 0; y < gridHeight; y++)
        {
            string[] cols = parsedRows[y];
            for (int x = 0; x < cols.Length; x++)
            {
                string key = cols[x].Trim();

                if (tilePrefabDict.TryGetValue(key, out GameObject prefab))
                {
                    // 각 셀의 중심 위치 계산
                    // CSV의 (0,0)은 좌상단, Unity 좌표계의 (0,0)은 좌하단이므로 y좌표 보정
                    float posX = startPos.x + x * cellWidth + (cellWidth / 2);
                    float posY = startPos.y + (gridHeight - 1 - y) * cellHeight + (cellHeight / 2);
                    Vector3 position = new Vector3(posX, posY, 0);

                    GameObject tileGO = Instantiate(prefab, position, Quaternion.identity);
                    tileGO.transform.SetParent(this.transform);

                    // 타일의 y 위치에 따라 렌더링 순서 동적 지정
                    SpriteRenderer sr = tileGO.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        // 행(y) 번호가 클수록 (아래에 있을수록) 앞에 그려지도록 설정
                        sr.sortingOrder = y * 10;
                    }

                    // 스폰 지점(S)인 경우 위치 저장
                    if (key == "S")
                    {
                        spawnPoint = tileGO.transform;
                    }
                }
            }
        }

        if (spawnPoint == null)
        {
            Debug.LogError("[GridManager] 맵에 스폰 지점(S)이 없습니다!");
        }
    }
}
