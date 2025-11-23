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
        
        /// <summary>
        /// 영웅 초월 확인 UI 설정
        /// </summary>
        public void SetDataForTranscendence(Hero hero, UpgradeDataSO transcendenceUpgrade)
        {
            if (hero == null || transcendenceUpgrade == null)
            {
                Debug.LogError("SetDataForTranscendence: 영웅 또는 초월 업그레이드 데이터가 null입니다.");
                UIManager.Instance.ClosePopup(this);
                return;
            }

            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                GameManager.Instance.ConfirmTranscendence(hero, transcendenceUpgrade);
                UIManager.Instance.ClosePopup(this); // ConfirmTranscendence 호출 후 팝업 스스로 닫기
            });

            costText.text = UpgradeManager.Instance.GetCurrentCost(transcendenceUpgrade.upgradeType).ToString();
            if (confirmButtonText != null)
            {
                confirmButtonText.text = "Mythic";
            }
            if (resourceIcon != null)
            {
                resourceIcon.sprite = transcendenceUpgrade.useGold ? ResourceManager.Instance.SpriteDB.goldIcon : ResourceManager.Instance.SpriteDB.mineralIcon;
            }

            transform.position = hero.transform.position + new Vector3(0, 1, 0);
        }
    }
    