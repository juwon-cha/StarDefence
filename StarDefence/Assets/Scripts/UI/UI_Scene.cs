using UnityEngine;

public class UI_Scene : UI_Base
{
    public override UIType UIType => UIType.Scene;

    // Scene UI는 World Space에 그려지거나 특정 씬에 종속되므로
    // 팝업처럼 부모를 강제로 지정할 필요가 없음
    // 개별 UI 스크립트에서 필요한 로직 구현
}
