using TMPro;
using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
public class GameManager : SingleTone<GameManager>
{
    [Header("UI 설정")]
    [SerializeField]
    GameObject staminaSlider;
    [SerializeField]
    GameObject healthSlider;
    [SerializeField]
    TextMeshProUGUI scoreText;
    [SerializeField]
    GameObject gameOver;
    [SerializeField]
    GameObject resume;
    [SerializeField]
    TextMeshProUGUI gameOverScoreText;
    [SerializeField]
    TextMeshProUGUI captionText;
    [SerializeField]
    GameObject bossHPSlider;
    [Header("오브젝트 연결")]
    public Camera mainCamera;
    [SerializeField]
    public Player player;
    [SerializeField]
    Canvas overlay;

    [Header("적 스폰 설정")]
    [SerializeField]
    GameObject enemyPrefab1;
    [SerializeField]
    GameObject enemyPrefab2;
    [SerializeField]
    GameObject bossPrefab;
    [SerializeField]
    GameObject hpSliderPrefab;
    [SerializeField]
    int maxEnemyCount = 20;
    public List<Enemy> enemies = new List<Enemy>();
    public int CurrentEnemyCount { get; set; } = 0;
    private float enemySpawnTimer = 0f;
    [SerializeField]
    private float enemySpawnInterval = 5f;
    private float enemySpawnIntervalInitial;

    [Header("점수 설정")]
    [SerializeField]
    private float scoreInterval = 1f; // 점수 증가 주기
    private float scoreTimer = 0f; // 점수 증가 타이머
    public int score = 0;
    private int maxScore = 0;
    public StageInfo stageInfo;

    private Vector3 playerStartPos;

    
    public override void Awake()
    {
        base.Awake();
        enemySpawnIntervalInitial = enemySpawnInterval;
        staminaSlider.GetComponent<PositionAutoSetter>().Setup(player.transform);
        healthSlider.GetComponent<PositionAutoSetter>().Setup(player.transform);
        gameOver.gameObject.SetActive(false);
        resume.gameObject.SetActive(false);
        score = 0;
        playerStartPos = player.transform.position;

        
    }

    private void Update()
    {
        //일시정지 메뉴
        if(Input.GetButtonDown("Cancel") && !player.IsDead)
        {
            
            if (Time.timeScale == 1)
            {
                CameraEffectManager.Instance.isPaused = true;
                Time.timeScale = 0;
                resume.SetActive(true);
            }
            else
            {
                CameraEffectManager.Instance.isPaused = false;
                Time.timeScale = 1;
                resume.SetActive(false);
            }
        }

        //생존점수 부여
        if (Time.time - scoreTimer >= scoreInterval && !player.IsDead)
        {
            score += 50;
            scoreTimer = Time.time;
        }


    }

    private void FixedUpdate()
    {
        //플레이어 사망 상태에서는 소환 X
        if (player.IsDead)
        {
            foreach (var enemy in enemies)
            {
                enemy.kill();

            }
            enemies.RemoveAll(enemy => true);
            if(SpawnedBoss != null) SpawnedBoss.GetComponent<Enemy>().kill();
            return;
        }

        // n초마다, 적 생성
        enemySpawnTimer += Time.fixedDeltaTime;
        if (CurrentEnemyCount < maxEnemyCount && enemySpawnTimer >= (enemySpawnInterval + Random.Range(0.0f, 1.25f)) && SpawnedBoss == null)
        {

            Camera cam = mainCamera.GetComponent<Camera>();
            float camHeight = 2f * cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            Vector3 camPos = cam.transform.position;

            float spawnX = Random.Range(-1, 1) < 0 ? camPos.x - camWidth / 2f - 2 : camPos.x + camWidth / 2f + 2;
            float spawnY = camPos.y;

            var clone = Instantiate(Random.Range(-1, 1) < 0 ? enemyPrefab1 : enemyPrefab2, new Vector3(spawnX, spawnY, 0), Quaternion.identity);
            enemies.Add(clone.GetComponent<Enemy>());
            clone.GetComponent<Enemy>().Setup(player.gameObject);

            //HP Slider 생성 및 할당
            var hpslider = Instantiate(hpSliderPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            hpslider.transform.SetParent(overlay.transform);
            hpslider.transform.localScale = Vector3.one;
            hpslider.GetComponent<EnemyHPSlider>().Setup(clone);
            hpslider.GetComponent<PositionAutoSetter>().Setup(clone.transform);


            CurrentEnemyCount++;
            enemySpawnTimer = 0f;
            enemySpawnInterval *= 0.96f;
        }
        
        if (score > stageInfo.BossThresholdScore)
        {
            //보스 소환
            SpawnBoss();
        }


    }

    GameObject SpawnedBoss;
    void SpawnBoss()
    {
        if(SpawnedBoss != null) return;
        SpawnedBoss = Instantiate(bossPrefab, new Vector3(-22, 3, 0), Quaternion.identity);
        SpawnedBoss.GetComponent<Enemy>().Setup(player.gameObject);
        CameraEffectManager.Instance.ApplyCameraShake();
        ShowCaption("강력한 적이 등장합니다 조심하세요!");
        stageInfo.increaseBossThreshold();
        bossHPSlider.GetComponent<EnemyHPSlider>().Setup(SpawnedBoss, true);
        bossHPSlider.SetActive(true);
    }

    private void LateUpdate()
    {
        if(player.IsDead)
        {
            staminaSlider.SetActive(false);
            healthSlider.SetActive(false);
            scoreText.gameObject.SetActive(false);
            gameOver.SetActive(true);
            maxScore = Mathf.Max(maxScore, score);
            gameOverScoreText.text = $"현재 기록 : {score}\n최고 기록 : {maxScore}";
            return;
        }

        if (player.CurrentStamina < player.MaxStamina)
        {
            staminaSlider.SetActive(true);
            staminaSlider.GetComponent<Slider>().value = player.CurrentStamina / player.MaxStamina;
        }
        else
        {
            staminaSlider.SetActive(false);
        }

        //플레이어 체력
        if (player.CurrentHealth < player.maxHealth)
        {
            healthSlider.SetActive(true);
            healthSlider.GetComponent<Slider>().value = (float)player.CurrentHealth / (float)player.maxHealth;
        }
        else
        {
            healthSlider.SetActive(false);
        }

        if (scoreText != null)
        {
            scoreText.text = "점수\n" + score.ToString();
        }
    }

    //Button Functions
    public void OnClickReturnToMenu()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
#endif
    }

    public void OnClickRestart()
    {
        foreach (var enemy in enemies)
        {
            enemy.kill();
        }
        enemies.RemoveAll(enemy => true);
        if (SpawnedBoss != null) SpawnedBoss.GetComponent<Enemy>().kill();



        OnClickResume();
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        player.transform.position = playerStartPos;
        player.gameObject.SetActive(true);
        player.IsDead = false;
        player.CurrentHealth = player.maxHealth;
        player.CurrentStamina = player.MaxStamina;
        stageInfo.ResetBossThreshold();
        scoreText.gameObject.SetActive(true);
        gameOver.SetActive(false);
        resume.SetActive(false);
        enemySpawnInterval = enemySpawnIntervalInitial;
        CameraEffectManager.Instance.SetSaturation(0);
        score = 0;
        scoreTimer = Time.time;
    }

    public void OnClickResume() 
    {
        CameraEffectManager.Instance.isPaused = false;
        Time.timeScale = 1;
        resume.SetActive(false);
    }

    public async void ShowCaption(string contnet)
    {
        captionText.faceColor = new Color(1, 1, 1, 0);
        captionText.gameObject.SetActive(true);
        captionText.text = contnet;

        //fadein
        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            captionText.faceColor = new Color(1, 1, 1, t);
            await System.Threading.Tasks.Task.Yield();
        }

        await System.Threading.Tasks.Task.Delay(1000);

        //fadeout
        for (float t = 1; t > 0; t -= Time.deltaTime)
        {
            captionText.faceColor = new Color(1, 1, 1, t);
            await System.Threading.Tasks.Task.Yield();
        }
        captionText.gameObject.SetActive(false);
    }
}
