using UnityEngine;

public class RangedHero : Hero
{
    // TODO: 발사체 프리팹을 외부에서 할당받도록 변경할 수 있음
    private const string PROJECTILE_PREFAB_PATH = "Prefabs/Projectiles/HeroProjectile";
    private GameObject projectilePrefab;

    public override void Init(HeroDataSO data, Tile tile)
    {
        base.Init(data, tile);
        
        // 원거리 영웅 초기화 시 발사체 프리팹을 로드
        projectilePrefab = Resources.Load<GameObject>(PROJECTILE_PREFAB_PATH);
        if (projectilePrefab == null)
        {
            Debug.LogError($"발사체 프리팹을 로드할 수 없습니다: {PROJECTILE_PREFAB_PATH}");
        }
    }

    protected override void Attack()
    {
        if (currentTarget == null || projectilePrefab == null) return;
        
        // TODO: 애니메이션 또는 이펙트 재생
        Debug.Log($"{heroData.heroName} fires a projectile at {currentTarget.name}.");
        
        // 발사체 생성
        GameObject projectileGO = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Projectile projectile = projectileGO.GetComponent<Projectile>();
        
        // 발사체 초기화
        projectile.Initialize(currentTarget, heroData.damage);
    }
}
