using UnityEngine;

public class Boss : MonoBehaviour
{
    [SerializeField]
    float fireInterval = 2.0f;
    [SerializeField]
    GameObject projectilePrepab;
    float lastFireTime = 0;

    private void Update()
    {
        if(Time.time - lastFireTime > fireInterval)
        {
            lastFireTime = Time.time;
            var clone = Instantiate(projectilePrepab, this.transform);
            Vector3 dir = GameManager.Instance.player.transform.position - clone.transform.position;
            dir.Normalize();
            clone.GetComponent<Movement2D>().MoveTo(dir);
            //각도 계산
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            //Z축 기준 회전
            clone.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

}
