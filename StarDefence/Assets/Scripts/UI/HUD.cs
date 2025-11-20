using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : UI_Scene
{
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI mineralsText;

    void Start()
    {
        // 재화 이벤트 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged += UpdateGoldText;
            GameManager.Instance.OnMineralsChanged += UpdateMineralsText;

            // 초기값 설정
            UpdateGoldText(GameManager.Instance.Gold);
            UpdateMineralsText(GameManager.Instance.Minerals);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged -= UpdateGoldText;
            GameManager.Instance.OnMineralsChanged -= UpdateMineralsText;
        }
    }

    public void UpdateWaveText(int currentWave, int totalWaves)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave {currentWave + 1} / {totalWaves}";
        }
    }

    public void ShowAllWavesCleared()
    {
        if (waveText != null)
        {
            waveText.text = "All Waves Cleared!";
        }
    }

    private void UpdateGoldText(int amount)
    {
        if (goldText != null)
        {
            goldText.text = amount.ToString();
        }
    }

    private void UpdateMineralsText(int amount)
    {
        if (mineralsText != null)
        {
            mineralsText.text = amount.ToString();
        }
    }
}
