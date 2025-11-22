using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BountyButtonUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image bountyIconImage;
    [SerializeField] private Button buttonComponent;

    [Header("Reward Sections")]
    [SerializeField] private GameObject goldRewardSection;
    [SerializeField] private TextMeshProUGUI goldRewardText;
    
    [SerializeField] private GameObject mineralRewardSection;
    [SerializeField] private TextMeshProUGUI mineralRewardText;

    public Button Button => buttonComponent;

    /// <summary>
    /// 버튼에 현상금 데이터 설정
    /// </summary>
    public void SetData(BountyDataSO data)
    {
        if (data == null) return;

        // 몬스터 아이콘 설정
        if (bountyIconImage != null)
        {
            bountyIconImage.sprite = data.bountyIcon;
        }

        // 골드 보상 섹션 설정
        if (goldRewardSection != null && goldRewardText != null)
        {
            bool hasGoldReward = data.bountyGold > 0;
            goldRewardSection.SetActive(hasGoldReward);
            if (hasGoldReward)
            {
                goldRewardText.text = data.bountyGold.ToString();
            }
        }

        // 미네랄 보상 섹션 설정
        if (mineralRewardSection != null && mineralRewardText != null)
        {
            bool hasMineralReward = data.bountyMineral > 0;
            mineralRewardSection.SetActive(hasMineralReward);
            if (hasMineralReward)
            {
                mineralRewardText.text = data.bountyMineral.ToString();
            }
        }
    }
}
