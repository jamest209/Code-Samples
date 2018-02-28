//This script was written by James | Last edited by James | Modified on September 7, 2017
//The purpose of this script is to manage the player's health and stagger whenever they are hit by an enemy.


using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerHealth : MonoBehaviour
{
    public int health = 100; //the health of the player.
    public float stagger_duration = 0.5f; //the duration of the stagger when the player is hit by an attack.
    //[HideInInspector]
    public PlayerGamepad player_pad; //needed to access the player's movement script.

    public GameObject damaged_effect;
    public static bool Dead;
	[SerializeField]
	private HealthStat Health;

	private Combat my_combat;
    public static PlayerHealth PH; //acid
    public Vector3 respawn_location;
    public GameObject playerGO;
    public GameObject current_spawn_point;
    // ---------------
    //James Added Here
    // ---------------

    //DEATH EVENT
    public NewDynamicCameraBehavior cam_script;
    public float respawn_timer = 2f;
    public bool check_to_die;

    public IEnumerator StaggerPlayer()
    {
        //turn off the player movement to simulate a stun.
        player_pad.SetPlayerMovement(false);
        //create damaged effect
		if (!my_combat.is_countering) {
			GameObject effect = Instantiate (damaged_effect, transform.position, transform.rotation);
			Destroy (effect, 1f);
		}
        yield return new WaitForSeconds(stagger_duration);
        //turn on the player movement to end the stun.
        player_pad.SetPlayerMovement(true);
    }
    
    public void DamageReceived(int damage) //function to apply the damage to the player's health.
    {
        health -= damage;
		Health.CurrentVal -= damage;
        if(health <= 0)
        {
            check_to_die = true;
            Dead = true;
        }
        else
        {
            StartCoroutine("StaggerPlayer");
        }
        
    }

    public void SaveRespawnLoc(GameObject spawn_point)
    {
        current_spawn_point = spawn_point;
        respawn_location = current_spawn_point.transform.position;
    }

    // ---------------
    // James Added Here
    // ---------------
    private void Respawn()
    {
        Debug.Log("moving to... " + respawn_location);
        ScoreSystem.Singleton_ScoreSystem.score_playerDeath();
        playerGO.transform.position = respawn_location;
        cam_script.death_event_mode = false;
        cam_script.ResetAngle();
        player_pad.SetPlayerMovement(true);
        player_pad.use_camera_type_1 = true;
        player_pad.current_speed = 0;
        Dead = false;
        Heal();
        current_spawn_point.GetComponent<Checkpoint_Script>().begin_reset = true;
    }

    // ---------------
    //James Added Here
    // ---------------
    public void FallDeath()
    {
        Dead = true;
        StartCoroutine(ChangeDeathState());
        cam_script.death_event_mode = true;
        player_pad.SetPlayerMovement(false);
        player_pad.use_camera_type_1 = false;
        cam_script.StoreAngle();
        health = 1;
        Health.CurrentVal = 1;
    }

    public IEnumerator ChangeDeathState()
    {
        print("changing to death state.");
        yield return new WaitForSeconds(respawn_timer);
        health = 0;
        check_to_die = true;
    }


    private void Awake()
    {
		my_combat = this.GetComponentInParent<Combat> ();
        playerGO = GameObject.Find("Player");
        player_pad = playerGO.GetComponent<PlayerGamepad>();
        

        if(PH == null)
        {
            PH = this;
        }

        
        // ---------------
        //James Added Here
        // ---------------
        cam_script = GameObject.Find("Camera Anchor").GetComponent<NewDynamicCameraBehavior>();

    }

	void Start(){
		//HealthStat.s = GameObject.FindGameObjectWithTag ("healthBar").GetComponent<HealthStat> ();
		Health.Initialize();
	}

	public void Heal(){
		health = 100;
		Health.CurrentVal = 100;
	}

    private void Update()
    {
        if (check_to_die)
        {
            Respawn();
            check_to_die = false;
        }
    }
}
