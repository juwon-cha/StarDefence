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
    [Tooltip("영웅 프리팹 파일명 (확장자 제외)")]
    public string heroPrefabName;

    // 완전한 영웅 프리팹 경로
    public string FullHeroPrefabPath => Constants.HERO_ROOT_PATH + heroPrefabName;

    [Header("Stats")]
    public AttackType attackType;
    public float attackRange = 2f;
    public float attackInterval = 1f;
    public int damage = 10;
    
    [Tooltip("투사체 프리팹 파일명 (원거리 영웅만 해당, 확장자 제외)")]
    public string projectilePrefabName;

    // 완전한 투사체 프리팹 경로
    public string FullProjectilePrefabPath => Constants.PROJECTILE_ROOT_PATH + projectilePrefabName;
}
