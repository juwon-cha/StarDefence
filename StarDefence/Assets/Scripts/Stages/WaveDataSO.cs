using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "ScriptableObjects/WaveDataSO", order = 2)]
public class WaveDataSO : ScriptableObject
{
    [System.Serializable]
    public struct EnemySpawnInfo
    {
        public EnemyDataSO enemyData;
        public int count;
    }

    [Header("Wave Composition")]
    public List<EnemySpawnInfo> enemiesToSpawn;

    [Header("Wave Timings")]
    [Tooltip("이 웨이브 내에서 각 적이 스폰되는 시간 간격 (초)")]
    public float spawnInterval = 0.5f;

    [Tooltip("이 웨이브가 끝난 후 다음 웨이브가 시작되기까지 대기 시간 (초)")]
    public float timeToNextWave = 5.0f;
}
