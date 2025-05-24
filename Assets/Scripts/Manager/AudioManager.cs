using UnityEngine;
using System.Collections.Generic;


public class AudioManager : SingleTone<AudioManager>
{
    private AudioSource audioSource; // 사운드 재생을 위한 AudioSource

    // Inspector에서 할당할 오디오 클립들
    [Header("Audio Clips")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip playerAttackSound;
    [SerializeField] private AudioClip enemyHitSound;
    [SerializeField] private AudioClip buttonClickSound;
    // ... 필요한 만큼 오디오 클립 추가


    public override void Awake()
    {
        base.Awake();

        // AudioSource 컴포넌트 가져오기 또는 추가하기
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
    /// 지정된 이름의 사운드 효과를 한 번 재생합니다.
    /// </summary>
    /// <param name="audioname">재생할 오디오의 이름</param>
    public void PlaySound(string audioname)
    {
        AudioClip clipToPlay = null;

        // 방법 1: Switch 문 사용 (SerializeField된 개별 AudioClip 변수 사용 시)
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
            // ... 다른 사운드 케이스들 추가
            default:
                Debug.LogWarning($"유효하지 않은 오디오 요청이거나 Dictionary에 없는 사운드입니다: {audioname}");
                return;
        }

        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay); // PlayOneShot은 여러 사운드를 겹쳐서 재생 가능
        }
    }

    /// <summary>
    /// 배경 음악 또는 반복 재생할 음악을 설정하고 재생합니다.
    /// </summary>
    /// <param name="musicClip">재생할 음악 클립</param>
    /// <param name="loop">반복 재생 여부</param>
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
            Debug.LogWarning("재생할 Music Clip이 할당되지 않았습니다.");
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