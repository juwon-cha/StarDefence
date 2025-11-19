using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    // 데이터
    /// <summary>key: 프리팹 경로, value: 해당 프리팹의 비활성 오브젝트들을 담는 스택</summary>
    private Dictionary<string, Stack<GameObject>> pools = new Dictionary<string, Stack<GameObject>>();
    
    /// <summary>key: 프리팹 경로, value: 로드된 원본 프리팹</summary>
    private Dictionary<string, GameObject> loadedPrefabs = new Dictionary<string, GameObject>();

    /// <summary>key: 활성화된 오브젝트의 고유 Instance ID, value: 해당 오브젝트의 프리팹 경로</summary>
    private Dictionary<int, string> activeObjectPrefabPath = new Dictionary<int, string>();
    
    // 하이어라키 관리용 컨테이너
    /// <summary>모든 풀 컨테이너들을 담을 최상위 부모 트랜스폼</summary>
    private Transform poolRootContainer;
    
    /// <summary>key: 프리팹 경로, value: 해당 프리팹의 오브젝트들을 담는 컨테이너 트랜스폼</summary>
    private Dictionary<string, Transform> containers = new Dictionary<string, Transform>();
    
    /// <summary>애플리케이션 종료 여부를 확인하는 플래그</summary>
    private bool isShuttingDown = false;

    protected override void Awake()
    {
        base.Awake();
        InitializePoolContainer();
    }
    
    // 게임 종료 시점에 호출되어 종료 중임을 알림
    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        isShuttingDown = true;
    }

    private void InitializePoolContainer()
    {
        poolRootContainer = new GameObject("PoolRoot").transform;
        poolRootContainer.SetParent(transform);
    }
    
    /// <summary>
    /// 풀에서 오브젝트 가져오고 없으면 새로 생성
    /// </summary>
    /// <param name="prefabPath">Resources 폴더 기준의 프리팹 경로</param>
    public GameObject Get(string prefabPath)
    {
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError("[PoolManager] Prefab path is null or empty.");
            return null;
        }

        if (!pools.ContainsKey(prefabPath))
        {
            pools.Add(prefabPath, new Stack<GameObject>());
        }

        var container = GetOrCreateContainer(prefabPath);
        
        GameObject obj;
        if (pools[prefabPath].Count > 0)
        {
            obj = pools[prefabPath].Pop();
        }
        else
        {
            if (!loadedPrefabs.ContainsKey(prefabPath))
            {
                var prefab = Resources.Load<GameObject>(prefabPath);
                if (prefab == null)
                {
                    Debug.LogError($"[PoolManager] Failed to load prefab from path: {prefabPath}");
                    return null;
                }
                loadedPrefabs.Add(prefabPath, prefab);
            }
            
            obj = Instantiate(loadedPrefabs[prefabPath]);
            obj.name = loadedPrefabs[prefabPath].name; // 오브젝트 이름도 원본 프리팹과 동일하게 설정
            obj.transform.SetParent(container);
        }
        
        obj.SetActive(true);
        activeObjectPrefabPath.Add(obj.GetInstanceID(), prefabPath);
        
        return obj;
    }

    /// <summary>
    /// 사용한 오브젝트를 풀에 반납
    /// </summary>
    /// <param name="obj">반납할 오브젝트</param>
    public void Release(GameObject obj)
    {
        if (obj == null) return;
        
        int instanceID = obj.GetInstanceID();
        
        if (!activeObjectPrefabPath.TryGetValue(instanceID, out string prefabPath))
        {
            Debug.LogWarning($"[PoolManager] This object '{obj.name}' is not managed by the PoolManager.");
            return;
        }
        
        activeObjectPrefabPath.Remove(instanceID);
        
        obj.SetActive(false);
        pools[prefabPath].Push(obj);
    }

    public void ClearAllPools()
    {
        if (poolRootContainer != null)
        {
            Destroy(poolRootContainer.gameObject);
        }
        
        pools.Clear();
        activeObjectPrefabPath.Clear();
        containers.Clear();
        loadedPrefabs.Clear();
        
        if (!isShuttingDown)
        {
            InitializePoolContainer();
        }
    }

    private Transform GetOrCreateContainer(string prefabPath)
    {
        if (!containers.TryGetValue(prefabPath, out Transform container))
        {
            // 경로에서 파일 이름만 추출하여 컨테이너 이름으로 사용
            string containerName = System.IO.Path.GetFileNameWithoutExtension(prefabPath) + "_Pool";
            container = new GameObject(containerName).transform;
            container.SetParent(poolRootContainer);
            containers.Add(prefabPath, container);
        }
        return container;
    }
}
