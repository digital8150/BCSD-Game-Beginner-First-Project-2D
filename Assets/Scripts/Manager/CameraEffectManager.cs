using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Collections;

public class CameraEffectManager : SingleTone<CameraEffectManager>
{
    [Header("��� ī�޶� ����")]
    public Camera mainCamera; // ���� ī�޶� ���� (�ν����Ϳ��� ���� �ʿ�)

    private Volume globalVolume; // ���� Global Volume ����
    private ChromaticAberration cb; // ������ ȿ�� ������Ʈ ����
    private ColorAdjustments colorAdjustments; // ���� ���� ������Ʈ ����

    [Header("������ ȿ�� ����")] // ������ ȿ�� ���� ��� �߰�
    public float maxChromaticAberrationIntensity = 0.5f; // �߻� �� ������ �ִ� ����
    public float chromaticAberrationDuration = 0.5f; // ������ ȿ�� ���� �ð�
    private Coroutine fadeCoroutine; // ������ ���̵� �ڷ�ƾ ����

    [Header("ī�޶� ��鸲 ȿ�� ����")]
    [SerializeField]
    private float shakeDuration = 0.5f; // ��鸲 ���� �ð�
    [SerializeField]
    private float shakeMagnitude = 0.1f; // ��鸲 ����
    private Vector3 originalPosition; // ���� ī�޶� ��ġ ����

    [Header("���� ���� ȿ�� ����")]
    [SerializeField]
    private float colorAdjustmentDuration = 0.5f; // ���� ���� ȿ�� ���� �ð�
    [SerializeField]
    private float postExposureBoostValue = 0.5f; // ���� ���� ȿ��
    [SerializeField]
    private float saturationBoostValue = 32.5f; // ���� ���� ȿ��
    private Coroutine fadePEBCoroutine;
    private Coroutine fadeSBCoroutine;
    private float originalSaturationValue; // ���� ���� ���� �� ���� 

    [Header("���ο� ��� ȿ�� ����")]
    public float slowFactor = 0.05f;
    public float slowLength = 4f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public override void Awake()
    {
        // Global Volume �� Chromatic Aberration ������Ʈ ��������
        if (mainCamera != null)
        {
            globalVolume = mainCamera.GetComponent<Volume>();
            if (globalVolume != null)
            {
                // Volume �������Ͽ��� ChromaticAberration ���� ��������
                if (globalVolume.profile.TryGet(out cb))
                {
                    // ChromaticAberration ������Ʈ�� ã�Ұ� Ȱ��ȭ�Ǿ� �ִٸ� �ʱ� ������ 0���� ����
                    if (cb.active)
                    {
                        cb.intensity.value = 0f;
                    }
                    else
                    {
                        Debug.LogWarning("ChromaticAberration ȿ���� Volume �������Ͽ� ������ Ȱ��ȭ�Ǿ� ���� �ʽ��ϴ�.");
                    }
                }
                else
                {
                    Debug.LogWarning("Volume �������Ͽ��� ChromaticAberration ȿ���� ã�� �� �����ϴ�.");
                }

                // Volume �������Ͽ��� ColorAdjustments ���� ��������
                if (globalVolume.profile.TryGet(out colorAdjustments))
                {
                    // ColorAdjustments ������Ʈ�� ã�Ұ� Ȱ��ȭ�Ǿ� �ִٸ� �ʱ� ���� ���� �� ����
                    if (colorAdjustments.active)
                    {
                        colorAdjustments.postExposure.value = 0f;
                        colorAdjustments.saturation.value = 0f;
                    }
                    else
                    {
                        Debug.LogWarning("ColorAdjustments ȿ���� Volume �������Ͽ� ������ Ȱ��ȭ�Ǿ� ���� �ʽ��ϴ�.");
                    }
                }
                else
                {
                    Debug.LogWarning("Volume �������Ͽ��� ColorAdjustments ȿ���� ã�� �� �����ϴ�.");
                }
            }
            else
            {
                Debug.LogWarning("���� ī�޶� Volume ������Ʈ�� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogError("���� ī�޶�(Main Camera) ������ Player ��ũ��Ʈ�� ������� �ʾҽ��ϴ�!");
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
        if (colorAdjustments != null && colorAdjustments.active) // ChromaticAberration ������Ʈ�� ��ȿ�ϰ� Ȱ��ȭ�Ǿ� ���� ���� ����
        {
            // ���� ���� �� ����
            colorAdjustments.saturation.value = value;
        }
    }

    public void ApplySaturationBoost()
    {
        if (colorAdjustments != null && colorAdjustments.active) // ChromaticAberration ������Ʈ�� ��ȿ�ϰ� Ȱ��ȭ�Ǿ� ���� ���� ����
        {
            // ���� ���̵� �ڷ�ƾ�� �ִٸ� ����
            if (fadePEBCoroutine != null)
            {
                StopCoroutine(fadeSBCoroutine);
            }

            originalSaturationValue = colorAdjustments.saturation.value; // ���� ���� ���� �� ����
            //���� �ν�Ʈ
            colorAdjustments.saturation.value = saturationBoostValue;
            // ���̵� �ƿ� �ڷ�ƾ ����
            fadeSBCoroutine = StartCoroutine(FadeSaturation(colorAdjustmentDuration));
        }
    }

    IEnumerator FadeSaturation(float duration)
    {
        // ChromaticAberration ������Ʈ�� ��ȿ���� �ٽ� Ȯ�� (�� ��ȯ ������ ���� null�� �� ���� �����Ƿ�)
        if (colorAdjustments == null || !colorAdjustments.active) yield break; // ��ȿ���� ������ �ڷ�ƾ ����

        float startIntensity = colorAdjustments.saturation.value; // ���� ���� (�ִ� ����)
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            // ��� �ð��� ����Ͽ� ������ 0���� ���� ����
            float currentIntensity = Mathf.Lerp(startIntensity, originalSaturationValue, timer / duration);
            colorAdjustments.saturation.value = currentIntensity;
            yield return null; // ���� �����ӱ��� ���
        }

        // ���̵� �Ϸ� �� ������ 0���� ��Ȯ�� ����
        colorAdjustments.saturation.value = originalSaturationValue;
        fadeSBCoroutine = null; // �ڷ�ƾ ���� ����
    }

    public void ApplyPostExposureBoost()
    {
        if (colorAdjustments != null && colorAdjustments.active) // ChromaticAberration ������Ʈ�� ��ȿ�ϰ� Ȱ��ȭ�Ǿ� ���� ���� ����
        {
            // ���� ���̵� �ڷ�ƾ�� �ִٸ� ����
            if (fadePEBCoroutine != null)
            {
                StopCoroutine(fadePEBCoroutine);
            }
            
            //���� �ν�Ʈ
            colorAdjustments.postExposure.value = postExposureBoostValue;
            // ���̵� �ƿ� �ڷ�ƾ ����
            fadePEBCoroutine = StartCoroutine(FadePostExposure(colorAdjustmentDuration));
        }
    }
    
    IEnumerator FadePostExposure(float duration)
    {
        // ChromaticAberration ������Ʈ�� ��ȿ���� �ٽ� Ȯ�� (�� ��ȯ ������ ���� null�� �� ���� �����Ƿ�)
        if (colorAdjustments == null || !colorAdjustments.active) yield break; // ��ȿ���� ������ �ڷ�ƾ ����

        float startIntensity = colorAdjustments.postExposure.value; // ���� ���� (�ִ� ����)
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            // ��� �ð��� ����Ͽ� ������ 0���� ���� ����
            float currentIntensity = Mathf.Lerp(startIntensity, 0f, timer / duration);
            colorAdjustments.postExposure.value = currentIntensity;
            yield return null; // ���� �����ӱ��� ���
        }

        // ���̵� �Ϸ� �� ������ 0���� ��Ȯ�� ����
        colorAdjustments.postExposure.value = 0f;
        fadePEBCoroutine = null; // �ڷ�ƾ ���� ����
    }

    public void ApplyCameraShake()
    {
        // ī�޶��� ���� ��ġ�� ����
        originalPosition = mainCamera.transform.localPosition;
        // ī�޶� ��鸲 �ڷ�ƾ ����
        StartCoroutine(ShakeCamera(shakeDuration));

    }

    public void ApplyCameraShake(float duration, float magnitude)
    {
        // ī�޶��� ���� ��ġ�� ����
        originalPosition = mainCamera.transform.localPosition;
        // ī�޶� ��鸲 �ڷ�ƾ ����
        StartCoroutine(ShakeCamera(duration, magnitude));
    }

    IEnumerator ShakeCamera(float shakeDuration)
    {
        float elapsed = 0.0f; // ��� �ð� �ʱ�ȭ
        while (elapsed < shakeDuration)
        {
            // ī�޶��� ��ġ�� �����ϰ� �����Ͽ� ��鸲 ȿ�� ����
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            mainCamera.transform.localPosition = new Vector3(mainCamera.transform.position.x + x, mainCamera.transform.position.y + y, originalPosition.z);
            elapsed += Time.deltaTime; // ��� �ð� ����
            yield return null; // ���� �����ӱ��� ���
        }
        // ��鸲�� ���� �� ���� ��ġ�� ����
        //mainCamera.transform.localPosition = originalPosition;
    }

    IEnumerator ShakeCamera(float shakeDuration, float magnitude)
    {
        float elapsed = 0.0f; // ��� �ð� �ʱ�ȭ
        while (elapsed < shakeDuration)
        {
            // ī�޶��� ��ġ�� �����ϰ� �����Ͽ� ��鸲 ȿ�� ����
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            mainCamera.transform.localPosition = new Vector3(mainCamera.transform.position.x + x, mainCamera.transform.position.y + y, originalPosition.z);
            elapsed += Time.deltaTime; // ��� �ð� ����
            yield return null; // ���� �����ӱ��� ���
        }
        // ��鸲�� ���� �� ���� ��ġ�� ����
        mainCamera.transform.localPosition = originalPosition;
    }

    public void ApplyCromaticAbb()
    {
        if (cb != null && cb.active) // ChromaticAberration ������Ʈ�� ��ȿ�ϰ� Ȱ��ȭ�Ǿ� ���� ���� ����
        {
            // ���� ���̵� �ڷ�ƾ�� �ִٸ� ����
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            // ������ ������ �ִ�� ����
            cb.intensity.value = maxChromaticAberrationIntensity;
            // ������ ���̵� �ƿ� �ڷ�ƾ ����
            fadeCoroutine = StartCoroutine(FadeChromaticAberration(chromaticAberrationDuration));
        }
    }

    // ������ ȿ���� ���� ���̴� �ڷ�ƾ
    IEnumerator FadeChromaticAberration(float duration)
    {
        // ChromaticAberration ������Ʈ�� ��ȿ���� �ٽ� Ȯ�� (�� ��ȯ ������ ���� null�� �� ���� �����Ƿ�)
        if (cb == null || !cb.active) yield break; // ��ȿ���� ������ �ڷ�ƾ ����

        float startIntensity = cb.intensity.value; // ���� ���� (�ִ� ����)
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            // ��� �ð��� ����Ͽ� ������ 0���� ���� ����
            float currentIntensity = Mathf.Lerp(startIntensity, 0f, timer / duration);
            cb.intensity.value = currentIntensity;
            yield return null; // ���� �����ӱ��� ���
        }

        // ���̵� �Ϸ� �� ������ 0���� ��Ȯ�� ����
        cb.intensity.value = 0f;
        fadeCoroutine = null; // �ڷ�ƾ ���� ����
    }

    // Update is called once per frame
    void Update()
    {

        Time.timeScale += (1f / slowLength) * Time.unscaledDeltaTime;
        Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }
}
