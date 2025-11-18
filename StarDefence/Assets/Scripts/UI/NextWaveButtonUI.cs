using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NextWaveButtonUI : UI_Scene
{
    [Header("UI Components")]
    [SerializeField] private Image circularSlider;
    [SerializeField] private Text timerText;
    [SerializeField] private Button button;

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

        if (button != null)
        {
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
                // 진행될수록 녹색 -> 붉은색으로 변경
                circularSlider.color = Color.Lerp(Color.green, Color.red, progress);
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

        gameObject.SetActive(false);
    }
}