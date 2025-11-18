using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : UI_Scene
{
    [SerializeField] private TextMeshProUGUI waveText;

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
}
