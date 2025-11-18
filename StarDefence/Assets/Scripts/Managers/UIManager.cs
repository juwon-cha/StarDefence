using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public Canvas MainCanvas { get; private set; }
    public Canvas PopupCanvas { get; private set; }
    
    private Stack<UI_Popup> popupStack = new Stack<UI_Popup>();
    private Dictionary<string, UI_Base> uiCache = new Dictionary<string, UI_Base>();
    private Dictionary<string, UI_Scene> _sceneCache = new Dictionary<string, UI_Scene>();

    public HUD MainHUD { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        EnsureEventSystem();
        CreateMainCanvas();
        CreatePopupCanvas();
    }

    void Start()
    {
        // TODO: Temp 게임 시작 시 메인 HUD 생성
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
        MainCanvas.sortingOrder = 0; // 기본 캔버스
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(300, 600); // 사용자 설정 해상도
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
        PopupCanvas.sortingOrder = 100; // 팝업은 항상 위에
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(300, 600); // 사용자 설정 해상도
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        canvasGO.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasGO);
    }

    public T ShowSceneUI<T>(string name) where T : UI_Scene
    {
        if (string.IsNullOrEmpty(name)) name = typeof(T).Name;

        if (_sceneCache.TryGetValue(name, out UI_Scene ui))
        {
            if(ui != null)
            {
                ui.gameObject.SetActive(true);
                return ui as T;
            }
            else
            {
                _sceneCache.Remove(name);
            }
        }

        T sceneUI = GetUI<T>(name, MainCanvas.transform);
        if (sceneUI != null)
        {
            _sceneCache.Add(name, sceneUI);
        }
        return sceneUI;
    }

    public T ShowPopupUI<T>(string name = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(name)) name = typeof(T).Name;

        T popup = GetUI<T>(name, PopupCanvas.transform);
        popupStack.Push(popup);
        return popup;
    }

    public void ClosePopupUI()
    {
        if (popupStack.Count > 0)
        {
            UI_Popup popup = popupStack.Pop();
            Destroy(popup.gameObject);
        }
    }

    public T ShowWorldSpaceUI<T>(string name, Vector3 position) where T : UI_Scene
    {
        if (string.IsNullOrEmpty(name)) name = typeof(T).Name;

        T worldUI = GetUI<T>(name, position, Quaternion.identity);
        
        Canvas canvas = worldUI.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = Camera.main;
            // 렌더링 순서 문제를 해결하기 위한 핵심 코드
            canvas.overrideSorting = true;
            canvas.sortingLayerName = "UI"; // "UI"라는 이름의 Sorting Layer 필요함
            canvas.sortingOrder = 10; 
        }

        return worldUI;
    }

    private T GetUI<T>(string name, Transform parent = null) where T : UI_Base
    {
        return GetUI<T>(name, Vector3.zero, Quaternion.identity, parent, true);
    }
    
    private T GetUI<T>(string name, Vector3 position, Quaternion rotation, Transform parent = null, bool isScreenSpace = false) where T : UI_Base
    {
        if (!uiCache.ContainsKey(name))
        {
            string path = $"Prefabs/UI/{(typeof(T).IsSubclassOf(typeof(UI_Popup)) ? "Popup" : "Scene")}/{name}";
            UI_Base prefab = Resources.Load<UI_Base>(path);
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] 프리팹을 로드할 수 없습니다: {path}");
                return null;
            }
            uiCache.Add(name, prefab);
        }

        UI_Base uiInstance;
        if (isScreenSpace)
        {
            uiInstance = Instantiate(uiCache[name], parent);
        }
        else
        {
            uiInstance = Instantiate(uiCache[name], position, rotation, parent);
        }
        
        uiInstance.Initialize();
        return uiInstance as T;
    }
}
