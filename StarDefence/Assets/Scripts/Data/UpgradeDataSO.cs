using UnityEngine;

public enum UpgradeType
{
    LowTierHeroStat,    // T1-2 영웅 스탯 강화
    HighTierHeroStat,   // T3-초월 영웅 스탯 강화
    CommanderStat,      // 지휘관 스탯 강화
    SummonRate          // 영웅 소환 확률 증가
}

[CreateAssetMenu(fileName = "UpgradeData", menuName = "ScriptableObjects/UpgradeDataSO", order = 10)]
public class UpgradeDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public UpgradeType upgradeType;
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;
    
    [Header("비용 정보")]
    public bool useGold; // true면 골드, false면 미네랄 사용
    public int baseCost;
    public int costIncreasePerLevel;
}
