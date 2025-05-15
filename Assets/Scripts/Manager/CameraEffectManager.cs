using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Collections;

public class CameraEffectManager : SingleTone<CameraEffectManager>
{
    [Header("������ ȿ�� ����")] // ������ ȿ�� ���� ��� �߰�
    [SerializeField]
    Camera mainCamera; // ���� ī�޶� ���� (�ν����Ϳ��� ���� �ʿ�)
    private Volume globalVolume; // ���� Global Volume ����
    private ChromaticAberration cb; // ������ ȿ�� ������Ʈ ����
    public float maxChromaticAberrationIntensity = 0.5f; // �߻� �� ������ �ִ� ����
    public float chromaticAberrationDuration = 0.5f; // ������ ȿ�� ���� �ð�
    private Coroutine fadeCoroutine; // ������ ���̵� �ڷ�ƾ ����


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

    public void ApplyCromaticAbb()
    {
        // ==== ���� �� ������ ȿ�� ���� ====
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
        // ==================================
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
        
    }
}
