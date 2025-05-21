using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("적 스텟 설정")]
    [SerializeField]
    private int maxHealth = 20;
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get { return maxHealth; } }
    [SerializeField]
    private int score = 500;

    [SerializeField]
    private int damage = 5;

    [SerializeField]
    private float moveSpeed = 2.0f;

    [SerializeField]
    private bool isFloating = false;


    [Header("오브젝트 연결")]
    [SerializeField]
    private GameObject target;

    [SerializeField]
    private GameObject deathParticlesPrefab;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private float diePositiionY = -6f;
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public void Setup(GameObject player)
    {
        this.target = player;
    }
    
    void Awake()
    {
        this.CurrentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //move to player(target) (physics method, only applie to x axis)
        if (target != null)
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, isFloating ? direction.y * moveSpeed * 0.75f : rb.linearVelocityY);
        }

        if(rb.linearVelocityX > 0)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }

    private void Update()
    {
        if(transform.position.y < diePositiionY)
        {
            OnDie(false);
        }

        //attack player if collided
        if (target != null && Vector3.Distance(transform.position, target.transform.position) < 1.5f)
        {
            target.GetComponent<Player>().TakeDamage(damage);
        }
    }

    public void TakeDamage(int damage)
    {
        this.CurrentHealth -= damage;
        if (this.CurrentHealth <= 0)
        {
            OnDie(true);
        }
    }

    public void OnDie(bool killedbyplayer)
    {
        // Play death animation or sound here
        Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);
        GameManager.Instance.enemies.Remove(this);
        Destroy(gameObject);
        GameManager.Instance.CurrentEnemyCount--;
        if(killedbyplayer)
        {
            CameraEffectManager.Instance.ApplySaturationBoost();
            CameraEffectManager.Instance.ApplyPostExposureBoost();
            CameraEffectManager.Instance.ApplyCromaticAbb();
            CameraEffectManager.Instance.ApplySlowMotion();
            GameManager.Instance.score += this.score;
        }
    }

    public void kill()
    {
        // Play death animation or sound here
        Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
        GameManager.Instance.CurrentEnemyCount--;
    }
}
