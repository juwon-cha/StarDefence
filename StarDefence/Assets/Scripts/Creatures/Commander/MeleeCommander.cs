using UnityEngine;

public class MeleeCommander : Commander
{
    protected override void Attack()
    {
        if (currentTarget == null) return;
        
        // TODO: 애니메이션 또는 이펙트 추가
        
        Debug.Log($"Commander attacks {currentTarget.name} for {CommanderData.attackDamage} damage.");
        currentTarget.TakeDamage(CommanderData.attackDamage);
    }
}
