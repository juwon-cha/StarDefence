using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NextWaveButtonUI : UI_Popup
{
    [Header("UI Components")]
    [SerializeField] private Image circularSlider;
    [SerializeField] private Text timerText;
    [SerializeField] private Button button;
    [SerializeField] private Gradient sliderGradient; // 색상 변경을 위한 Gradient

    private float countdownDuration;
    private float currentTime;
    private Action onComplete;

    public override void Initialize()
    {
        base.Initialize();
    }

    public void Initialize(float duration, Action onCompleteCallback)
    {
        this.countdownDuration = duration;
        this.currentTime = duration;
        this.onComplete = onCompleteCallback;

        // 리스너 중복 등록 방지
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClicked);
        }

        if (circularSlider != null)
        {
            if (circularSlider.type != Image.Type.Filled)
            {
                Debug.LogWarning("Circular Slider의 Image Type은 'Filled'로 설정되어야 합니다.", this);
                circularSlider.type = Image.Type.Filled;
            }
            circularSlider.fillMethod = Image.FillMethod.Radial360;
            circularSlider.fillClockwise = true; // 시계 방향으로 채우기
        }
        
        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            
            // 경과 시간 비율 계산 (0 -> 1)
            float progress = 1.0f - Mathf.Max(0, currentTime / countdownDuration);

            if (circularSlider != null)
            {
                circularSlider.fillAmount = progress;
                // Gradient를 사용해 색상 평가
                circularSlider.color = sliderGradient.Evaluate(progress);
            }

            if (timerText != null)
            {
                timerText.text = Mathf.CeilToInt(currentTime).ToString();
            }

            yield return null;
        }

        Complete();
    }

    private void OnButtonClicked()
    {
        Complete();
    }

    private void Complete()
    {
        onComplete?.Invoke();
        
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
        StopAllCoroutines();

        UIManager.Instance.ClosePopup(this);
    }
}