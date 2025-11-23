using UnityEngine;

[CreateAssetMenu(fileName = "MeleeCleaveSkill", menuName = "MythicSkills/MeleeCleaveSkill", order = 1)]
public class MeleeCleaveSkillSO : MythicSkillSO
{
    [Header("Cleave Specifics")]
    public float cleaveRadius = 2f;
    [Tooltip("Secondary targets receive this much damage as a multiplier (e.g., 0.5 for 50% damage)")]
    public float secondaryDamageMultiplier = 0.5f;
}
