using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Probe : MonoBehaviour
{
    private enum State
    {
        Idle,
        MovingToMine,
        Mining,
        ReturningToCommandCenter,
    }

    [SerializeField] private ProbeDataSO probeData;
    
    private State currentState;
    private Vector3 mineralTargetPos;
    private Vector3 commandCenterTargetPos;
    private SpriteRenderer spriteRenderer;

    private float miningTimer;
    private bool hasMineral = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(ProbeDataSO data, Vector3 minePos, Vector3 commandCenterPos)
    {
        probeData = data;
        mineralTargetPos = minePos;
        commandCenterTargetPos = commandCenterPos;
        
        // 시작 위치를 커맨드 센터 위치로 설정
        transform.position = commandCenterTargetPos;
        SetState(State.MovingToMine);
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.MovingToMine:
                MoveTo(mineralTargetPos, () => SetState(State.Mining));
                break;
            case State.Mining:
                miningTimer -= Time.deltaTime;
                if (miningTimer <= 0)
                {
                    hasMineral = true;
                    // TODO: 미네랄을 들고 있는 시각적 표시 (예: 스프라이트 변경)
                    SetState(State.ReturningToCommandCenter);
                }
                break;
            case State.ReturningToCommandCenter:
                MoveTo(commandCenterTargetPos, OnArriveAtCommandCenter);
                break;
        }
    }
    
    private void SetState(State newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (currentState)
        {
            case State.Idle:
                break;
            case State.MovingToMine:
                // 이동 시작 시 특별한 로직이 필요하다면 여기에
                break;
            case State.Mining:
                miningTimer = probeData.miningDuration;
                break;
            case State.ReturningToCommandCenter:
                break;
        }
    }

    private void MoveTo(Vector3 destination, System.Action onArrived)
    {
        // 이동 방향에 따라 스프라이트 뒤집기
        if (destination.x > transform.position.x)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }
        
        transform.position = Vector3.MoveTowards(transform.position, destination, probeData.moveSpeed * Time.deltaTime);

        // 목적지에 도착했는지 확인 (거의 도착했을 때를 체크)
        if (Vector3.Distance(transform.position, destination) < 0.01f)
        {
            onArrived?.Invoke();
        }
    }

    private void OnArriveAtCommandCenter()
    {
        if (hasMineral)
        {
            GameManager.Instance.AddMinerals(probeData.mineralsPerTrip);
            hasMineral = false;
            // TODO: 미네랄을 내려놓은 시각적 표시 (예: 스프라이트 원상복귀)
        }
        SetState(State.MovingToMine);
    }
    
    public void Cleanup()
    {
        // 오브젝트 풀에 반환될 때 호출될 정리 로직
        SetState(State.Idle);
        hasMineral = false;
    }
}
