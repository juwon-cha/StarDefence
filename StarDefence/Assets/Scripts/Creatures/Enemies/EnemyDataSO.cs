using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/EnemyDataSO", order = 1)]
public class EnemyDataSO : CreatureDataSO
{
    [Tooltip("적 프리팹 파일명 (확장자 제외)")]
    public string enemyPrefabName;

    public string FullEnemyPrefabPath => Constants.ENEMY_ROOT_PATH + enemyPrefabName;

    public float speed = 2f;
    [Tooltip("이 적을 처치했을 때 얻는 골드")]
    public int goldReward = 5;
}
