using UnityEngine;

[CreateAssetMenu(fileName = "BountyData", menuName = "ScriptableObjects/BountyData")]
public class BountyDataSO : ScriptableObject
{
    [Header("Base Enemy Data")]
    [Tooltip("스폰될 적의 기본 데이터를 연결합니다.")]
    public EnemyDataSO enemyData;

    [Header("Bounty Specifics")]
    [Tooltip("UI에 표시될 아이콘입니다.")]
    public Sprite bountyIcon;

    [Tooltip("현상금으로 지급할 골드입니다.")]
    public int bountyGold;

    [Tooltip("현상금으로 지급할 미네랄입니다.")]
    public int bountyMineral;
}
