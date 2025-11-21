using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 팝업이 표시될 캔버스 타입을 정의
public enum UICanvasType { ScreenSpaceOverlay, WorldSpace }

public class UIManager : Singleton<UIManager>
{
    public Canvas MainCanvas { get; private set; }
    public Canvas PopupCanvas { get; private set; }
    public Canvas WorldSpaceCanvas { get; private set; } // 새로운 월드 공간 캔버스 추가

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
        CreateWorldSpaceCanvas(); // 월드 공간 캔버스 생성
    }

    void Start()
    {
        MainHUD = ShowSceneUI<HUD>(Constants.HUD_UI_NAME);
        // WorldSpaceCanvas의 World Camera 설정
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

    // 새로운 월드 공간 캔버스 생성 메서드
    private void CreateWorldSpaceCanvas()
    {
        GameObject canvasGO = new GameObject { name = "WorldSpaceCanvas" };
        WorldSpaceCanvas = canvasGO.AddComponent<Canvas>();
        WorldSpaceCanvas.renderMode = RenderMode.WorldSpace; // 월드 공간 렌더링
        WorldSpaceCanvas.sortingOrder = 1000;
        
        // WorldSpaceCanvas의 기본 스케일 설정(자식 UI가 적절한 크기로 보이도록)
        // 적절한 스케일 값은 게임 해상도 및 UI 디자인에 따라 달라질 수 있음
        canvasGO.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

        canvasGO.AddComponent<GraphicRaycaster>();
    }
    #endregion

    #region UI 관리 메서드
    public T ShowSceneUI<T>(string uiName) where T : UI_Scene // uiName 매개변수 추가
    {
        if (string.IsNullOrEmpty(uiName)) uiName = typeof(T).Name; // null 또는 빈 문자열이면 타입 이름 사용

        if (sceneCache.TryGetValue(uiName, out var sceneUI) && sceneUI != null)
        {
            sceneUI.gameObject.SetActive(true);
            return sceneUI as T;
        }

        T ui = GetUI<T>(uiName, MainCanvas.transform);
        if (ui != null)
        {
            sceneCache[uiName] = ui;
        }
        return ui;
    }

    /// <summary>
    /// 팝업 UI를 보여줌. 이미 생성된 팝업이 있다면 재사용
    /// </summary>
    /// <param name="uiName">표시할 UI 프리팹의 이름</param>
    /// <param name="canvasType">팝업이 올라갈 캔버스의 종류</param>
    public T ShowPopup<T>(string uiName, UICanvasType canvasType = UICanvasType.ScreenSpaceOverlay) where T : UI_Popup // uiName 매개변수 추가
    {
        if (string.IsNullOrEmpty(uiName))
        {
            uiName = typeof(T).Name;
        }

        Transform targetParent = (canvasType == UICanvasType.WorldSpace) ? WorldSpaceCanvas.transform : PopupCanvas.transform;
        var popupType = typeof(T);

        if (!popupCache.TryGetValue(popupType, out var popup))
        {
            popup = GetUI<T>(uiName, targetParent);
            if (popup == null) return null;
            popupCache.Add(popupType, popup);
        }

        // 캐시에서 가져왔더라도 부모가 다르면 재설정
        if (popup.transform.parent != targetParent)
        {
            popup.transform.SetParent(targetParent, false);
        }

        // 월드 스페이스 UI의 크기 문제 해결을 위해 스케일 초기화
        popup.transform.localScale = Vector3.one;

        // 팝업 활성화 및 스택 관리
        popup.gameObject.SetActive(true);
        RebuildStackAndExclude(popup);
        popupStack.Push(popup);

        return popup as T;
    }

    /// <summary>
    /// 월드 공간 팝업 UI를 보여줌. 이미 생성된 팝업이 있다면 재사용
    /// 이 메서드는 항상 WorldSpaceCanvas에 UI 생성
    /// </summary>
    /// <param name="uiName">표시할 UI 프리팹의 이름</param>
    public T ShowWorldSpacePopup<T>(string uiName) where T : UI_Popup
    {
        return ShowPopup<T>(uiName, UICanvasType.WorldSpace);
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
        RebuildStackAndExclude(popupToClose);
    }

    /// <summary>
    /// 모든 팝업 및 씬 UI를 강제로 파괴하고 캐시 초기화
    /// </summary>
    public void ClearAllUI()
    {
        // 스택에 있는 활성 팝업 파괴
        while (popupStack.Count > 0)
        {
            UI_Popup popup = popupStack.Pop();
            if (popup != null)
            {
                Destroy(popup.gameObject);
            }
        }
        popupStack.Clear();

        // 캐시에 있는 모든 팝업 인스턴스 파괴
        foreach (var pair in popupCache)
        {
            if (pair.Value != null)
            {
                Destroy(pair.Value.gameObject);
            }
        }
        popupCache.Clear();

        // 씬 UI 파괴
        foreach (var pair in sceneCache)
        {
            if (pair.Value != null)
            {
                Destroy(pair.Value.gameObject);
            }
        }
        sceneCache.Clear();
        MainHUD = null; // 참조도 초기화
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

    private T GetUI<T>(string uiName, Transform parent = null) where T : UI_Base // uiName 매개변수 추가
    {
        if (!prefabCache.TryGetValue(uiName, out var prefab)) // uiName을 키로 사용
        {
            string subPath = typeof(T).IsSubclassOf(typeof(UI_Popup)) ? Constants.UI_POPUP_SUB_PATH : Constants.UI_SCENE_SUB_PATH;
            string path = Constants.UI_ROOT_PATH + subPath + uiName; // Constants 사용
            
            prefab = Resources.Load<UI_Base>(path);
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] 프리팹을 로드할 수 없습니다: {path}");
                return null;
            }
            prefabCache.Add(uiName, prefab); // uiName을 키로 사용
        }

        var uiInstance = Instantiate(prefab, parent);
        uiInstance.Initialize();
        return uiInstance as T;
    }
    #endregion
}
