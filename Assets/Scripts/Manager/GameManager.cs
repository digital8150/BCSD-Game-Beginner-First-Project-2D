using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GameManager : SingleTone<GameManager>
{
    [Header("UI 설정")]
    [SerializeField]
    GameObject staminaSlider;
    [SerializeField]
    GameObject healthSlider;
    [SerializeField]
    TextMeshProUGUI scoreText;
    [Header("오브젝트 연결")]
    [SerializeField]
    GameObject mainCamera;
    [SerializeField]
    Player player;
    [SerializeField]
    GameObject enemyPrefab;
    [SerializeField]
    int maxEnemyCount = 20;
    public int CurrentEnemyCount { get; set; } = 0;

    public int score = 0;



    public override void Awake()
    {
        staminaSlider.GetComponent<PositionAutoSetter>().Setup(player.transform);
        healthSlider.GetComponent<PositionAutoSetter>().Setup(player.transform);
        base.Awake();
    }


    private float enemySpawnTimer = 0f;
    private float enemySpawnInterval = 5f;

    private void FixedUpdate()
    {
        // n초마다, 적 생성
        enemySpawnTimer += Time.fixedDeltaTime;
        if (enemyPrefab != null && CurrentEnemyCount < maxEnemyCount && enemySpawnTimer >= enemySpawnInterval)
        {
            Camera cam = mainCamera.GetComponent<Camera>();
            float camHeight = 2f * cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            Vector3 camPos = cam.transform.position;
            
            float spawnX = Random.Range(-1,1) < 0 ?  camPos.x - camWidth / 2f - 2 : camPos.x + camWidth / 2f + 2;
            float spawnY = camPos.y;

            var clone = Instantiate(enemyPrefab, new Vector3(spawnX, spawnY, 0), Quaternion.identity);
            clone.GetComponent<Enemy>().Setup(player.gameObject);
            CurrentEnemyCount++;
            enemySpawnTimer = 0f;
            enemySpawnInterval = Random.Range(1f, 5f);
        }
    }

    private void LateUpdate()
    {
        if(player.CurrentStamina < player.MaxStamina)
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
}
