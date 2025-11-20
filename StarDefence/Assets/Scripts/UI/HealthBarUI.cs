using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage; // 체력바 채움 이미지
    [SerializeField] private float yOffset = 1f; // 몬스터 머리 위에서의 Y축 오프셋

    private Creature trackedCreature; // 이 체력바가 추적할 생명체

    /// <summary>
    /// 체력바 UI를 초기화하고 추적할 생명체 설정
    /// </summary>
    /// <param name="creature">체력바가 표시될 생명체</param>
    public void Initialize(Creature creature)
    {
        trackedCreature = creature;
        // 생명체의 체력 변경 이벤트 구독
        trackedCreature.OnHealthChanged += UpdateHealthBar;
        
        // 초기 위치 설정
        if (trackedCreature != null)
        {
            transform.position = trackedCreature.transform.position + Vector3.up * yOffset;
        }
        // 초기 체력바 상태 업데이트
        UpdateHealthBar(trackedCreature.CurrentHealth, trackedCreature.CreatureData.maxHealth);
    }

    /// <summary>
    /// 생명체의 현재 체력에 따라 체력바 UI 업데이트
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
        // 추적할 생명체가 유효하면 위치를 업데이트
        if (trackedCreature != null && trackedCreature.gameObject.activeSelf)
        {
            transform.position = trackedCreature.transform.position + Vector3.up * yOffset;
        }
        else if (trackedCreature != null) // 생명체가 비활성화되었지만 아직 null이 아닐 때(풀에 반환되는 중)
        {
            // 생명체가 사라지면 체력바도 풀에 반환
            PoolManager.Instance.Release(gameObject);
        }
    }

    private void OnDisable()
    {
        // 풀에 반환될 때 추적 생명체 참조 해제 및 이벤트 구독 해제
        if (trackedCreature != null)
        {
            trackedCreature.OnHealthChanged -= UpdateHealthBar;
            trackedCreature = null;
        }
    }
}
