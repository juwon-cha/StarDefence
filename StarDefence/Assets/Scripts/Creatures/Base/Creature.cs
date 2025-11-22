using System;
using UnityEngine;

public abstract class Creature : MonoBehaviour
{
    public event Action<float, float> OnHealthChanged;

    [SerializeField] protected CreatureDataSO creatureData;
    public CreatureDataSO CreatureData => creatureData;

    protected float currentHealth;
    public float CurrentHealth => currentHealth;
    
    // 런타임에 버프/업그레이드가 적용될 실제 능력치
    protected float currentMaxHealth;
    protected float currentAttackDamage;
    
    public float CurrentMaxHealth => currentMaxHealth; // UI 등에서 접근할 수 있도록 public 속성 추가

    protected virtual void Start()
    {
        // CreatureDataSO에서 기본 능력치를 가져와 런타임 능력치를 초기화
        // 이 값들은 Hero, Commander 등에서 재정의될 수 있음
        currentMaxHealth = creatureData.maxHealth;
        currentHealth = currentMaxHealth;
        currentAttackDamage = creatureData.attackDamage;
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth, currentMaxHealth); // 최대 체력은 런타임 값 사용

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        // 현상금 몬스터인지 확인하고 보상 지급
        BountyTarget bountyTarget = GetComponent<BountyTarget>();
        if (bountyTarget != null)
        {
            bountyTarget.GrantReward();
        }
    }

    /// <summary>
    /// 자식 클래스에서 체력 변경 이벤트를 안전하게 발생시키기 위한 메소드
    /// </summary>
    protected void TriggerHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, currentMaxHealth);
    }
}
