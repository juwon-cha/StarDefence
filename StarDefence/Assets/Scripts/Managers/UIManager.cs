using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 팝업이 표시될 캔버스 타입을 정의
public enum UICanvasType { ScreenSpaceOverlay, WorldSpace }

public class UIManager : Singleton<UIManager>
{
    public Canvas MainCanvas { get; private set; }
    public Canvas PopupCanvas { get; private set; }
    public Canvas WorldSpaceCanvas { get; private set; }

    private readonly Stack<UI_Popup> popupStack = new Stack<UI_Popup>();
    private readonly Dictionary<string, UI_Popup> popupCache = new Dictionary<string, UI_Popup>();
    private readonly Dictionary<string, UI_Base> prefabCache = new Dictionary<string, UI_Base>();
    private readonly Dictionary<string, UI_Scene> sceneCache = new Dictionary<string, UI_Scene>();
    
    public HUD MainHUD { get; private set; }

    #region 초기화
    protected override void Awake()
    {
        base.Awake();
        EnsureEventSystem();
        CreateMainCanvas();
        CreatePopupCanvas();
        CreateWorldSpaceCanvas();
    }

    void Start()
    {
        MainHUD = ShowSceneUI<HUD>(Constants.HUD_UI_NAME);
        if (WorldSpaceCanvas != null && Camera.main != null)
        {
            WorldSpaceCanvas.worldCamera = Camera.main;
        }
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

    private void CreateWorldSpaceCanvas()
    {
        GameObject canvasGO = new GameObject { name = "WorldSpaceCanvas" };
        WorldSpaceCanvas = canvasGO.AddComponent<Canvas>();
        WorldSpaceCanvas.renderMode = RenderMode.WorldSpace;
        WorldSpaceCanvas.sortingOrder = 1000;
        
        canvasGO.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        canvasGO.AddComponent<GraphicRaycaster>();
    }
    #endregion

    #region UI 관리 메서드
    public T ShowSceneUI<T>(string uiName) where T : UI_Scene
    {
        if (string.IsNullOrEmpty(uiName)) uiName = typeof(T).Name;

        if (sceneCache.TryGetValue(uiName, out var sceneUI) && sceneUI != null)
        {
            sceneUI.gameObject.SetActive(true);
            return sceneUI as T;
        }

        T ui = GetUI<T>(uiName, MainCanvas.transform);
        if (ui != null)
        {
            sceneCache[uiName] = ui;
            ui.gameObject.SetActive(true); // 새로 생성된 UI를 즉시 활성화
        }
        return ui;
    }
    
    public T ShowWorldSpacePopup<T>(string uiName) where T : UI_Popup
    {
        return ShowPopup<T>(uiName, UICanvasType.WorldSpace);
    }
    
    public T ShowPopup<T>(string uiName, UICanvasType canvasType = UICanvasType.ScreenSpaceOverlay) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(uiName)) uiName = typeof(T).Name;

        // 팝업 인스턴스를 먼저 가져오거나 생성
        T popupInstance = GetPopupInstance<T>(uiName, canvasType);
        if (popupInstance == null) return null;

        // 팝업을 띄우기 전, 팝업의 종류에 따라 다른 팝업들을 정리
        if (!popupInstance.IsIndependentPopup)
        {
            CloseAllNormalPopups();
        }

        // 실제 팝업 활성화 및 스택 관리
        Transform targetParent = (canvasType == UICanvasType.WorldSpace) ? WorldSpaceCanvas.transform : PopupCanvas.transform;
        if (popupInstance.transform.parent != targetParent)
        {
            popupInstance.transform.SetParent(targetParent, false);
        }
        popupInstance.transform.localScale = Vector3.one;
        popupInstance.gameObject.SetActive(true);
        
        // 스택에 이미 있으면 제거하고 다시 Push하여 최상단으로
        RebuildStackAndExclude(popupInstance); 
        popupStack.Push(popupInstance);

        return popupInstance;
    }

    public void CloseTopPopup()
    {
        if (popupStack.Count > 0)
        {
            ClosePopup(popupStack.Pop());
        }
    }

    public void ClosePopup(UI_Popup popupToClose)
    {
        if (popupToClose == null || !popupToClose.gameObject.activeSelf) return;
        
        popupToClose.gameObject.SetActive(false);
        RebuildStackAndExclude(popupToClose);
    }
    
    public void CloseAllNormalPopups()
    {
        // 스택을 복사하여 순회 (수정 중 순회 에러 방지)
        var currentPopups = popupStack.ToList(); 
        foreach (var popup in currentPopups)
        {
            if (popup != null && !popup.IsIndependentPopup)
            {
                ClosePopup(popup);
            }
        }
    }

    public void ClearAllUI()
    {
        while (popupStack.Count > 0)
        {
            UI_Popup popup = popupStack.Pop();
            if (popup != null) Destroy(popup.gameObject);
        }
        popupStack.Clear();

        foreach (var pair in popupCache.Values)
        {
            if (pair != null) Destroy(pair.gameObject);
        }
        popupCache.Clear();

        foreach (var pair in sceneCache.Values)
        {
            if (pair != null) Destroy(pair.gameObject);
        }
        sceneCache.Clear();
        MainHUD = null;
    }
    
    #endregion

    #region 헬퍼 메서드
    private void RebuildStackAndExclude(UI_Popup popupToExclude)
    {
        if (popupStack.Count == 0) return;

        var tempStack = new Stack<UI_Popup>(popupStack.Count);
        
        while (popupStack.Count > 0)
        {
            var item = popupStack.Pop();
            if (item != popupToExclude)
            {
                tempStack.Push(item);
            }
        }
        
        while (tempStack.Count > 0)
        {
            popupStack.Push(tempStack.Pop());
        }
    }
    
    private T GetPopupInstance<T>(string uiName, UICanvasType canvasType) where T : UI_Popup
    {
        if (popupCache.TryGetValue(uiName, out var cachedPopup) && cachedPopup != null)
        {
            return cachedPopup as T;
        }

        Transform targetParent = (canvasType == UICanvasType.WorldSpace) ? WorldSpaceCanvas.transform : PopupCanvas.transform;
        T newPopup = GetUI<T>(uiName, targetParent);
        if (newPopup != null)
        {
            popupCache[uiName] = newPopup;
        }
        return newPopup;
    }

    private T GetUI<T>(string uiName, Transform parent = null) where T : UI_Base
    {
        if (!prefabCache.TryGetValue(uiName, out var prefab))
        {
            string subPath = typeof(T).IsSubclassOf(typeof(UI_Popup)) ? Constants.UI_POPUP_SUB_PATH : Constants.UI_SCENE_SUB_PATH;
            string path = Constants.UI_ROOT_PATH + subPath + uiName;
            
            prefab = Resources.Load<UI_Base>(path);
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] 프리팹을 로드할 수 없습니다: {path}");
                return null;
            }
            prefabCache.Add(uiName, prefab);
        }

        var uiInstance = Instantiate(prefab, parent);
        uiInstance.name = prefab.name; // 이름 일치
        var component = uiInstance.GetComponent<T>();
        component.Initialize();
        component.gameObject.SetActive(false); // 생성 시 비활성화
        return component;
    }
    #endregion
}
