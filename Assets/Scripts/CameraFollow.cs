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

    // LateUpdate�� ��� Update ȣ���� ���� �� ȣ��˴ϴ�.
    // ĳ������ ������ ����� �Ϸ�� �� ī�޶� ��ġ�� ������Ʈ�ϱ⿡ �����մϴ�.
    void LateUpdate()
    {
        // ���� ����� ������ �ƹ��͵� ���� �ʽ��ϴ�.
        if (target == null) return;

        // ī�޶� �̵��Ϸ��� ��ǥ ��ġ�� ����մϴ�.
        // ��ǥ X ��ġ�� �÷��̾��� ���� X ��ġ�� ����մϴ�.
        // Y ��ġ�� Start���� ������ �ʱ� Y ���� �״�� ����մϴ�.
        // Z ��ġ�� Start���� ������ �ʱ� Z ���� �״�� ����մϴ�.
        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y > initialY ? target.position.y : initialY, initialZ);

        // Vector3.SmoothDamp �Լ��� ����Ͽ� ���� ī�޶� ��ġ���� ��ǥ ��ġ(desiredPosition)���� �ε巴�� �̵��մϴ�.
        // �� �Լ��� ���� ��ġ, ��ǥ ��ġ, ���� �ӵ� (ref Ű���� ���), ��ǥ���� �ɸ��� ���� �ð�(smoothTime)�� ���ڷ� �޽��ϴ�.
        // �Լ��� ����� �ε巯�� ���� ��ġ ���� ��ȯ�մϴ�.
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

        // ���� �ε巯�� ��ġ�� ī�޶��� Transform.position�� ������Ʈ�մϴ�.
        transform.position = smoothedPosition;
    }
}