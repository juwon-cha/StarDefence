using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : Singleton<WaveManager>
{
    [Header("Wave Settings")]
    [Tooltip("이 스테이지에서 진행할 웨이브 목록")]
    public List<WaveDataSO> waves;
    [Tooltip("다음 웨이브 시작 버튼을 누를 수 있는 제한 시간")]
    public float timeToPressButton = 15f;

    private int currentWaveIndex = -1;
    private int enemiesAlive = 0;
    private bool isSpawning = false;

    #region Unity Lifecycle
    void OnEnable()
    {
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    void OnDisable()
    {
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    void Start()
    {
        StartCoroutine(WaitForHUDAndStart());
    }
    #endregion

    #region Wave Flow
    public void PlayerStartsNextWave()
    {
        StartNextWaveFlow();
    }

    private void StartNextWaveFlow()
    {
        currentWaveIndex++;
        if (currentWaveIndex < waves.Count)
        {
            if (UIManager.Instance.MainHUD != null)
                UIManager.Instance.MainHUD.UpdateWaveText(currentWaveIndex, waves.Count);
            
            StartCoroutine(SpawnWave(waves[currentWaveIndex]));
        }
        else
        {
            if (UIManager.Instance.MainHUD != null)
                UIManager.Instance.MainHUD.ShowAllWavesCleared();
            
            Debug.Log("All waves cleared!");
        }
    }

    private void PrepareForNextWave()
    {
        if (currentWaveIndex + 1 < waves.Count)
        {
            // UIManager에게 다음 웨이브 버튼 표시를 요청
            NextWaveButtonUI button = UIManager.Instance.ShowSceneUI<NextWaveButtonUI>("NextWaveButtonUI");

            if (button != null)
            {
                // 스폰 지점 위쪽에 버튼을 위치시키기 위한 좌표 변환
                Vector3 spawnPointPos = GridManager.Instance.SpawnPoint.position;
                Vector3 worldPosAbove = spawnPointPos + new Vector3(0, 2f, 0); // 월드 좌표 기준 Y축으로 2 유닛 위
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosAbove);

                if (screenPos.z > 0) // 카메라 앞에 있을 때만 위치 설정
                {
                    button.transform.position = screenPos;
                }
                
                button.Initialize(timeToPressButton, PlayerStartsNextWave);
            }
        }
        else
        {
            Debug.Log("All waves cleared!");
        }
    }
    #endregion

    #region Coroutines
    private IEnumerator WaitForHUDAndStart()
    {
        yield return new WaitUntil(() => UIManager.Instance != null && UIManager.Instance.MainHUD != null);

        // 첫 웨이브 정보 표시 후 다음 웨이브 준비
        if (currentWaveIndex + 1 < waves.Count)
        {
            UIManager.Instance.MainHUD.UpdateWaveText(currentWaveIndex + 1, waves.Count);
        }
        PrepareForNextWave();
    }

    private IEnumerator SpawnWave(WaveDataSO currentWave)
    {
        isSpawning = true;
        enemiesAlive = 0;

        foreach (var enemyInfo in currentWave.enemiesToSpawn)
        {
            enemiesAlive += enemyInfo.count;
            for (int i = 0; i < enemyInfo.count; i++)
            {
                Transform spawnPoint = GridManager.Instance.SpawnPoint;
                Transform endPoint = GridManager.Instance.EndPoint;

                if (spawnPoint == null || endPoint == null)
                {
                    Debug.LogError("스폰 위치 또는 목표 위치가 GridManager에 설정되지 않았습니다.");
                    yield break;
                }

                GameObject enemyGO = Instantiate(enemyInfo.enemyData.enemyPrefab, spawnPoint.position, Quaternion.identity);
                enemyGO.GetComponent<Enemy>().Initialize(enemyInfo.enemyData, endPoint);

                yield return new WaitForSeconds(currentWave.spawnInterval);
            }
        }

        isSpawning = false;
    }
    #endregion

    #region Event Handlers
    private void HandleEnemyDestroyed()
    {
        enemiesAlive--;

        if (!isSpawning && enemiesAlive <= 0)
        {
            PrepareForNextWave();
        }
    }
    #endregion
}
