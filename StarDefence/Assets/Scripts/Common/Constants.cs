using UnityEngine;

/// <summary>
/// 프로젝트 전반에 사용되는 상수들을 관리하는 클래스
/// </summary>
public static class Constants
{
    // Prefab Root Paths
    // 경로는 Resources폴더를 기준으로 한다. 프리팹 카테고리의 루트 경로 지정
    // 최종 프리팹 경로는 이 RootPath와 ScriptableObject에 저장된 프리팹 파일 이름(확장자 제외)을 조합하여 생성
    public const string HERO_ROOT_PATH = "Prefabs/Creatures/Heroes/";
    public const string ENEMY_ROOT_PATH = "Prefabs/Creatures/Enemies/";
    public const string PROJECTILE_ROOT_PATH = "Prefabs/Projectiles/";
    
    // Specific Prefab Names
    // 각 프리팹의 파일명(확장자 제외). 이 이름들은 ScriptableObject에 할당될 때 사용
    // 반드시 실제 프리팹 파일명과 동일하게 변경!
    public const string ENEMY_PREFAB_NAME = "Monster";
    public const string MELEE_HERO_PREFAB_NAME = "MeleeHero";
    public const string RANGED_HERO_PREFAB_NAME = "RangedHero";
    public const string PROJECTILE_PREFAB_NAME = "HeroProjectile";

    // 기타 상수들
}
