using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField]
    int damage;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
            return;
        }

        if (!collision.CompareTag("Player"))
        {
            return;
        }


        collision.GetComponent<Player>().TakeDamage(this.damage);
        Destroy(gameObject);
    }
}
