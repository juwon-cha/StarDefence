using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeButtonUI : MonoBehaviour
{
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image costIconImage; // 골드 또는 미네랄 아이콘

    [SerializeField] private Sprite goldIcon;
    [SerializeField] private Sprite mineralIcon;

    private UpgradeType upgradeType;

    private void Awake()
    {
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
    }

    public void SetData(UpgradeDataSO data)
    {
        upgradeType = data.upgradeType;
        iconImage.sprite = data.icon;
        nameText.text = data.upgradeName;
        costIconImage.sprite = data.useGold ? goldIcon : mineralIcon;
        
        UpdateUI();
    }
    
    public void UpdateUI()
    {
        int currentLevel = UpgradeManager.Instance.GetUpgradeLevel(upgradeType);
        int cost = UpgradeManager.Instance.GetCurrentCost(upgradeType);

        levelText.text = $"Lv. {currentLevel}";
        costText.text = cost.ToString();
    }

    private void OnUpgradeClicked()
    {
        UpgradeManager.Instance.PurchaseUpgrade(upgradeType);
    }
}
