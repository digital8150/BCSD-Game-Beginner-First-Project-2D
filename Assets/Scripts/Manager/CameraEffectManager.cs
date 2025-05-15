using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Collections;

public class CameraEffectManager : SingleTone<CameraEffectManager>
{
    [Header("색수차 효과 설정")] // 색수차 효과 관련 헤더 추가
    [SerializeField]
    Camera mainCamera; // 메인 카메라 참조 (인스펙터에서 연결 필요)
    private Volume globalVolume; // 씬의 Global Volume 참조
    private ChromaticAberration cb; // 색수차 효과 컴포넌트 참조
    public float maxChromaticAberrationIntensity = 0.5f; // 발사 시 색수차 최대 강도
    public float chromaticAberrationDuration = 0.5f; // 색수차 효과 지속 시간
    private Coroutine fadeCoroutine; // 색수차 페이드 코루틴 참조


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

    public void ApplyCromaticAbb()
    {
        // ==== 발포 시 색수차 효과 적용 ====
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
        // ==================================
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
        
    }
}
