using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProbeManager : Singleton<ProbeManager>
{
    [Header("Probe Data")]
    [SerializeField] private ProbeDataSO initialProbeData;
    [SerializeField] private int maxProbeCount = 22;

    public event System.Action<int, int> OnProbeCountChanged;

    public int CurrentProbeCount => activeProbes.Count;
    public int MaxProbeCount => maxProbeCount;

    // HUD로부터 런타임에 참조를 받아올 변수들
    private RectTransform commandCenterRect;
    private RectTransform leftMineralPatchRect;
    private RectTransform rightMineralPatchRect;
    
    private List<Probe> activeProbes = new List<Probe>();
    private int leftProbeCount = 0;
    private int rightProbeCount = 0;

    // UI 좌표를 변환하여 저장할 월드 좌표
    private Vector3 commandCenterWorldPos;
    private Vector3 leftMineralPatchWorldPos;
    private Vector3 rightMineralPatchWorldPos;

    void Start()
    {
        StartCoroutine(InitializeTargetsCoroutine());
    }

    private IEnumerator InitializeTargetsCoroutine()
    {
        // UIManager와 MainHUD가 준비될 때까지 한 프레임씩 기다림
        while (UIManager.Instance == null || UIManager.Instance.MainHUD == null)
        {
            yield return null;
        }

        // UIManager를 통해 HUD의 RectTransform 참조를 가져옴
        commandCenterRect = UIManager.Instance.MainHUD.CommandCenterRect;
        leftMineralPatchRect = UIManager.Instance.MainHUD.LeftMineralPatchRect;
        rightMineralPatchRect = UIManager.Instance.MainHUD.RightMineralPatchRect;

        if (commandCenterRect == null || leftMineralPatchRect == null || rightMineralPatchRect == null)
        {
            Debug.LogError("[ProbeManager] UI Scene references from HUD are not set! Initialization cannot continue.");
            yield break; // 코루틴 중단
        }

        // UI 좌표를 월드 좌표로 변환
        commandCenterWorldPos = GetWorldPositionFromUI(commandCenterRect);
        leftMineralPatchWorldPos = GetWorldPositionFromUI(leftMineralPatchRect);
        rightMineralPatchWorldPos = GetWorldPositionFromUI(rightMineralPatchRect);
        
        // 게임 시작 시 프로브 한 마리 생성
        CreateProbe();
    }

    /// <summary>
    /// 새로운 프로브를 생성하고 적절한 자원지로 보냄
    /// </summary>
    public void CreateProbe()
    {
        if (string.IsNullOrEmpty(initialProbeData.FullPrefabPath))
        {
            Debug.LogError("[ProbeManager] Probe prefab path is not set in ProbeDataSO!");
            return;
        }

        GameObject probeGO = PoolManager.Instance.Get(initialProbeData.FullPrefabPath);
        if (probeGO == null)
        {
            Debug.LogError("[ProbeManager] Failed to get Probe from PoolManager.");
            return;
        }

        Probe probe = probeGO.GetComponent<Probe>();
        if (probe == null)
        {
            Debug.LogError("[ProbeManager] The probe prefab does not have a Probe component.");
            PoolManager.Instance.Release(probeGO); // 잘못된 프리팹이므로 반납
            return;
        }

        // 프로브를 보낼 자원지 결정 (수가 더 적은 쪽으로)
        Vector3 targetMineralPatchPos;
        if (leftProbeCount <= rightProbeCount)
        {
            targetMineralPatchPos = leftMineralPatchWorldPos;
            leftProbeCount++;
        }
        else
        {
            targetMineralPatchPos = rightMineralPatchWorldPos;
            rightProbeCount++;
        }
        
        probe.Init(initialProbeData, targetMineralPatchPos, commandCenterWorldPos);
        activeProbes.Add(probe);
        
        // 프로브 수가 변경되었음을 알림
        OnProbeCountChanged?.Invoke(CurrentProbeCount, MaxProbeCount);
    }
    
    private Vector3 GetWorldPositionFromUI(RectTransform rectTransform)
    {
        // Screen Space - Overlay Canvas의 UI 요소를 월드 좌표로 변환
        // 게임 플레이가 이루어지는 2D 평면의 z 좌표를 0으로 가정
        Vector3 screenPos = rectTransform.position;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0; // 2D 게임 평면에 맞게 z 좌표를 0으로 고정
        return worldPos;
    }
    
    public int GetCurrentProbeCost()
    {
        // TODO: 향후 프로브 숫자에 따라 비용이 증가하는 로직을 여기에 추가
        return initialProbeData.purchaseCost + (activeProbes.Count * 2);
    }

    // 게임 종료 또는 씬 전환 시 모든 프로브를 정리하는 로직
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        // PoolManager가 이미 파괴되었을 수 있으므로 null 체크
        if (PoolManager.Instance != null)
        {
            foreach (var probe in activeProbes)
            {
                if (probe != null)
                {
                    probe.Cleanup();
                    PoolManager.Instance.Release(probe.gameObject);
                }
            }
        }
        activeProbes.Clear();
    }
}
