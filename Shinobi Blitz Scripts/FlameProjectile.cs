using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameProjectile : MonoBehaviour {

    private Transform target;
    public int damage = 12;
    public int damage_bonus = 8;
    public GameObject damage_effect;
    [HideInInspector]
    public enemy enemy_script;
    [HideInInspector]
    public enemy2 enemy_script2;
    public GameObject flame_particle;
    public float duration = 0.75f;


    // Use this for initialization
    void Start()
    {
        Destroy(gameObject, duration);
        GameObject flame_effect = Instantiate(flame_particle, transform.position, transform.rotation);
        Destroy(flame_effect, duration);
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Enemy")
        {
            GameObject effect = (GameObject)Instantiate(damage_effect, transform.position, transform.rotation);
            Destroy(effect, 1f);
            enemy_script = col.GetComponent<enemy>();
            if(enemy_script.slowed)
            {
                damage += damage_bonus;
                enemy_script.ReceiveDamage(damage);
                damage -= damage_bonus;
            }
            else
            {
                enemy_script.ReceiveDamage(damage);
            }
            enemy_script.GetBurned();
        }

        if (col.gameObject.tag == "Enemy2")
        {
            GameObject effect = (GameObject)Instantiate(damage_effect, transform.position, transform.rotation);
            Destroy(effect, 1f);
            enemy_script2 = col.GetComponent<enemy2>();
            if (enemy_script2.slowed)
            {
                damage += damage_bonus;
                enemy_script2.ReceiveDamage(damage);
                damage -= damage_bonus;
            }
            else
            {
                enemy_script2.ReceiveDamage(damage);
            }
            enemy_script2.GetBurned();
        }
    }
}
