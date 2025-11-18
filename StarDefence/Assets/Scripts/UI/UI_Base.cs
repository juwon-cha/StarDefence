using UnityEngine;

public enum UIType
{
    Scene,
    Popup
}

public abstract class UI_Base : MonoBehaviour
{
    public abstract UIType UIType { get; }

    public virtual void Initialize()
    {
        // 기본 초기화 로직 (필요 시 자식 클래스에서 재정의)
    }
}
