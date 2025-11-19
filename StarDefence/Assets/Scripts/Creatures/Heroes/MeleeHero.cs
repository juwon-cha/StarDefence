using UnityEngine;

public class MeleeHero : Hero
{
    protected override void Attack()
    {
        if (currentTarget == null) return;
        
        // TODO: 애니메이션 또는 이펙트 재생
        
        Debug.Log($"{heroData.heroName} attacks {currentTarget.name} for {heroData.damage} damage.");
        currentTarget.TakeDamage(heroData.damage);
    }
}
