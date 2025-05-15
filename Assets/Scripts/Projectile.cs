using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private int damage;

    // Update is called once per frame
    void FixedUpdate()
    {

        Camera cam = CameraEffectManager.Instance.mainCamera.GetComponent<Camera>();
        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        Vector3 camPos = cam.transform.position;
        //Destroy the projectile if it goes out of bounds
        if (transform.position.x < (camPos.x - camWidth / 2) ||
            transform.position.x > (camPos.x + camWidth / 2))
        {
            Destroy(gameObject);
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;

        collision.GetComponent<Enemy>().TakeDamage(this.damage);
        Destroy(gameObject);
    }
}
