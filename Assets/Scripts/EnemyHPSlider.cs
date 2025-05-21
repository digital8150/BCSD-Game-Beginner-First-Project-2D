using UnityEngine;
using UnityEngine.UI;

public class EnemyHPSlider : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private GameObject target;
    private bool isSetup = false;
    private bool isBoss = false;

    public void Setup(GameObject target, bool isBoss = false)
    {
        this.target = target;
        isSetup = true;
        this.isBoss = isBoss;
    }


    // Update is called once per frame
    void Update()
    {
        if(!isSetup)
            return;

        if (target == null)
        {
            if (isBoss)
            {
                gameObject.SetActive(false);
                
            }
            else
            {
                Destroy(gameObject);
                
            }
            return;

        }

        GetComponent<Slider>().value = (float)target.GetComponent<Enemy>().CurrentHealth / target.GetComponent<Enemy>().MaxHealth;

    }
}
