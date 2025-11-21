using UnityEngine;

// 버프 타입 정의
public enum BuffType
{
    AttackSpeed,
    AttackDamage,
    Range,
}

[CreateAssetMenu(fileName = "NewBuff", menuName = "ScriptableObjects/BuffSO")]
public class BuffDataSO : ScriptableObject
{
    [Header("Buff Info")]
    public BuffType buffType;
    public string buffName;
    [TextArea] public string description;

    [Header("Buff Stats")]
    [Tooltip("공격 속도: 0.2 입력 시 20% 증가")]
    public float value; // 버프 값
    public Color tileColor; // 버프 타일의 색상
}
