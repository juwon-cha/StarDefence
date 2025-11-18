using UnityEngine;

public class UI_Popup : UI_Base
{
    public override UIType UIType => UIType.Popup;

    public override void Initialize()
    {
        base.Initialize();
        // 모든 팝업 UI는 UIManager가 관리하는 팝업 캔버스의 자식으로 들어감
        transform.SetParent(UIManager.Instance.PopupCanvas.transform, false);
    }
}
