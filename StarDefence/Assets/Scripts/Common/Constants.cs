using UnityEngine;

public static class Constants
{
    // Prefab Root Paths
    // 경로는 Resources 폴더를 기준으로 하고 프리팹 카테고리의 루트 경로를 지정
    // 최종 프리팹 경로는 이 RootPath와 ScriptableObject에 저장된 프리팹 파일 이름(확장자 제외)을 조합하여 생성
    public const string HERO_ROOT_PATH = "Prefabs/Creatures/Heroes/";
    public const string ENEMY_ROOT_PATH = "Prefabs/Creatures/Enemies/";
    public const string PROJECTILE_ROOT_PATH = "Prefabs/Projectiles/";
    public const string UI_ROOT_PATH = "Prefabs/UI/";
    public const string UI_POPUP_SUB_PATH = "Popup/";
    public const string UI_SCENE_SUB_PATH = "Scene/";

    // Specific Prefab Names
    // 각 프리팹의 파일명 (확장자 제외)
    // 반드시 실제 프리팹 파일명과 동일하게 변경
    public const string ENEMY_PREFAB_NAME = "Monster";
    public const string MELEE_HERO_PREFAB_NAME = "MeleeHero";
    public const string RANGED_HERO_PREFAB_NAME = "RangedHero";
    public const string PROJECTILE_PREFAB_NAME = "HeroProjectile";
    
    // UI Prefab Names
    public const string HEALTH_BAR_UI_PREFAB_NAME = "HealthBarUI";
    public const string HUD_UI_NAME = "HUD";
    public const string NEXT_WAVE_BUTTON_UI_NAME = "NextWaveButtonUI";
    public const string PLACE_HERO_CONFIRM_UI_NAME = "PlaceHeroConfirmUI";

    // 기타 상수들
}
