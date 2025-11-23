using UnityEngine;

public class UI_Popup : UI_Base
{
    /// <summary>
    /// true일 경우 다른 팝업이 열릴 때 닫히지 않으며 이 팝업이 열릴 때 다른 팝업을 닫지 않음
    /// </summary>
    public bool IsIndependentPopup = false;
    
    public override UIType UIType => UIType.Popup;

    public override void Initialize()
    {
        base.Initialize();
    }
}
