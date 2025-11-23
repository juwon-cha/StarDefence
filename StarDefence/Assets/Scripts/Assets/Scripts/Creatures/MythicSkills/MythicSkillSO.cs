using UnityEngine;

public abstract class MythicSkillSO : ScriptableObject
{
    [Header("Skill Info")]
    public string skillName = "New Mythic Skill";
    [TextArea] public string skillDescription = "Description of the skill.";
    public Sprite skillIcon;

    [Header("Skill Parameters")]
    public float cooldown = 5f; // 스킬 재사용 대기시간

    // 필요하다면, 스킬 타입(예: 액티브, 패시브)이나 발동 조건 등을 여기에 추가
}
