using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameResultUI : UI_Popup
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button restartButton;

    private void Start()
    {
        restartButton.onClick.AddListener(OnRestartButtonClicked);
    }

    /// <summary>
    /// 특정 제목으로 팝업 초기화
    /// </summary>
    public void SetTitle(string title)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }
    }

    private void OnRestartButtonClicked()
    {
        GameManager.Instance.RestartGame();
    }

    private void OnDestroy()
    {
        restartButton.onClick.RemoveListener(OnRestartButtonClicked);
    }
}
