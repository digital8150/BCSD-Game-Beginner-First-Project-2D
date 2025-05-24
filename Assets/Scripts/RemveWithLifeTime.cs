using UnityEngine;

public class RemoveWithLifeTime : MonoBehaviour // 클래스 이름을 오타 없이 Remov_e_WithLifeTime으로 수정했습니다.
{
    public int lifeTime = 60; // FixedUpdate 호출 횟수를 기준으로 한 생존 시간
    public bool useFadeout = false;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float initialLifeTime; // float으로 초기 생명 시간을 저장하여 정확한 비율 계산

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 스프라이트 렌더러 컴포넌트 가져오기
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null && useFadeout)
        {
            Debug.LogWarning("SpriteRenderer component not found on " + gameObject.name + ". Fadeout will not work.");
            useFadeout = false; // 페이드아웃 비활성화
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        initialLifeTime = lifeTime; // 초기 생명 시간 저장
    }

    private void FixedUpdate()
    {
        lifeTime--;

        if (useFadeout && spriteRenderer != null)
        {
            // lifetime이 0에 가까워질수록 투명도 감소 적용
            if (lifeTime > 0 && initialLifeTime > 0) // lifeTime과 initialLifeTime이 0보다 클 때만 계산
            {
                // 현재 lifeTime 비율 계산 (0.0 ~ 1.0)
                float alphaRatio = (float)lifeTime / initialLifeTime;
                // 새로운 색상 설정 (기존 색상의 RGB 값은 유지하고 알파 값만 변경)
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, originalColor.a * alphaRatio);
            }
            else if (lifeTime <= 0) // lifeTime이 0 이하가 되면 완전히 투명하게 처리 (선택 사항)
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
        // FixedUpdate에서 생명 시간 관리를 하므로 Update는 비워둡니다.
    }
}