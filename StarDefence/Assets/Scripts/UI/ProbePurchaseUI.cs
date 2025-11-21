using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProbePurchaseUI : UI_Popup
{
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private int probeCost;

    public override void Initialize()
    {
        base.Initialize();

        yesButton.onClick.AddListener(OnYesButtonClicked);
        noButton.onClick.AddListener(OnNoButtonClicked);
    }

    /// <summary>
    /// 팝업이 표시될 때 데이터 설정
    /// </summary>
    public void SetData()
    {
        // ProbeManager에서 현재 프로브 생성 비용 가져오기
        probeCost = ProbeManager.Instance.GetCurrentProbeCost();
        
        if (costText != null)
        {
            costText.text = $"{probeCost}";
        }
    }

    private void OnYesButtonClicked()
    {
        // GameManager에 프로브 생성 요청
        GameManager.Instance.TryPurchaseProbe();
        
        UIManager.Instance.CloseTopPopup();
    }

    private void OnNoButtonClicked()
    {
        UIManager.Instance.CloseTopPopup();
    }
    
    private void OnDestroy()
    {
        if (yesButton != null)
        {
            yesButton.onClick.RemoveListener(OnYesButtonClicked);
        }
        if (noButton != null)
        {
            noButton.onClick.RemoveListener(OnNoButtonClicked);
        }
    }
}
