using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("카메라 추적 설정")]
    public Transform target; // 유니티 인스펙터에서 추적할 플레이어 오브젝트의 Transform을 연결해주세요.
    public float smoothTime = 0.3f; // 카메라가 목표 X 위치에 도달하는 데 걸리는 시간 (작을수록 빠르게 따라가고 덜 부드러움)

    private Vector3 velocity = Vector3.zero; // SmoothDamp 함수 내부에서 사용할 속도 참조 변수
    private float initialY; // 카메라의 초기 Y 위치 (수직 고정)
    private float initialZ; // 카메라의 초기 Z 위치 (깊이 고정)

    void Start()
    {
        // 추적 대상이 지정되지 않았다면 경고 메시지를 출력합니다.
        if (target == null)
        {
            Debug.LogError("카메라 추적 대상(Target) Transform이 지정되지 않았습니다! 인스펙터에서 플레이어 오브젝트를 연결해주세요.");
            // 또는 GameObject.FindGameObjectWithTag("Player") 등을 사용하여 코드로 플레이어를 찾을 수도 있습니다.
        }

        // 스크립트 시작 시 카메라의 초기 Y, Z 위치를 저장해둡니다.
        initialY = transform.position.y;
        initialZ = transform.position.z;
    }

    // LateUpdate는 모든 Update 호출이 끝난 후 호출됩니다.
    // 캐릭터의 움직임 계산이 완료된 후 카메라 위치를 업데이트하기에 적합합니다.
    void LateUpdate()
    {
        // 추적 대상이 없으면 아무것도 하지 않습니다.
        if (target == null) return;

        // 카메라가 이동하려는 목표 위치를 계산합니다.
        // 목표 X 위치는 플레이어의 현재 X 위치를 사용합니다.
        // Y 위치는 Start에서 저장한 초기 Y 값을 그대로 사용합니다.
        // Z 위치는 Start에서 저장한 초기 Z 값을 그대로 사용합니다.
        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y > initialY ? target.position.y : initialY, initialZ);

        // Vector3.SmoothDamp 함수를 사용하여 현재 카메라 위치에서 목표 위치(desiredPosition)까지 부드럽게 이동합니다.
        // 이 함수는 현재 위치, 목표 위치, 현재 속도 (ref 키워드 사용), 목표까지 걸리는 예상 시간(smoothTime)을 인자로 받습니다.
        // 함수가 계산한 부드러운 다음 위치 값을 반환합니다.
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

        // 계산된 부드러운 위치로 카메라의 Transform.position을 업데이트합니다.
        transform.position = smoothedPosition;
    }
}