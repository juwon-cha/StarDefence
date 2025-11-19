using UnityEngine;

public class RangedHero : Hero
{
    protected override void Attack()
    {
        if (currentTarget == null) return;
        
        // HeroDataSO에 지정된 발사체 프리팹 경로 사용
        if (string.IsNullOrEmpty(heroData.FullProjectilePrefabPath))
        {
            Debug.LogError($"{heroData.heroName} has no projectile prefab path assigned in its HeroDataSO.");
            return;
        }
        
        // TODO: 애니메이션 또는 이펙트 재생
        Debug.Log($"{heroData.heroName} fires a projectile at {currentTarget.name}.");
        
        GameObject projectileGO = PoolManager.Instance.Get(heroData.FullProjectilePrefabPath);
        if (projectileGO == null) return;

        projectileGO.transform.position = transform.position;
        
        Projectile projectile = projectileGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            // 발사체 초기화
            projectile.Initialize(currentTarget, heroData.damage);
        }
    }
}
