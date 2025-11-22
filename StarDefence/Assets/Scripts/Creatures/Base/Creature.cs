using System;
using UnityEngine;

public abstract class Creature : MonoBehaviour
{
    public event Action<float, float> OnHealthChanged;

    [SerializeField] protected CreatureDataSO creatureData;
    public CreatureDataSO CreatureData => creatureData;

    protected float currentHealth;
    public float CurrentHealth => currentHealth;

    protected virtual void Start()
    {
        currentHealth = creatureData.maxHealth;
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth, creatureData.maxHealth);

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
}
