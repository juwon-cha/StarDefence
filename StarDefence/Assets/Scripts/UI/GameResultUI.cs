using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        Time.timeScale = 1f;

        // UIManager가 관리하는 모든 UI 요소 지우기
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ClearAllUI();
        }

        // 깨끗한 상태를 위해 영구 관리자 파괴
        if (PoolManager.Instance != null)
        {
            Destroy(PoolManager.Instance.gameObject);
        }
        if (UIManager.Instance != null)
        {
            Destroy(UIManager.Instance.gameObject);
        }
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }
        
        // 현재 활성화된 씬 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        restartButton.onClick.RemoveListener(OnRestartButtonClicked);
    }
}
