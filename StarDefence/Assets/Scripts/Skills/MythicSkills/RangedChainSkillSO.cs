using UnityEngine;

[CreateAssetMenu(fileName = "RangedChainSkill", menuName = "MythicSkills/RangedChainSkill", order = 2)]
public class RangedChainSkillSO : MythicSkillSO
{
    [Header("Chain Specifics")]
    [Tooltip("Number of extra targets beyond the primary target")]
    public int numExtraTargets = 2;
    [Tooltip("Damage multiplier for secondary targets (e.g., 0.8 for 80% damage)")]
    public float secondaryDamageMultiplier = 0.8f;
    // Chain range could be added here if needed
}
