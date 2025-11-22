using UnityEngine;

public class RangedCommander : Commander
{
    protected override void Attack()
    {
        if (currentTarget == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(CommanderData.projectilePrefabName))
        {
            Debug.LogError($"{CommanderData.commanderName} has no projectile prefab name assigned in its CommanderDataSO.");
            return;
        }

        // TODO: 애니메이션 또는 이펙트 재생
        Debug.Log($"{CommanderData.commanderName} fires a projectile at {currentTarget.name}.");
        
        GameObject projectileGO = PoolManager.Instance.Get(CommanderData.FullProjectilePrefabPath);
        if (projectileGO == null)
        {
            return;
        }

        projectileGO.transform.position = transform.position;
        
        Projectile projectile = projectileGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            // 발사체 초기화
            projectile.Initialize(currentTarget, currentAttackDamage);
        }
    }
}
