using UnityEngine;

public enum AttackType
{
    Melee,
    Ranged
}

[CreateAssetMenu(fileName = "HeroData", menuName = "ScriptableObjects/HeroDataSO", order = 1)]
public class HeroDataSO : ScriptableObject
{
    [Header("Info")]
    public string heroName;
    [TextArea] public string heroDescription;
    public GameObject heroPrefab;

    [Header("Stats")]
    public AttackType attackType;
    public float attackRange = 2f;
    public float attackInterval = 1f;
    public int damage = 10;
}
