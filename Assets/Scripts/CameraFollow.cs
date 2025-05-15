using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("ī�޶� ���� ����")]
    public Transform target; // ����Ƽ �ν����Ϳ��� ������ �÷��̾� ������Ʈ�� Transform�� �������ּ���.
    public float smoothTime = 0.3f; // ī�޶� ��ǥ X ��ġ�� �����ϴ� �� �ɸ��� �ð� (�������� ������ ���󰡰� �� �ε巯��)

    private Vector3 velocity = Vector3.zero; // SmoothDamp �Լ� ���ο��� ����� �ӵ� ���� ����
    private float initialY; // ī�޶��� �ʱ� Y ��ġ (���� ����)
    private float initialZ; // ī�޶��� �ʱ� Z ��ġ (���� ����)

    void Start()
    {
        // ���� ����� �������� �ʾҴٸ� ��� �޽����� ����մϴ�.
        if (target == null)
        {
            Debug.LogError("ī�޶� ���� ���(Target) Transform�� �������� �ʾҽ��ϴ�! �ν����Ϳ��� �÷��̾� ������Ʈ�� �������ּ���.");
            // �Ǵ� GameObject.FindGameObjectWithTag("Player") ���� ����Ͽ� �ڵ�� �÷��̾ ã�� ���� �ֽ��ϴ�.
        }

        // ��ũ��Ʈ ���� �� ī�޶��� �ʱ� Y, Z ��ġ�� �����صӴϴ�.
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