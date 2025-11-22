using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : UI_Scene
{
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI mineralsText;
    [SerializeField] private TextMeshProUGUI probeCountText; // 프로브 숫자 표시용
    [SerializeField] private Button probePurchaseButton;
    [SerializeField] private Button commandCenterButton;
    [SerializeField] private Button bountyButton; // 현상금 버튼 참조
    [SerializeField] private Button upgradeButton; // 업그레이드 버튼 참조
    
    [Header("Probe Target UI References")]
    [SerializeField] private RectTransform commandCenterRect;
    [SerializeField] private RectTransform leftMineralPatchRect;
    [SerializeField] private RectTransform rightMineralPatchRect;

    public RectTransform CommandCenterRect => commandCenterRect;
    public RectTransform LeftMineralPatchRect => leftMineralPatchRect;
    public RectTransform RightMineralPatchRect => rightMineralPatchRect;

    void Start()
    {
        // 재화 이벤트 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged += UpdateGoldText;
            GameManager.Instance.OnMineralsChanged += UpdateMineralsText;

            // 초기값 설정
            UpdateGoldText(GameManager.Instance.Gold);
            UpdateMineralsText(GameManager.Instance.Minerals);
        }

        // 프로브 숫자 변경 이벤트 구독
        if (ProbeManager.Instance != null)
        {
            ProbeManager.Instance.OnProbeCountChanged += UpdateProbeCountText;
            // 초기값 설정
            UpdateProbeCountText(ProbeManager.Instance.CurrentProbeCount, ProbeManager.Instance.MaxProbeCount);
        }

        // 리스너 초기화 후 코드에서 명시적으로 등록 (에디터 설정 오류 방지)
        if (probePurchaseButton != null)
        {
            probePurchaseButton.onClick.RemoveAllListeners();
            probePurchaseButton.onClick.AddListener(OnProbePurchaseButtonClicked);
        }
        if (commandCenterButton != null)
        {
            commandCenterButton.onClick.RemoveAllListeners();
            commandCenterButton.onClick.AddListener(OnProbePurchaseButtonClicked);
        }
        if (bountyButton != null)
        {
            bountyButton.onClick.RemoveAllListeners();
            bountyButton.onClick.AddListener(OnBountyButtonClicked);
        }
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged -= UpdateGoldText;
            GameManager.Instance.OnMineralsChanged -= UpdateMineralsText;
        }

        if (ProbeManager.Instance != null)
        {
            ProbeManager.Instance.OnProbeCountChanged -= UpdateProbeCountText;
        }
        
        // 버튼 리스너 해제
        if (probePurchaseButton != null)
        {
            probePurchaseButton.onClick.RemoveAllListeners();
        }
        if (commandCenterButton != null)
        {
            commandCenterButton.onClick.RemoveAllListeners();
        }
        if (bountyButton != null)
        {
            bountyButton.onClick.RemoveAllListeners();
        }
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
        }
    }

    private void OnBountyButtonClicked()
    {
        UIManager.Instance.ShowPopup<BountyPopupUI>(Constants.BOUNTY_POPUP_UI_NAME);
    }

    private void OnUpgradeButtonClicked()
    {
        UIManager.Instance.ShowPopup<UpgradePopupUI>(Constants.UPGRADE_POPUP_UI_NAME);
    }

    private void UpdateProbeCountText(int current, int max)
    {
        if (probeCountText != null)
        {
            probeCountText.text = $"{current}/{max}";
        }

        // 프로브 수가 최대치에 도달하면 버튼 비활성화
        bool canPurchase = current < max;
        if (probePurchaseButton != null)
        {
            probePurchaseButton.interactable = canPurchase;
        }
        if (commandCenterButton != null)
        {
            // 커맨드 센터는 버튼 기능만 비활성화하고 시각적으로는 활성화 상태를 유지할 수 있음
            commandCenterButton.interactable = canPurchase;
        }
    }

    private void OnProbePurchaseButtonClicked()
    {
        var popup = UIManager.Instance.ShowPopup<ProbePurchaseUI>(Constants.PROBE_PURCHASE_UI_NAME);
        if (popup != null)
        {
            popup.SetData();
        }
    }

    public void UpdateWaveText(int currentWave, int totalWaves)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave {currentWave + 1} / {totalWaves}";
        }
    }

    public void ShowAllWavesCleared()
    {
        if (waveText != null)
        {
            waveText.text = "All Waves Cleared!";
        }
    }

    private void UpdateGoldText(int amount)
    {
        if (goldText != null)
        {
            goldText.text = amount.ToString();
        }
    }

    private void UpdateMineralsText(int amount)
    {
        if (mineralsText != null)
        {
            mineralsText.text = amount.ToString();
        }
    }
}
