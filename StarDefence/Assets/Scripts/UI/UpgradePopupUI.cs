using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePopupUI : UI_Popup
{
    [SerializeField] private GameObject upgradeButtonContainer;
    [SerializeField] private GameObject upgradeButtonPrefab;
    [SerializeField] private Button closeButton;
    
    private readonly List<UpgradeButtonUI> upgradeButtons = new List<UpgradeButtonUI>();

    public override void Initialize()
    {
        base.Initialize();
        
        closeButton.onClick.AddListener(Hide);
        
        SetupButtons();
    }

    private void OnEnable()
    {
        UpgradeManager.Instance.OnUpgradePurchased += HandleUpgradePurchased;
        UpdateAllButtons(); // 팝업이 열릴 때마다 최신 정보로 갱신
    }

    private void OnDisable()
    {
        if(UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnUpgradePurchased -= HandleUpgradePurchased;
    }
    
    private void SetupButtons()
    {
        var allUpgradeDatas = UpgradeManager.Instance.GetAllUpgradeData();
        if (allUpgradeDatas == null)
        {
            Debug.LogError("[UpgradePopupUI] Upgrade list is not set in UpgradeManager!");
            return;
        }

        List<UpgradeDataSO> filteredUpgradeList = new List<UpgradeDataSO>();
        foreach (var data in allUpgradeDatas)
        {
            // 초월 업그레이드는 제외
            if (data != null && !data.isTranscendenceUpgrade)
            {
                filteredUpgradeList.Add(data);
            }
        }
        
        // 필요한 만큼 버튼 생성 또는 기존 버튼 재사용
        for (int i = 0; i < filteredUpgradeList.Count; i++)
        {
            UpgradeButtonUI buttonUI;
            if (i < upgradeButtons.Count)
            {
                buttonUI = upgradeButtons[i];
            }
            else
            {
                var btnObj = Instantiate(upgradeButtonPrefab, upgradeButtonContainer.transform);
                buttonUI = btnObj.GetComponent<UpgradeButtonUI>();
                if (buttonUI == null)
                {
                    Debug.LogError("[UpgradePopupUI] upgradeButtonPrefab에 UpgradeButtonUI 스크립트가 없습니다!");
                    continue;
                }
                upgradeButtons.Add(buttonUI);
            }

            buttonUI.SetData(filteredUpgradeList[i]);
            buttonUI.gameObject.SetActive(true);
        }

        // 남는 버튼들은 비활성화
        for (int i = filteredUpgradeList.Count; i < upgradeButtons.Count; i++)
        {
            upgradeButtons[i].gameObject.SetActive(false);
        }
    }

    private void HandleUpgradePurchased(UpgradeType purchasedType)
    {
        // 모든 버튼 UI를 업데이트하여 레벨과 비용을 갱신
        UpdateAllButtons();
    }

    private void UpdateAllButtons()
    {
        foreach (var button in upgradeButtons)
        {
            if(button.gameObject.activeSelf)
                button.UpdateUI();
        }
    }
    
    public void Hide()
    {
        UIManager.Instance.ClosePopup(this);
    }
}
