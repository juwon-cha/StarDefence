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

    #region 유니티 생명주기
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

    #region 웨이브 관리
    public void PlayerStartsNextWave()
    {
        StartNextWaveFlow();
    }

    private void StartNextWaveFlow()
    {
        // 게임 상태 Wave로 변경하여 영웅들이 공격 시작
        GameManager.Instance.ChangeStatus(GameStatus.Wave);
        
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
        // 게임 상태 Build로 변경하여 다음 웨이브 전까지 영웅 배치 등을 할 수 있도록 함
        GameManager.Instance.ChangeStatus(GameStatus.Build);

        if (currentWaveIndex + 1 < waves.Count)
        {
            // 다음 웨이브 버튼 표시 (WorldSpaceCanvas에 띄움)
            NextWaveButtonUI button = UIManager.Instance.ShowWorldSpacePopup<NextWaveButtonUI>(Constants.NEXT_WAVE_BUTTON_UI_NAME);

            if (button != null)
            {
                // 스폰 지점 위쪽에 버튼을 위치시키기 위한 월드 좌표
                Vector3 spawnPointPos = GridManager.Instance.SpawnPoint.position;
                Vector3 worldPosAbove = spawnPointPos + new Vector3(0, 2f, 0); 

                // WorldSpaceCanvas의 자식이므로 월드 좌표를 직접 할당
                button.transform.position = worldPosAbove;
                
                button.Initialize(timeToPressButton, PlayerStartsNextWave);
            }
        }
        else
        {
            Debug.Log("All waves cleared!");
        }
    }
    #endregion

    #region 코루틴
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

                // PoolManager를 사용하여 몬스터 오브젝트를 가져옴
                GameObject enemyGO = PoolManager.Instance.Get(enemyInfo.enemyData.FullEnemyPrefabPath);
                if(enemyGO == null) continue;

                // 위치와 회전 초기화
                enemyGO.transform.position = spawnPoint.position;
                enemyGO.transform.rotation = Quaternion.identity;
                
                enemyGO.GetComponent<Enemy>().Initialize(enemyInfo.enemyData, endPoint);

                yield return new WaitForSeconds(currentWave.spawnInterval);
            }
        }

        isSpawning = false;
    }
    #endregion

    #region 이벤트 핸들러
    private void HandleEnemyDestroyed(Enemy enemy) // Enemy 인자 추가
    {
        enemiesAlive--;

        if (!isSpawning && enemiesAlive <= 0)
        {
            PrepareForNextWave();
        }
    }
    #endregion
}
