using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/EnemyDataSO", order = 1)]
public class EnemyDataSO : ScriptableObject
{
    public GameObject enemyPrefab;
    public float health = 100f;
    public float speed = 2f;
    // 필요하다면 다른 스탯 (e.g., 공격력, 보상 등) 추가
}
