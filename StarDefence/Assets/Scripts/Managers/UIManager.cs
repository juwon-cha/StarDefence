using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public Canvas MainCanvas { get; private set; }
    public Canvas PopupCanvas { get; private set; }

    // 활성화된 팝업의 순서를 관리하는 스택
    private readonly Stack<UI_Popup> popupStack = new Stack<UI_Popup>();
    // 생성된 팝업 인스턴스를 재사용하기 위한 캐시
    private readonly Dictionary<Type, UI_Popup> popupCache = new Dictionary<Type, UI_Popup>();
    // 리소스에서 로드한 프리팹을 캐싱
    private readonly Dictionary<string, UI_Base> prefabCache = new Dictionary<string, UI_Base>();
    // 활성화된 씬 UI를 캐싱
    private readonly Dictionary<string, UI_Scene> sceneCache = new Dictionary<string, UI_Scene>();

    public HUD MainHUD { get; private set; }

    #region 초기화
    protected override void Awake()
    {
        base.Awake();
        EnsureEventSystem();
        CreateMainCanvas();
        CreatePopupCanvas();
    }

    void Start()
    {
        MainHUD = ShowSceneUI<HUD>("HUD");
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }

    private void CreateMainCanvas()
    {
        GameObject canvasGO = new GameObject { name = "MainCanvas" };
        MainCanvas = canvasGO.AddComponent<Canvas>();
        MainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        MainCanvas.sortingOrder = 0;
        
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(300, 600);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        canvasGO.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasGO);
    }

    private void CreatePopupCanvas()
    {
        GameObject canvasGO = new GameObject { name = "PopupCanvas" };
        PopupCanvas = canvasGO.AddComponent<Canvas>();
        PopupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        PopupCanvas.sortingOrder = 100;
        
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(300, 600);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        canvasGO.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasGO);
    }
    #endregion

    #region UI 관리 메서드
    public T ShowSceneUI<T>(string name = null) where T : UI_Scene
    {
        if (string.IsNullOrEmpty(name)) name = typeof(T).Name;

        if (sceneCache.TryGetValue(name, out var sceneUI) && sceneUI != null)
        {
            sceneUI.gameObject.SetActive(true);
            return sceneUI as T;
        }

        T ui = GetUI<T>(name, MainCanvas.transform);
        if (ui != null)
        {
            sceneCache[name] = ui;
        }
        return ui;
    }

    /// <summary>
    /// 팝업 UI를 보여줌. 이미 생성된 팝업이 있다면 재사용
    /// </summary>
    public T ShowPopup<T>() where T : UI_Popup
    {
        var popupType = typeof(T);

        if (!popupCache.TryGetValue(popupType, out var popup))
        {
            // 캐시에 없으면 새로 생성
            popup = GetUI<T>(popupType.Name, PopupCanvas.transform);
            if (popup == null) return null;
            popupCache.Add(popupType, popup);
        }
        
        popup.gameObject.SetActive(true);
        
        // 스택에서 해당 팝업을 제거했다가 다시 Push하여 가장 위로 올림
        RebuildStackAndExclude(popup);
        popupStack.Push(popup);

        return popup as T;
    }

    /// <summary>
    /// 가장 위에 있는 팝업 닫기
    /// </summary>
    public void CloseTopPopup()
    {
        if (popupStack.Count > 0)
        {
            ClosePopup(popupStack.Pop());
        }
    }

    /// <summary>
    /// 특정 팝업 UI 닫기(비활성화)
    /// </summary>
    public void ClosePopup(UI_Popup popupToClose)
    {
        if (popupToClose == null || !popupToClose.gameObject.activeSelf) return;

        popupToClose.gameObject.SetActive(false);

        // 스택에서 제거
        RebuildStackAndExclude(popupToClose);
    }
    #endregion

    #region 헬퍼 메서드
    /// <summary>
    /// 스택에서 특정 팝업을 제외하고 스택을 다시 만듦
    /// </summary>
    private void RebuildStackAndExclude(UI_Popup popupToExclude)
    {
        if (popupStack.Count == 0) return;

        var tempStack = new Stack<UI_Popup>(popupStack.Count);
        
        // 제외할 팝업을 만나기 전까지 임시 스택에 보관
        while (popupStack.Count > 0)
        {
            var item = popupStack.Pop();
            if (item != popupToExclude)
            {
                tempStack.Push(item);
            }
        }

        // 임시 스택에서 다시 원래 스택으로 옮겨 순서 복원
        while (tempStack.Count > 0)
        {
            popupStack.Push(tempStack.Pop());
        }
    }

    private T GetUI<T>(string name, Transform parent = null) where T : UI_Base
    {
        if (!prefabCache.TryGetValue(name, out var prefab))
        {
            string path = $"Prefabs/UI/{(typeof(T).IsSubclassOf(typeof(UI_Popup)) ? "Popup" : "Scene")}/{name}";
            prefab = Resources.Load<UI_Base>(path);
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] 프리팹을 로드할 수 없습니다: {path}");
                return null;
            }
            prefabCache.Add(name, prefab);
        }

        var uiInstance = Instantiate(prefab, parent);
        uiInstance.Initialize();
        return uiInstance as T;
    }
    #endregion
}
