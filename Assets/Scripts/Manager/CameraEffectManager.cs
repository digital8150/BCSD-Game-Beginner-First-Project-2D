using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Collections;

public class CameraEffectManager : SingleTone<CameraEffectManager>
{
    [Header("대상 카메라 지정")]
    public Camera mainCamera; // 메인 카메라 참조 (인스펙터에서 연결 필요)

    private Volume globalVolume; // 씬의 Global Volume 참조
    private ChromaticAberration cb; // 색수차 효과 컴포넌트 참조
    private ColorAdjustments colorAdjustments; // 색상 조정 컴포넌트 참조

    [Header("색수차 효과 설정")] // 색수차 효과 관련 헤더 추가
    public float maxChromaticAberrationIntensity = 0.5f; // 발사 시 색수차 최대 강도
    public float chromaticAberrationDuration = 0.5f; // 색수차 효과 지속 시간
    private Coroutine fadeCoroutine; // 색수차 페이드 코루틴 참조

    [Header("카메라 흔들림 효과 설정")]
    [SerializeField]
    private float shakeDuration = 0.5f; // 흔들림 지속 시간
    [SerializeField]
    private float shakeMagnitude = 0.1f; // 흔들림 강도
    private Vector3 originalPosition; // 원래 카메라 위치 저장

    [Header("색상 조정 효과 설정")]
    [SerializeField]
    private float colorAdjustmentDuration = 0.5f; // 색상 조정 효과 지속 시간
    [SerializeField]
    private float postExposureBoostValue = 0.5f; // 색상 조정 효과
    [SerializeField]
    private float saturationBoostValue = 32.5f; // 색상 조정 효과
    private Coroutine fadePEBCoroutine;
    private Coroutine fadeSBCoroutine;
    private float originalSaturationValue; // 원래 색상 조정 값 저장 

    [Header("슬로우 모션 효과 설정")]
    public float slowFactor = 0.05f;
    public float slowLength = 4f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public override void Awake()
    {
        // Global Volume 및 Chromatic Aberration 컴포넌트 가져오기
        if (mainCamera != null)
        {
            globalVolume = mainCamera.GetComponent<Volume>();
            if (globalVolume != null)
            {
                // Volume 프로파일에서 ChromaticAberration 설정 가져오기
                if (globalVolume.profile.TryGet(out cb))
                {
                    // ChromaticAberration 컴포넌트를 찾았고 활성화되어 있다면 초기 강도를 0으로 설정
                    if (cb.active)
                    {
                        cb.intensity.value = 0f;
                    }
                    else
                    {
                        Debug.LogWarning("ChromaticAberration 효과가 Volume 프로파일에 있지만 활성화되어 있지 않습니다.");
                    }
                }
                else
                {
                    Debug.LogWarning("Volume 프로파일에서 ChromaticAberration 효과를 찾을 수 없습니다.");
                }

                // Volume 프로파일에서 ColorAdjustments 설정 가져오기
                if (globalVolume.profile.TryGet(out colorAdjustments))
                {
                    // ColorAdjustments 컴포넌트를 찾았고 활성화되어 있다면 초기 색상 조정 값 설정
                    if (colorAdjustments.active)
                    {
                        colorAdjustments.postExposure.value = 0f;
                        colorAdjustments.saturation.value = 0f;
                    }
                    else
                    {
                        Debug.LogWarning("ColorAdjustments 효과가 Volume 프로파일에 있지만 활성화되어 있지 않습니다.");
                    }
                }
                else
                {
                    Debug.LogWarning("Volume 프로파일에서 ColorAdjustments 효과를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("메인 카메라에 Volume 컴포넌트가 없습니다.");
            }
        }
        else
        {
            Debug.LogError("메인 카메라(Main Camera) 참조가 Player 스크립트에 연결되지 않았습니다!");
        }


        base.Awake();
    }

    public void ApplySlowMotion()
    {
        Time.timeScale = slowFactor;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }

    public void SetSaturation(float value)
    {
        if (colorAdjustments != null && colorAdjustments.active) // ChromaticAberration 컴포넌트가 유효하고 활성화되어 있을 때만 적용
        {
            // 색상 조정 값 설정
            colorAdjustments.saturation.value = value;
        }
    }

    public void ApplySaturationBoost()
    {
        if (colorAdjustments != null && colorAdjustments.active) // ChromaticAberration 컴포넌트가 유효하고 활성화되어 있을 때만 적용
        {
            // 기존 페이드 코루틴이 있다면 중지
            if (fadePEBCoroutine != null)
            {
                StopCoroutine(fadeSBCoroutine);
            }

            originalSaturationValue = colorAdjustments.saturation.value; // 원래 색상 조정 값 저장
            //색상 부스트
            colorAdjustments.saturation.value = saturationBoostValue;
            // 페이드 아웃 코루틴 시작
            fadeSBCoroutine = StartCoroutine(FadeSaturation(colorAdjustmentDuration));
        }
    }

    IEnumerator FadeSaturation(float duration)
    {
        // ChromaticAberration 컴포넌트가 유효한지 다시 확인 (씬 전환 등으로 인해 null이 될 수도 있으므로)
        if (colorAdjustments == null || !colorAdjustments.active) yield break; // 유효하지 않으면 코루틴 종료

        float startIntensity = colorAdjustments.saturation.value; // 시작 강도 (최대 강도)
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            // 경과 시간에 비례하여 강도를 0으로 선형 보간
            float currentIntensity = Mathf.Lerp(startIntensity, originalSaturationValue, timer / duration);
            colorAdjustments.saturation.value = currentIntensity;
            yield return null; // 다음 프레임까지 대기
        }

        // 페이드 완료 후 강도를 0으로 정확히 설정
        colorAdjustments.saturation.value = originalSaturationValue;
        fadeSBCoroutine = null; // 코루틴 참조 해제
    }

    public void ApplyPostExposureBoost()
    {
        if (colorAdjustments != null && colorAdjustments.active) // ChromaticAberration 컴포넌트가 유효하고 활성화되어 있을 때만 적용
        {
            // 기존 페이드 코루틴이 있다면 중지
            if (fadePEBCoroutine != null)
            {
                StopCoroutine(fadePEBCoroutine);
            }
            
            //색상 부스트
            colorAdjustments.postExposure.value = postExposureBoostValue;
            // 페이드 아웃 코루틴 시작
            fadePEBCoroutine = StartCoroutine(FadePostExposure(colorAdjustmentDuration));
        }
    }
    
    IEnumerator FadePostExposure(float duration)
    {
        // ChromaticAberration 컴포넌트가 유효한지 다시 확인 (씬 전환 등으로 인해 null이 될 수도 있으므로)
        if (colorAdjustments == null || !colorAdjustments.active) yield break; // 유효하지 않으면 코루틴 종료

        float startIntensity = colorAdjustments.postExposure.value; // 시작 강도 (최대 강도)
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            // 경과 시간에 비례하여 강도를 0으로 선형 보간
            float currentIntensity = Mathf.Lerp(startIntensity, 0f, timer / duration);
            colorAdjustments.postExposure.value = currentIntensity;
            yield return null; // 다음 프레임까지 대기
        }

        // 페이드 완료 후 강도를 0으로 정확히 설정
        colorAdjustments.postExposure.value = 0f;
        fadePEBCoroutine = null; // 코루틴 참조 해제
    }

    public void ApplyCameraShake()
    {
        // 카메라의 원래 위치를 저장
        originalPosition = mainCamera.transform.localPosition;
        // 카메라 흔들림 코루틴 시작
        StartCoroutine(ShakeCamera(shakeDuration));

    }

    public void ApplyCameraShake(float duration, float magnitude)
    {
        // 카메라의 원래 위치를 저장
        originalPosition = mainCamera.transform.localPosition;
        // 카메라 흔들림 코루틴 시작
        StartCoroutine(ShakeCamera(duration, magnitude));
    }

    IEnumerator ShakeCamera(float shakeDuration)
    {
        float elapsed = 0.0f; // 경과 시간 초기화
        while (elapsed < shakeDuration)
        {
            // 카메라의 위치를 랜덤하게 변경하여 흔들림 효과 생성
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            mainCamera.transform.localPosition = new Vector3(mainCamera.transform.position.x + x, mainCamera.transform.position.y + y, originalPosition.z);
            elapsed += Time.deltaTime; // 경과 시간 증가
            yield return null; // 다음 프레임까지 대기
        }
        // 흔들림이 끝난 후 원래 위치로 복원
        //mainCamera.transform.localPosition = originalPosition;
    }

    IEnumerator ShakeCamera(float shakeDuration, float magnitude)
    {
        float elapsed = 0.0f; // 경과 시간 초기화
        while (elapsed < shakeDuration)
        {
            // 카메라의 위치를 랜덤하게 변경하여 흔들림 효과 생성
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            mainCamera.transform.localPosition = new Vector3(mainCamera.transform.position.x + x, mainCamera.transform.position.y + y, originalPosition.z);
            elapsed += Time.deltaTime; // 경과 시간 증가
            yield return null; // 다음 프레임까지 대기
        }
        // 흔들림이 끝난 후 원래 위치로 복원
        mainCamera.transform.localPosition = originalPosition;
    }

    public void ApplyCromaticAbb()
    {
        if (cb != null && cb.active) // ChromaticAberration 컴포넌트가 유효하고 활성화되어 있을 때만 적용
        {
            // 기존 페이드 코루틴이 있다면 중지
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            // 색수차 강도를 최대로 설정
            cb.intensity.value = maxChromaticAberrationIntensity;
            // 색수차 페이드 아웃 코루틴 시작
            fadeCoroutine = StartCoroutine(FadeChromaticAberration(chromaticAberrationDuration));
        }
    }

    // 색수차 효과를 점차 줄이는 코루틴
    IEnumerator FadeChromaticAberration(float duration)
    {
        // ChromaticAberration 컴포넌트가 유효한지 다시 확인 (씬 전환 등으로 인해 null이 될 수도 있으므로)
        if (cb == null || !cb.active) yield break; // 유효하지 않으면 코루틴 종료

        float startIntensity = cb.intensity.value; // 시작 강도 (최대 강도)
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            // 경과 시간에 비례하여 강도를 0으로 선형 보간
            float currentIntensity = Mathf.Lerp(startIntensity, 0f, timer / duration);
            cb.intensity.value = currentIntensity;
            yield return null; // 다음 프레임까지 대기
        }

        // 페이드 완료 후 강도를 0으로 정확히 설정
        cb.intensity.value = 0f;
        fadeCoroutine = null; // 코루틴 참조 해제
    }

    // Update is called once per frame
    void Update()
    {

        Time.timeScale += (1f / slowLength) * Time.unscaledDeltaTime;
        Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }
}
