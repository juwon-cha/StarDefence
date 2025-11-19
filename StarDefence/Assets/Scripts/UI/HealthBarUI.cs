using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 몬스터 체력바 UI의 동작을 관리하는 스크립트
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage; // 체력바 채움 이미지
    [SerializeField] private float yOffset = 1f; // 몬스터 머리 위에서의 Y축 오프셋

    private Enemy trackedEnemy; // 이 체력바가 추적할 몬스터

    /// <summary>
    /// 체력바 UI를 초기화하고 추적할 몬스터 설정
    /// </summary>
    /// <param name="enemy">체력바가 표시될 몬스터</param>
    public void Initialize(Enemy enemy)
    {
        trackedEnemy = enemy;
        // 몬스터의 체력 변경 이벤트 구독
        trackedEnemy.OnHealthChanged += UpdateHealthBar;
        
        // 초기 위치 설정
        if (trackedEnemy != null)
        {
            transform.position = trackedEnemy.transform.position + Vector3.up * yOffset;
        }
        // 초기 체력바 상태 업데이트는 몬스터의 OnHealthChanged 이벤트가 발생할 때 처리
    }

    /// <summary>
    /// 몬스터의 현재 체력에 따라 체력바 UI 업데이트
    /// </summary>
    /// <param name="currentHealth">현재 체력</param>
    /// <param name="maxHealth">최대 체력</param>
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = currentHealth / maxHealth;
        }
    }

    private void LateUpdate()
    {
        // 추적할 몬스터가 유효하면 위치를 업데이트
        if (trackedEnemy != null && trackedEnemy.gameObject.activeSelf)
        {
            transform.position = trackedEnemy.transform.position + Vector3.up * yOffset;
        }
        else if (trackedEnemy != null) // 몬스터가 비활성화되었지만 아직 null이 아닐 때(풀에 반환되는 중)
        {
            // 몬스터가 사라지면 체력바도 풀에 반환
            PoolManager.Instance.Release(gameObject);
        }
    }

    private void OnDisable()
    {
        // 풀에 반환될 때 추적 몬스터 참조 해제 및 이벤트 구독 해제
        if (trackedEnemy != null)
        {
            trackedEnemy.OnHealthChanged -= UpdateHealthBar;
            trackedEnemy = null;
        }
    }
}
