using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/EnemyDataSO", order = 1)]
public class EnemyDataSO : CreatureDataSO
{
    [Tooltip("적 프리팹 파일명 (확장자 제외)")]
    public string enemyPrefabName;

    public string FullEnemyPrefabPath => Constants.ENEMY_ROOT_PATH + enemyPrefabName;

    public float speed = 2f;
    // TODO: 다른 스탯(보상 등) 추가?
}
