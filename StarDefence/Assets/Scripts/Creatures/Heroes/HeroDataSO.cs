using UnityEngine;

public enum AttackType
{
    Melee,
    Ranged
}

[CreateAssetMenu(fileName = "HeroData", menuName = "ScriptableObjects/HeroDataSO", order = 1)]
public class HeroDataSO : CreatureDataSO
{
    [Header("Info")]
    public string heroName;
    [TextArea] public string heroDescription;
    [Tooltip("영웅 프리팹 파일명 (확장자 제외)")]
    public string heroPrefabName;

    // 영웅 프리팹 경로
    public string FullHeroPrefabPath => Constants.HERO_ROOT_PATH + heroPrefabName;

    [Header("Stats")]
    public AttackType attackType;
    
    [Tooltip("투사체 프리팹 파일명 (원거리 영웅만 해당, 확장자 제외)")]
    public string projectilePrefabName;

    // 투사체 프리팹 경로
    public string FullProjectilePrefabPath => Constants.PROJECTILE_ROOT_PATH + projectilePrefabName;
}
