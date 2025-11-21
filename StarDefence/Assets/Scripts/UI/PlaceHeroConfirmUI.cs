using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaceHeroConfirmUI : UI_Popup
{
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image resourceIcon; // 재화 아이콘을 위한 필드 추가
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI confirmButtonText;

    private void OnDisable()
    {
        confirmButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// 신규 영웅 배치 확인 UI 설정
    /// </summary>
    public void SetDataForPlacement(Tile tile, int cost)
    {
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            GameManager.Instance.ConfirmPlaceHero(tile);
            UIManager.Instance.ClosePopup(this);
        });

        costText.text = cost.ToString();
        if (confirmButtonText != null)
        {
            confirmButtonText.text = "Summon";
        }
        if (resourceIcon != null)
        {
            resourceIcon.sprite = ResourceManager.Instance.SpriteDB.goldIcon; // 골드 아이콘으로 설정
        }
        
        transform.position = tile.transform.position + new Vector3(0, 1, 0);
    }

    /// <summary>
    /// 영웅 승급 확인 UI 설정
    /// </summary>
        public void SetDataForUpgrade(Hero heroToUpgrade, Hero mergePartner)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                GameManager.Instance.ConfirmUpgradeHero(heroToUpgrade, mergePartner);
                UIManager.Instance.ClosePopup(this);
            });
    
            costText.text = heroToUpgrade.HeroData.upgradeCost.ToString();
            if (confirmButtonText != null)
            {
                confirmButtonText.text = "Upgrade";
            }
            if (resourceIcon != null)
            {
                resourceIcon.sprite = ResourceManager.Instance.SpriteDB.mineralIcon; // 미네랄 아이콘으로 설정
            }
            
            transform.position = heroToUpgrade.transform.position + new Vector3(0, 1, 0);
        }
    
        /// <summary>
        /// 타일 수리 확인 UI 설정
        /// </summary>
        public void SetDataForRepair(Tile tile, int cost)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                GameManager.Instance.ConfirmRepairTile(tile);
                UIManager.Instance.ClosePopup(this);
            });
    
            costText.text = cost.ToString();
            if (confirmButtonText != null)
            {
                confirmButtonText.text = "Repair";
            }
            if (resourceIcon != null)
            {
                resourceIcon.sprite = ResourceManager.Instance.SpriteDB.mineralIcon; // 미네랄 아이콘으로 설정
            }
            
            transform.position = tile.transform.position + new Vector3(0, 1, 0);
        }
    }
    