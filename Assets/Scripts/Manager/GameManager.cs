using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GameManager : SingleTone<GameManager>
{
    [SerializeField]
    GameObject staminaSlider;
    [SerializeField]
    Player player;
    [SerializeField]
    int maxEnemyCount = 20;
    public int CurrentEnemyCount { get; set; } = 0;
    [SerializeField]
    GameObject enemyPrefab;
    [SerializeField]
    GameObject mainCamera;



    public override void Awake()
    {
        staminaSlider.GetComponent<PositionAutoSetter>().Setup(player.transform);
        RemoveDuplicates();
    }


    private float enemySpawnTimer = 0f;
    private float enemySpawnInterval = 5f;

    private void FixedUpdate()
    {
        // 5초마다, 카메라 위에서, 카메라 가로 범위 내에서 적 생성
        enemySpawnTimer += Time.fixedDeltaTime;
        if (enemyPrefab != null && CurrentEnemyCount < maxEnemyCount && enemySpawnTimer >= enemySpawnInterval)
        {
            Camera cam = mainCamera.GetComponent<Camera>();
            float camHeight = 2f * cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            Vector3 camPos = cam.transform.position;
            float spawnX = Random.Range(camPos.x - camWidth / 2f, camPos.x + camWidth / 2f);
            float spawnY = camPos.y + camHeight / 2f + 2f; // 카메라 위 2유닛

            var clone = Instantiate(enemyPrefab, new Vector3(spawnX, spawnY, 0), Quaternion.identity);
            clone.GetComponent<Enemy>().Setup(player.gameObject);
            CurrentEnemyCount++;
            enemySpawnTimer = 0f;
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
        
    }
}
