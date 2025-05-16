using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;

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
    [Header("오브젝트 연결")]
    [SerializeField]
    GameObject mainCamera;
    [SerializeField]
    Player player;
    [SerializeField]
    GameObject enemyPrefab1;
    [SerializeField]
    GameObject enemyPrefab2;
    [SerializeField]
    int maxEnemyCount = 20;
    public List<Enemy> enemies = new List<Enemy>();
    public int CurrentEnemyCount { get; set; } = 0;

    [Header("점수 설정")]
    [SerializeField]
    private float scoreInterval = 1f; // 점수 증가 주기
    private float scoreTimer = 0f; // 점수 증가 타이머
    public int score = 0;
    private int maxScore = 0;

    private Vector3 playerStartPos;

    
    public override void Awake()
    {
        staminaSlider.GetComponent<PositionAutoSetter>().Setup(player.transform);
        healthSlider.GetComponent<PositionAutoSetter>().Setup(player.transform);
        gameOver.gameObject.SetActive(false);
        resume.gameObject.SetActive(false);
        score = 0;
        playerStartPos = player.transform.position;
        base.Awake();
        
    }

    private void Update()
    {
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
    }

    private float enemySpawnTimer = 0f;
    private float enemySpawnInterval = 5f;

    private void FixedUpdate()
    {
        if (player.IsDead)
        {
            foreach (var enemy in enemies)
            {
                enemy.kill();
                
            }
            enemies.RemoveAll(enemy=>true);
            return;
        }



        if(Time.time - scoreTimer >= scoreInterval)
        {
            score += 50;
            scoreTimer = Time.time;
        }

        // n초마다, 적 생성
        enemySpawnTimer += Time.fixedDeltaTime;
        if (CurrentEnemyCount < maxEnemyCount && enemySpawnTimer >= enemySpawnInterval)
        {
            
            Camera cam = mainCamera.GetComponent<Camera>();
            float camHeight = 2f * cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            Vector3 camPos = cam.transform.position;
            
            float spawnX = Random.Range(-1,1) < 0 ?  camPos.x - camWidth / 2f - 2 : camPos.x + camWidth / 2f + 2;
            float spawnY = camPos.y;

            var clone = Instantiate(Random.Range(-1,1) < 0 ? enemyPrefab1 : enemyPrefab2, new Vector3(spawnX, spawnY, 0), Quaternion.identity);
            enemies.Add(clone.GetComponent<Enemy>());
            clone.GetComponent<Enemy>().Setup(player.gameObject);
            CurrentEnemyCount++;
            enemySpawnTimer = 0f;
            enemySpawnInterval = Random.Range(1f, 5f);
        }
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


        OnClickResume();
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        player.transform.position = playerStartPos;
        player.gameObject.SetActive(true);
        player.IsDead = false;
        player.CurrentHealth = player.maxHealth;
        player.CurrentStamina = player.MaxStamina;
        scoreText.gameObject.SetActive(true);
        gameOver.SetActive(false);
        resume.SetActive(false);
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
}
