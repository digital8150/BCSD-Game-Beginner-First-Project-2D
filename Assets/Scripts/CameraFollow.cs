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

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desiredPosition = new Vector3(target.position.x, initialY, initialZ);
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

        transform.position = smoothedPosition;
    }
}