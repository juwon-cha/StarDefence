using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Enemy target;
    private int damage;
    private float speed = 15f; // 발사체 속도

    private void OnDisable()
    {
        // 풀에 반환될 때 타겟 정보 초기화
        target = null;
    }

    /// <summary>
    /// 발사체 초기화
    /// </summary>
    /// <param name="target">공격할 대상</param>
    /// <param name="damage">입힐 데미지</param>
    public void Initialize(Enemy target, int damage)
    {
        this.target = target;
        this.damage = damage;
    }

    void Update()
    {
        // 타겟이 없거나 비활성화되면 풀에 반환
        if (target == null || !target.gameObject.activeSelf)
        {
            PoolManager.Instance.Release(gameObject);
            return;
        }

        // 타겟을 향해 이동
        Vector2 direction = (target.transform.position - transform.position).normalized;
        transform.Translate(direction * speed * Time.deltaTime);

        // 타겟과의 거리가 매우 가까워지면 데미지를 입히고 풀에 반환
        if (Vector2.Distance(transform.position, target.transform.position) < 0.1f)
        {
            target.TakeDamage(damage);
            PoolManager.Instance.Release(gameObject);
        }
    }
}
