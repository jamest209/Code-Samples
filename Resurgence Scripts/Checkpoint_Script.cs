using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint_Script : MonoBehaviour
{
    private GameObject[] CheckpointPrefab;
    public Vector3 SaveLoc;
    public bool ActiveCheckPoint;
    public bool PlayerCheck = false;
    public float pos1;
    public float pos2;
	private Vector3[] EnemySpawnLoc;
    public GameObject[] allEnemies;
	public GameObject[] battleAreas;
    public GameObject ThePlayer;
    public bool begin_reset;
    private PlayerHealth health_scr;

    // ---------------
    //James Added Here
    // ---------------
    private PlayerGamepad player_gamepad;

    private void Awake()
    {
		battleAreas = GameObject.FindGameObjectsWithTag ("battleArea");
        allEnemies = GameObject.FindGameObjectsWithTag("Enemy"); // Finds all objects in the scene with the tag "enemy"
		EnemySpawnLoc = new Vector3 [allEnemies.Length];

		for (int i = 0; i < allEnemies.Length; i++) //go through all enemies currently on the map at the start of the game
        {
            EnemySpawnLoc[i] = allEnemies[i].transform.position; //Set a list of Vectors "EnemySpawnLoc" to a list of the enemy's positions at the start of the map so it can be saved for respawning the enemy
            //print(EnemySpawnLoc[i]);
            //print(allEnemies[i]);
        }
        ThePlayer = GameObject.FindGameObjectWithTag("Player"); //Set ThePlayer to the Player asset within the scene



        player_gamepad = ThePlayer.GetComponent<PlayerGamepad>(); //Get the reference to the player's gamepad.
    }
		
    void Update()
    {    
		if (ActiveCheckPoint) {
			pos1 = ThePlayer.transform.position.y; //gets the current y position of the player

			if (begin_reset) {
                begin_reset = false;

                    for (int i = 0; i < allEnemies.Length; i++)
                    {
                        allEnemies[i].transform.position = EnemySpawnLoc[i]; //sets all enemies positions back to their originol positions for when the player is killed
                        allEnemies[i].SetActive(true);
                        allEnemies[i].GetComponent<BasicAI>().StartCoroutine("reset");
                        print("Enemies Back home.");
                        print(allEnemies[i]);
                    }
                    for (int i = 0; i < battleAreas.Length; i++)
                    {
                        Debug.Log("Check point calling reset walls!");
                        battleAreas[i].GetComponent<BattleArea_End>().ResetWalls();
                    }

                    ThePlayer.GetComponent<PlayerHealth> ().Heal (); // Resets player health for when they die
					ThePlayer.GetComponentInParent<PlayerGamepad> ().setPlayerDeath (false);//Sets PlayerDied to false when the player has respawned
					
			}
		}
    }

	bool discoverd = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player") //Checks to see if the player has collided with the checkpoint
        {
            //once the player runs into the checkpoint, the checkpoint will pass this current gameobject to it so that it can respawn back at this specific checkpoint.
            health_scr = GameObject.Find("Player_Capsule").GetComponent<PlayerHealth>();
            health_scr.SaveRespawnLoc(this.gameObject);
            CheckpointPrefab = GameObject.FindGameObjectsWithTag("Checkpoint"); // Finds all checkpoints within the map
            Debug.Log("player hit the checkpoint.");
            for (int i = 0; i < CheckpointPrefab.Length; i++)
            {
				if (!discoverd) {
					ThePlayer.GetComponent<PlayerHealth> ().Heal ();
					discoverd = true;
				}
                CheckpointPrefab[i].GetComponent<Checkpoint_Script>().ActiveCheckPoint = false; // Sets any other active checkpoint to inactive
                CheckpointPrefab[i].GetComponent<Checkpoint_Script>().SaveLoc = gameObject.transform.position; //Sets the respawn location(SaveLoc) to whichever checkpoint the player collided with last
               
                pos2 = ThePlayer.transform.position.y; //gets the y position for when they enter the checkpoint
                ActiveCheckPoint = true; //Sets the checkpoint that the player just collided with to the Active checkpoint and the platyer will only respawn there
                print("Checkpoint");
            }
        }
    }
}