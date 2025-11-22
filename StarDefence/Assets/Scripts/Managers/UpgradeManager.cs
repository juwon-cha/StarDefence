using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UpgradeManager : Singleton<UpgradeManager>
{
    [Header("업그레이드 설정")]
    [SerializeField] private List<UpgradeDataSO> upgradeDatas;
    
    [Header("영웅 소환 티어 설정")]
    [SerializeField] private List<HeroDataSO> tier1Heroes;
    [SerializeField] private List<HeroDataSO> tier2Heroes;
    [SerializeField] private List<HeroDataSO> tier3Heroes;

    // 각 업그레이드의 현재 레벨을 저장
    private Dictionary<UpgradeType, int> upgradeLevels = new Dictionary<UpgradeType, int>();
    
    // 영웅 소환 확률 (레벨별로 T1, T2, T3 확률)
    private readonly List<float[]> summonProbabilities = new List<float[]>()
    {
        new float[] { 1.0f, 0.0f, 0.0f }, // Level 0: T1 100%
        new float[] { 0.8f, 0.2f, 0.0f }, // Level 1: T1 80%, T2 20%
        new float[] { 0.6f, 0.3f, 0.1f }, // Level 2: T1 60%, T2 30%, T3 10%
        new float[] { 0.4f, 0.4f, 0.2f }, // Level 3: T1 40%, T2 40%, T3 20%
        new float[] { 0.2f, 0.5f, 0.3f }, // Level 4: T1 20%, T2 50%, T3 30%
        new float[] { 0.1f, 0.4f, 0.5f }  // Level 5: T1 10%, T2 40%, T3 50%
    };

    // 스탯 강화 효과 (레벨당 데미지/체력 10% 증가)
    private const float STAT_INCREASE_PER_LEVEL = 0.1f;
    
    public event System.Action<UpgradeType> OnUpgradePurchased;
    
    protected override void Awake()
    {
        base.Awake();
        // 레벨 초기화
        foreach (var data in upgradeDatas)
        {
            if (!upgradeLevels.ContainsKey(data.upgradeType))
            {
                upgradeLevels.Add(data.upgradeType, 0);
            }
        }
    }
    
    public int GetUpgradeLevel(UpgradeType type) => upgradeLevels.GetValueOrDefault(type, 0);
    public List<UpgradeDataSO> GetAllUpgradeData() => upgradeDatas;
    
    public int GetCurrentCost(UpgradeType type)
    {
        var data = upgradeDatas.FirstOrDefault(d => d.upgradeType == type);
        if (data == null) return int.MaxValue;
        
        return data.baseCost + (GetUpgradeLevel(type) * data.costIncreasePerLevel);
    }
    
    public void PurchaseUpgrade(UpgradeType type)
    {
        var data = upgradeDatas.FirstOrDefault(d => d.upgradeType == type);
        if (data == null)
        {
            Debug.LogError($"[UpgradeManager] 해당 타입의 업그레이드 데이터를 찾을 수 없습니다: {type}");
            return;
        }

        int cost = GetCurrentCost(type);
        bool success = data.useGold ? GameManager.Instance.SpendGold(cost) : GameManager.Instance.SpendMinerals(cost);

        if (success)
        {
            upgradeLevels[type]++;
            Debug.Log($"[UpgradeManager] {type} 업그레이드 구매 성공! 현재 레벨: {upgradeLevels[type]}");
            OnUpgradePurchased?.Invoke(type);
        }
        else
        {
            Debug.LogWarning($"[UpgradeManager] {type} 업그레이드 구매 실패: 재화 부족");
            // TODO: 재화 부족 UI 피드백 (예: 사운드, 화면 흔들림)
        }
    }
    
    public HeroDataSO GetRandomHeroForSummon()
    {
        int summonLevel = GetUpgradeLevel(UpgradeType.SummonRate);
        if (summonLevel >= summonProbabilities.Count)
        {
            summonLevel = summonProbabilities.Count - 1; // 최대 레벨 초과 방지
        }

        float[] probs = summonProbabilities[summonLevel];
        float random = Random.value;

        if (random < probs[2] && tier3Heroes.Any()) // T3 확률부터 체크
        {
            return tier3Heroes[Random.Range(0, tier3Heroes.Count)];
        }
        if (random < probs[1] + probs[2] && tier2Heroes.Any()) // T2 확률 체크
        {
            return tier2Heroes[Random.Range(0, tier2Heroes.Count)];
        }
        
        // 기본은 T1
        return tier1Heroes.Any() ? tier1Heroes[Random.Range(0, tier1Heroes.Count)] : null;
    }
    
    // 영웅 스탯 보너스를 적용한 새로운 임시 SO를 반환 (개념 예시, 실제 적용은 Hero.cs에서)
    public float GetStatBonus(HeroDataSO heroData)
    {
        if (heroData.tier <= 2)
        {
            return GetUpgradeLevel(UpgradeType.LowTierHeroStat) * STAT_INCREASE_PER_LEVEL;
        }
        return GetUpgradeLevel(UpgradeType.HighTierHeroStat) * STAT_INCREASE_PER_LEVEL;
    }

    public float GetCommanderStatBonus()
    {
        return GetUpgradeLevel(UpgradeType.CommanderStat) * STAT_INCREASE_PER_LEVEL;
    }
}
