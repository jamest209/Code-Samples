using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameTower : MonoBehaviour {

    public Transform target;
    public float range = 2f;

    public float fire_rate = 2f;
    private float fire_countdown = 0f;

    public GameObject flame_prefab;
    public Transform spawn_point;


    void Shoot()
    {
        Instantiate(flame_prefab, spawn_point.position, spawn_point.rotation);
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
        InvokeRepeating("UpdateTarget", 0f, 0.05f);
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
    }
}
