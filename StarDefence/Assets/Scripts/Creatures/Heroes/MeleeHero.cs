using UnityEngine;

public class MeleeHero : Hero
{
    protected override void Attack()
    {
        if (currentTarget == null) return;
        
        // TODO: 애니메이션 또는 이펙트 재생
        
        Debug.Log($"{HeroData.heroName} attacks {currentTarget.name} for {currentAttackDamage} damage.");
        currentTarget.TakeDamage(currentAttackDamage);
    }
}
