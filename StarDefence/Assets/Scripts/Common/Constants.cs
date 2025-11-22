using UnityEngine;

public static class Constants
{
    // 프리팹 루트 경로
    // 경로는 Resources 폴더를 기준으로 하고 프리팹 카테고리의 루트 경로를 지정
    // 최종 프리팹 경로는 이 RootPath와 ScriptableObject에 저장된 프리팹 파일 이름(확장자 제외)을 조합하여 생성
    public const string COMMANDER_ROOT_PATH = "Prefabs/Creatures/Commander/";
    public const string HERO_ROOT_PATH = "Prefabs/Creatures/Heroes/";
    public const string ENEMY_ROOT_PATH = "Prefabs/Creatures/Enemies/";
    public const string PROBE_ROOT_PATH = "Prefabs/Creatures/Probes/";
    public const string PROJECTILE_ROOT_PATH = "Prefabs/Projectiles/";
    public const string UI_ROOT_PATH = "Prefabs/UI/";
    public const string UI_POPUP_SUB_PATH = "Popup/";
    public const string UI_SCENE_SUB_PATH = "Scene/";
    
    // UI 프리팹 이름
    public const string HEALTH_BAR_UI_PREFAB_NAME = "HealthBarUI";
    public const string HUD_UI_NAME = "HUD";
    public const string NEXT_WAVE_BUTTON_UI_NAME = "NextWaveButtonUI";
    public const string PLACE_HERO_CONFIRM_UI_NAME = "PlaceHeroConfirmUI";
    public const string PROBE_PURCHASE_UI_NAME = "ProbePurchaseUI";
    public const string BOUNTY_POPUP_UI_NAME = "BountyPopupUI";
    public const string UPGRADE_POPUP_UI_NAME = "UpgradePopupUI";

    // 기타 상수들
}
