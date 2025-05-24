using UnityEngine;

public class RemoveWithLifeTime : MonoBehaviour // Ŭ���� �̸��� ��Ÿ ���� Remov_e_WithLifeTime���� �����߽��ϴ�.
{
    public int lifeTime = 60; // FixedUpdate ȣ�� Ƚ���� �������� �� ���� �ð�
    public bool useFadeout = false;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float initialLifeTime; // float���� �ʱ� ���� �ð��� �����Ͽ� ��Ȯ�� ���� ���

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ��������Ʈ ������ ������Ʈ ��������
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null && useFadeout)
        {
            Debug.LogWarning("SpriteRenderer component not found on " + gameObject.name + ". Fadeout will not work.");
            useFadeout = false; // ���̵�ƿ� ��Ȱ��ȭ
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        initialLifeTime = lifeTime; // �ʱ� ���� �ð� ����
    }

    private void FixedUpdate()
    {
        lifeTime--;

        if (useFadeout && spriteRenderer != null)
        {
            // lifetime�� 0�� ����������� ���� ���� ����
            if (lifeTime > 0 && initialLifeTime > 0) // lifeTime�� initialLifeTime�� 0���� Ŭ ���� ���
            {
                // ���� lifeTime ���� ��� (0.0 ~ 1.0)
                float alphaRatio = (float)lifeTime / initialLifeTime;
                // ���ο� ���� ���� (���� ������ RGB ���� �����ϰ� ���� ���� ����)
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, originalColor.a * alphaRatio);
            }
            else if (lifeTime <= 0) // lifeTime�� 0 ���ϰ� �Ǹ� ������ �����ϰ� ó�� (���� ����)
            {
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
            }
        }

        if (lifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // FixedUpdate���� ���� �ð� ������ �ϹǷ� Update�� ����Ӵϴ�.
    }
}