using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class BountyPopupUI : UI_Popup
{
    [SerializeField] private GameObject bountyButtonContainer;
    [SerializeField] private GameObject bountyButtonPrefab;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private GameObject cooldownOverlay;
    [SerializeField] private Button closeButton;

    private readonly List<BountyButtonUI> bountyButtons = new List<BountyButtonUI>();
    private Coroutine updateTimerCoroutine;

    public override void Initialize()
    {
        base.Initialize();

        if (bountyButtonContainer == null || bountyButtonPrefab == null || cooldownText == null || cooldownOverlay == null || closeButton == null)
        {
            Debug.LogError($"[BountyPopupUI] Inspector 참조가 하나 이상 누락되었습니다. BountyPopupUI 프리팹을 선택하여 모든 필드가 제대로 할당되었는지 확인해주세요.");
            gameObject.SetActive(false); // 추가 에러 방지를 위해 비활성화
            return;
        }

        // 이벤트 구독
        BountyManager.Instance.OnCooldownStarted += HandleCooldownStarted;
        BountyManager.Instance.OnCooldownFinished += HandleCooldownFinished;
        
        closeButton.onClick.AddListener(Hide);

        // 버튼 목록 초기 설정
        SetupButtons();
    }
    
    private void OnEnable()
    {
        // 팝업이 활성화될 때마다 현재 쿨타임 상태를 즉시 반영
        if (BountyManager.Instance != null && BountyManager.Instance.IsOnCooldown)
        {
            HandleCooldownStarted();
        }
        else
        {
            HandleCooldownFinished();
        }
    }

    private void OnDestroy()
    {
        if (BountyManager.Instance != null)
        {
            BountyManager.Instance.OnCooldownStarted -= HandleCooldownStarted;
            BountyManager.Instance.OnCooldownFinished -= HandleCooldownFinished;
        }
    }
    
    private void SetupButtons()
    {
        var bountyList = BountyManager.Instance.GetBountyList();
        if (bountyList == null)
        {
            Debug.LogError("[BountyPopupUI] Bounty list is not set in BountyManager!");
            return;
        }

        // 필요한 만큼 버튼 생성 또는 기존 버튼 재사용
        for (int i = 0; i < bountyList.Count; i++)
        {
            BountyButtonUI buttonUI;
            if (i < bountyButtons.Count)
            {
                // 기존 버튼 재사용
                buttonUI = bountyButtons[i];
            }
            else
            {
                // 버튼이 부족하면 새로 생성
                var btnObj = Instantiate(bountyButtonPrefab, bountyButtonContainer.transform);
                buttonUI = btnObj.GetComponent<BountyButtonUI>();
                if (buttonUI == null)
                {
                    Debug.LogError("[BountyPopupUI] bountyButtonPrefab에 BountyButtonUI 스크립트가 없습니다!");
                    continue;
                }
                bountyButtons.Add(buttonUI);
            }

            // 버튼 데이터 설정 및 리스너 연결
            var data = bountyList[i];
            buttonUI.SetData(data);
        
            buttonUI.Button.onClick.RemoveAllListeners();
            buttonUI.Button.onClick.AddListener(() => OnBountyButtonClicked(data));
        
            buttonUI.gameObject.SetActive(true);
        }

        // 남는 버튼들은 비활성화
        for (int i = bountyList.Count; i < bountyButtons.Count; i++)
        {
            bountyButtons[i].gameObject.SetActive(false);
        }
    }

    private void HandleCooldownStarted()
    {
        cooldownOverlay.SetActive(true);
        if (updateTimerCoroutine != null)
        {
            StopCoroutine(updateTimerCoroutine);
        }
        updateTimerCoroutine = StartCoroutine(UpdateTimerCoroutine());
    }

    private void HandleCooldownFinished()
    {
        if(cooldownOverlay != null)
            cooldownOverlay.SetActive(false);
            
        if (updateTimerCoroutine != null)
        {
            StopCoroutine(updateTimerCoroutine);
            updateTimerCoroutine = null;
        }
    }

    private IEnumerator UpdateTimerCoroutine()
    {
        while (BountyManager.Instance != null && BountyManager.Instance.IsOnCooldown)
        {
            var timeSpan = System.TimeSpan.FromSeconds(BountyManager.Instance.CurrentCooldown);
            cooldownText.text = $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
            yield return null;
        }
        // 코루틴이 정상적으로 끝나면 마지막으로 한번 더 상태를 동기화
        HandleCooldownFinished();
    }

    private void OnBountyButtonClicked(BountyDataSO data)
    {
        BountyManager.Instance.TrySpawnBountyMonster(data);
    }

    public void Hide()
    {
        UIManager.Instance.ClosePopup(this);
    }
}
