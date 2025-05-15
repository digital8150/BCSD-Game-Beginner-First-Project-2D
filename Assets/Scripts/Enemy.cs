using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField]
    private int maxHealth = 20;
    public int CurrentHealth { get; private set; }

    [SerializeField]
    private int damage = 5;

    [SerializeField]
    private float moveSpeed = 2.0f;

    [SerializeField]
    private GameObject target;

    [SerializeField]
    private GameObject deathParticlesPrefab;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float diePositiionY = -7f;
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
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
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
            OnDie();
        }
    }

    public void TakeDamage(int damage)
    {
        this.CurrentHealth -= damage;
        if (this.CurrentHealth <= 0)
        {
            OnDie();
        }
    }

    private void OnDie()
    {
        // Play death animation or sound here
        Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
        GameManager.Instance.CurrentEnemyCount--;
    }
}
