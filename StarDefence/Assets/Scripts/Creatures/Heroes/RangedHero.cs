using System.Collections.Generic;
using UnityEngine;

public class RangedHero : Hero
{
    protected override void Attack()
    {
        if (currentTarget == null) return;
        if (string.IsNullOrEmpty(HeroData.FullProjectilePrefabPath))
        {
            Debug.LogError($"{HeroData.heroName} has no projectile prefab path assigned in its HeroDataSO.");
            return;
        }

        // 초월 상태이고 스킬 쿨다운이 준비되었으며 신화 스킬이 할당되어 있을 경우
        if (IsTranscended && mythicSkillCooldownTimer <= 0 && HeroData.mythicSkill != null)
        {
            // RangedChainSkillSO로 캐스팅하여 스킬 데이터 사용
            RangedChainSkillSO chainSkill = HeroData.mythicSkill as RangedChainSkillSO;
            if (chainSkill != null)
            {
                Debug.Log($"[Mythic Skill] {HeroData.heroName} uses {chainSkill.skillName}!");

                // 주 타겟 공격 (100% 데미지)
                FireProjectile(currentTarget, currentAttackDamage);

                // 추가 타겟 찾기
                int extraTargetCount = chainSkill.numExtraTargets;
                if (extraTargetCount > 0)
                {
                    List<Enemy> potentialSecondaryTargets = new List<Enemy>();
                    float attackRangeSqr = HeroData.attackRange * HeroData.attackRange; // 거리 비교를 위해 제곱값 사용

                    // 조건에 맞는 모든 잠재적 타겟 수집
                    foreach (var enemy in WaveManager.Instance.ActiveEnemies)
                    {
                        if (enemy == null || !enemy.gameObject.activeSelf || enemy == currentTarget)
                        {
                            continue;
                        }

                        // 사거리 내에 있는 적만 고려
                        if (Vector3.SqrMagnitude(transform.position - enemy.transform.position) <= attackRangeSqr)
                        {
                            potentialSecondaryTargets.Add(enemy);
                        }
                    }

                    // 수집된 타겟을 영웅으로부터의 거리 기준으로 정렬
                    potentialSecondaryTargets.Sort((e1, e2) =>
                    {
                        float dist1 = Vector3.SqrMagnitude(transform.position - e1.transform.position);
                        float dist2 = Vector3.SqrMagnitude(transform.position - e2.transform.position);
                        return dist1.CompareTo(dist2);
                    });

                    // 필요한 개수만큼의 타겟에 투사체 발사
                    for (int i = 0; i < extraTargetCount && i < potentialSecondaryTargets.Count; i++)
                    {
                        Enemy target = potentialSecondaryTargets[i];
                        FireProjectile(target, currentAttackDamage * chainSkill.secondaryDamageMultiplier);
                    }
                }
                
                // 쿨다운 초기화
                mythicSkillCooldownTimer = chainSkill.cooldown;
            }
            else
            {
                Debug.LogWarning($"[RangedHero] {HeroData.heroName} is transcended but has an incompatible MythicSkillSO assigned!");
                // 호환되지 않는 스킬 SO가 할당되었을 경우 일반 공격 수행
                FireProjectile(currentTarget, currentAttackDamage);
            }
        }
        else
        {
            // 일반 공격
            FireProjectile(currentTarget, currentAttackDamage);
        }
    }

    /// <summary>
    /// 지정된 대상에게 투사체를 발사하는 헬퍼 메서드
    /// </summary>
    private void FireProjectile(Enemy target, float damage)
    {
        if (target == null) return;
        
        GameObject projectileGO = PoolManager.Instance.Get(HeroData.FullProjectilePrefabPath);
        if (projectileGO == null) return;

        projectileGO.transform.position = transform.position;
        
        Projectile projectile = projectileGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(target, damage);
        }
    }
}
