using UnityEngine;

public class MeleeHero : Hero
{
    protected override void Attack()
    {
        if (currentTarget == null) return;
        
        // 초월 상태이고 스킬 쿨다운이 준비되었으며 신화 스킬이 할당되어 있을 경우
        if (IsTranscended && mythicSkillCooldownTimer <= 0 && HeroData.mythicSkill != null)
        {
            // MeleeCleaveSkillSO로 캐스팅하여 스킬 데이터 사용
            MeleeCleaveSkillSO cleaveSkill = HeroData.mythicSkill as MeleeCleaveSkillSO;
            if (cleaveSkill != null)
            {
                Debug.Log($"[Mythic Skill] {HeroData.heroName} uses {cleaveSkill.skillName}!");

                // 주 타겟에게는 100% 데미지
                currentTarget.TakeDamage(currentAttackDamage);

                // 주변의 다른 적들 탐색
                Collider2D[] colliders = Physics2D.OverlapCircleAll(currentTarget.transform.position, cleaveSkill.cleaveRadius, LayerMask.GetMask("Enemy"));
                foreach (var col in colliders)
                {
                    // 주 타겟이 아닌 다른 적에게 광역 데미지
                    if (col.gameObject != currentTarget.gameObject)
                    {
                        Enemy enemy = col.GetComponent<Enemy>();
                        if (enemy != null)
                        {
                            enemy.TakeDamage(currentAttackDamage * cleaveSkill.secondaryDamageMultiplier);
                        }
                    }
                }
                
                // 쿨다운 초기화
                mythicSkillCooldownTimer = cleaveSkill.cooldown;
            }
            else
            {
                Debug.LogWarning($"[MeleeHero] {HeroData.heroName} is transcended but has an incompatible MythicSkillSO assigned!");
                // 호환되지 않는 스킬 SO가 할당되었을 경우 일반 공격 수행
                Debug.Log($"{HeroData.heroName} attacks {currentTarget.name} for {currentAttackDamage} damage.");
                currentTarget.TakeDamage(currentAttackDamage);
            }
        }
        else
        {
            // 일반 공격
            Debug.Log($"{HeroData.heroName} attacks {currentTarget.name} for {currentAttackDamage} damage.");
            currentTarget.TakeDamage(currentAttackDamage);
        }
    }
}
