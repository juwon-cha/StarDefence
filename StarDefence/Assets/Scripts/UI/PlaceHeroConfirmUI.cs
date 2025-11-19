using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaceHeroConfirmUI : UI_Popup
{
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button confirmButton;

    private void OnDisable()
    {
        confirmButton.onClick.RemoveAllListeners();
    }

    public void SetData(Tile tile)
    {
        Debug.Log($"[UI SetData] UI가 타일 ({tile.GridX}, {tile.GridY}) 정보로 설정됩니다.");

        // 리스너를 추가하기 전에 항상 초기화하여 중복 방지
        confirmButton.onClick.RemoveAllListeners();

        // tile 변수를 직접 캡처
        confirmButton.onClick.AddListener(() =>
        {
            // 확인 버튼이 어떤 타일을 위해 동작하는지 확인
            Debug.Log($"[UI OnConfirm] '확인' 버튼 클릭! 타일 ({tile.GridX}, {tile.GridY})에 영웅 배치 요청");

            GameManager.Instance.ConfirmPlaceHero(tile);
            UIManager.Instance.ClosePopup(this);
        });

        // TODO: Temp
        costText.text = "0";

        // 타일 위쪽에 UI 위치 설정 (World Space Canvas에 바로 위치 지정)
        transform.position = tile.transform.position + new Vector3(0, 1, 0);
    }
}