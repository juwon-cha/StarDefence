using UnityEngine;

public class RangedHero : Hero
{
    protected override void Attack()
    {
        if (currentTarget == null)
        {
            return;
        }
        
        if (string.IsNullOrEmpty(HeroData.FullProjectilePrefabPath))
        {
            Debug.LogError($"{HeroData.heroName} has no projectile prefab path assigned in its HeroDataSO.");
            return;
        }
        
        // TODO: 애니메이션 또는 이펙트 재생
        Debug.Log($"{HeroData.heroName} fires a projectile at {currentTarget.name}.");
        
        GameObject projectileGO = PoolManager.Instance.Get(HeroData.FullProjectilePrefabPath);
        if (projectileGO == null) return;

        projectileGO.transform.position = transform.position;
        
        Projectile projectile = projectileGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            // 발사체 초기화
            projectile.Initialize(currentTarget, HeroData.attackDamage);
        }
    }
}
