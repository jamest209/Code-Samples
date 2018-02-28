using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MortarProjectile : MonoBehaviour {
    [HideInInspector]
    public float current_timer = 0f;
    public Color explosion_color;
    public int damage = 15;
    [HideInInspector]
    public MortarTower mortar_scr;
    [HideInInspector]
    public Vector3 destination;
    private float duration = 0.8f;
    private Vector3 start_pos;
    private Transform end_pos;
    private float start_time;
    private float length;
    private float distance;
    private bool stop_chasing = false;
    public GameObject mortarshell_model;
    private MeshRenderer m_render;

    private Renderer rendr; //the renderer to change the color of the cannonball on explosion.
    [HideInInspector]
    public enemy enemy_script;
    [HideInInspector]
    public enemy2 enemy_script2;
    [HideInInspector]
    public bool exploding = false;
    [HideInInspector]
    public Rigidbody rb;

    public void SetDestination(Transform target)
    {
        end_pos = target;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Enemy" || col.gameObject.tag == "Enemy2" || col.gameObject.tag == "Pathway")
        {
            mortarshell_model.SetActive(false);
            m_render.enabled = true;
            exploding = true;
            rb.isKinematic = true;
            rendr.material.color = explosion_color;

            if (col.gameObject.tag == "Enemy")
            {
                enemy_script = col.GetComponent<enemy>();
                enemy_script.ReceiveDamage(damage);
            }

            else if (col.gameObject.tag == "Enemy2")
            {
                enemy_script2 = col.GetComponent<enemy2>();
                enemy_script2.ReceiveDamage(damage);
            }

            else
            {
                //add fix here if necessary
            }
        }
    }

        

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rendr = GetComponent<Renderer>();
        m_render = GetComponent<MeshRenderer>();
        start_pos = transform.position;
        start_time = Time.time;
        destination = end_pos.position;
        Destroy(gameObject, 5f);
        transform.parent = GameObject.Find("BuildList").transform;
    }

    void Update()
    {

        distance = Vector3.Distance(transform.position, destination);
        if (distance < 0.2f)
        {
            stop_chasing = true;
        }

        




        if (!stop_chasing)
        {
            Vector3 center = (start_pos + destination) * 0.5F;
            center -= new Vector3(0, 1, 0);
            Vector3 riseRelCenter = start_pos - center;
            Vector3 setRelCenter = destination - center;
            float fracComplete = (Time.time - start_time) / duration;
            transform.position = Vector3.Slerp(riseRelCenter, setRelCenter, fracComplete);
            transform.position += center;
        }
        else
        {
            //add fix here if neccessary
        }
        
        
        if (exploding)
        {
            current_timer += Time.deltaTime;
            transform.localScale += new Vector3(5.5f, 5.5f, 5.5f) * Time.deltaTime;
            if (current_timer > .4f)
            {
                Destroy(gameObject);
            }
        }
    }
}
