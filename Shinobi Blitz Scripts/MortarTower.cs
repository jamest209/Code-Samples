using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MortarTower : MonoBehaviour {

    public Transform target;
    public float range = 2f;

    public float fire_rate = 2f;
    private float fire_countdown = 0f;

    public GameObject projectile_prefab;
    public Transform spawn_point;


    public GameObject barrel;
    [HideInInspector]
    public Vector3 barrel_original_pos;
    public bool firing = false;
    public float barrel_move_timer = 0.1f;
    public float current_move_timer = 0f;


    void Shoot()
    {
        GameObject projectiletGO = (GameObject)Instantiate(projectile_prefab, spawn_point.position, spawn_point.rotation);
        MortarProjectile bullet_script = projectiletGO.GetComponent<MortarProjectile>();

        if (bullet_script != null)
        {
            bullet_script.SetDestination(target);
            firing = true;
            current_move_timer = barrel_move_timer;
        }
    }

    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy"); //array of enemies to compare each other.
        GameObject[] enemies2 = GameObject.FindGameObjectsWithTag("Enemy2"); //array of enemies to compare each other.
        float closest_distance = Mathf.Infinity; //this is to check the distance of the closest enemy.
        GameObject closest_enemy = null; //this is the closest enemy. set to null for set up

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position); //get the distance in units.

            if (distanceToEnemy < closest_distance) //compare each of the enemies in terms of distance
            {
                closest_distance = distanceToEnemy; //if it finds one that is closer, the closest distance is stored.
                closest_enemy = enemy; //also store the closest enemy.
            }
        }

        foreach (GameObject enemy in enemies2)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position); //get the distance in units.

            if (distanceToEnemy < closest_distance) //compare each of the enemies in terms of distance
            {
                closest_distance = distanceToEnemy; //if it finds one that is closer, the closest distance is stored.
                closest_enemy = enemy; //also store the closest enemy.
            }
        }

        if (closest_enemy != null && closest_distance <= range)
        {
            target = closest_enemy.transform;
        }
        else if (closest_enemy != null && closest_distance >= range)
        {
            target = null;
        }
    }

    private void Start()
    {
        InvokeRepeating("UpdateTarget", 0f, 0.15f);
        barrel_original_pos = barrel.transform.position;
    }

    void Update()
    {
        if (target == null)
            return;

        if (fire_countdown <= 0f)
        {
            Shoot();
            fire_countdown = 1f / fire_rate;
        }

        fire_countdown -= Time.deltaTime;

        if(firing)
        {
            Vector3 temp = barrel.transform.position;
            temp.y -= 0.01f;
            barrel.transform.position = temp;
            current_move_timer -= Time.deltaTime;
            if(current_move_timer <= 0f)
            {
                firing = false;
            }
        }
        else
        {
            barrel.transform.position = barrel_original_pos;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
