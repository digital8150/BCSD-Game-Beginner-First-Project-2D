using UnityEngine;
using System.Collections.Generic;


public class AudioManager : SingleTone<AudioManager>
{
    private AudioSource audioSource; // ���� ����� ���� AudioSource

    // Inspector���� �Ҵ��� ����� Ŭ����
    [Header("Audio Clips")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip playerAttackSound;
    [SerializeField] private AudioClip enemyHitSound;
    [SerializeField] private AudioClip buttonClickSound;
    // ... �ʿ��� ��ŭ ����� Ŭ�� �߰�


    public override void Awake()
    {
        base.Awake();

        // AudioSource ������Ʈ �������� �Ǵ� �߰��ϱ�
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        PlayMusic(backgroundMusic);
        CameraEffectManager.Instance.OnSlowMotionAudio += OnSlowMotionAudio;
    }

    /// <summary>
    /// ������ �̸��� ���� ȿ���� �� �� ����մϴ�.
    /// </summary>
    /// <param name="audioname">����� ������� �̸�</param>
    public void PlaySound(string audioname)
    {
        AudioClip clipToPlay = null;

        // ��� 1: Switch �� ��� (SerializeField�� ���� AudioClip ���� ��� ��)
        switch (audioname)
        {
            case "PlayerAttack":
                clipToPlay = playerAttackSound;
                break;
            case "EnemyHit":
                clipToPlay = enemyHitSound;
                break;
            case "ButtonClick":
                clipToPlay = buttonClickSound;
                break;
            // ... �ٸ� ���� ���̽��� �߰�
            default:
                Debug.LogWarning($"��ȿ���� ���� ����� ��û�̰ų� Dictionary�� ���� �����Դϴ�: {audioname}");
                return;
        }

        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay); // PlayOneShot�� ���� ���带 ���ļ� ��� ����
        }
    }

    /// <summary>
    /// ��� ���� �Ǵ� �ݺ� ����� ������ �����ϰ� ����մϴ�.
    /// </summary>
    /// <param name="musicClip">����� ���� Ŭ��</param>
    /// <param name="loop">�ݺ� ��� ����</param>
    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicClip != null)
        {
            audioSource.clip = musicClip;
            audioSource.loop = loop;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("����� Music Clip�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }

    private void OnDisable()
    {
        CameraEffectManager.Instance.OnSlowMotionAudio -= OnSlowMotionAudio;
    }

    void OnSlowMotionAudio()
    {
        GetComponent<AudioSource>().pitch = Time.timeScale;
    }
}