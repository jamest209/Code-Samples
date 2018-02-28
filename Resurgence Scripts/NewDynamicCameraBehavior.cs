// Original Author: Steven Trujillo || Last Edited: James Tran | Modified on November 24, 2017
// NewCameraBehavior: Sets camera anchor to player's position, clamps camera's x_axis and handles targetting enemies.
// 
//
//
// 
//
//
//
//WARNING: Collider's must be tagged!!

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewDynamicCameraBehavior : MonoBehaviour {
	public GameObject player;
    
	private GameObject targeted_enemy;
	private float temp_axis;

	private bool target_locked = false;
	private GameObject target = null;

	public float maxFindTargetDistance = 25f;
	public float maxTargetDistance = 25f;

	public float defaultFOV = 70;
	public float focusedFOV = 60;
    public bool checkRet = false;
	public GameObject retPrefab;
	GameObject ret;
	public GameObject retTarget;
	public Vector3 retPos;


    // ---------------
    //James Added Here
    // ---------------
    public bool death_event_mode = false;
    private float rotation_x, rotation_y, rotation_z;


	// Whether the joystick has been recentered.
	private bool next_target_joystick_x_centered = false;


	//TJ add on
	private Combat player_combat;

	void Awake () {
		Camera.main.fieldOfView = defaultFOV;
		player = GameObject.Find ("Player");
		ret = GameObject.FindGameObjectWithTag("Reticle");

		player_combat = player.GetComponent<Combat> ();
    }

    private void Update()
    {
        // ---------------
        //James Added Here
        // ---------------
        // Sets camera anchor to player's position.
        if (!PlayerHealth.Dead)
        {
            transform.position = player.transform.position;
        }
        else
        {
            transform.LookAt(player.transform);
        }
    }


    void FixedUpdate () {
		retTarget = target;

		if (target_locked) {
			if (!target.activeInHierarchy) {
				Debug.Log ("hes dead bitch");
				target_locked = false;
				//added by TJ
				player_combat.locked_on = false;
				checkRet = false;
				//ret.SetActive (false);
			}
		}

		// Note whether the joystick has been recentered since last time searching for 'NEXT' target.
		if (next_target_joystick_x_centered == false) {
			if (Mathf.Abs(Input.GetAxisRaw ("RightJoystickX")) < 0.1f) {
				next_target_joystick_x_centered = true;
			}
		}

		
		

		// Clamp the camera's x-axis.
		if (transform.rotation.eulerAngles.x < 60)
		{
			temp_axis = transform.rotation.eulerAngles.x;
		}
		if (transform.rotation.eulerAngles.x > 330)
		{
			temp_axis = transform.rotation.eulerAngles.x;
		}
		transform.rotation = Quaternion.Euler(temp_axis, transform.rotation.eulerAngles.y, 0);

		// On right bumper pressed, find a target.
		if (Input.GetButtonDown ("Controller_LB")) {
			// If targetting already active, end it.
			if (target_locked == true) {
				target_locked = false;
                checkRet = false;

				//added by TJ
				player_combat.locked_on = false;
            } 

			// Else, check for target.
			else {
				// Receives GameObject or null.
				target = FindTarget();

				if (target != null) {
					// TARGET FOUND.
					//Debug.Log ("Target set.");
					target_locked = true;
                    checkRet = true;

					//added by TJ
					player_combat.locked_on = true;
                } 
				else {
					//Debug.Log("No targets found.");
				}
			}
		}

		// Target switching.
		if (target != null) {
			if (target_locked == true) {
				if (Mathf.Abs (Input.GetAxisRaw ("RightJoystickX")) > 0.75f && next_target_joystick_x_centered == true) {
					next_target_joystick_x_centered = false;
					GameObject next_target = NextTarget (Input.GetAxisRaw ("RightJoystickX"));

					if (next_target != null) {
						target = next_target;
					}
				}
			}
		} 
		else {
			if (target_locked == true) {
				target_locked = false;
                checkRet = false;
            }
		}

		// Smoothly looks at target.
		if (target_locked == true) {
			// Set focused field of view.
			Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView,focusedFOV,0.35f);

			if (target != null) {
				// Check if target is too far from the player.
				if (Vector3.Distance (player.transform.position, target.transform.position) < maxTargetDistance) {
					// Look at target.
					Quaternion look_pos = Quaternion.LookRotation (target.transform.position - transform.position);
					float look_speed = 0.15f;
					transform.rotation = Quaternion.Lerp (transform.rotation, look_pos, look_speed);
				} else {
					// Else, remove target.
					target = null;
					target_locked = false;
                    checkRet = false;
				}
			} else {
				target_locked = false;
			}
		} 
		else {
			// Set default field of view.
			Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView,defaultFOV,0.35f);
		}
        if (checkRet == true){
            ret.SetActive(true);
			ret.transform.position = Camera.main.WorldToScreenPoint(retTarget.transform.position);
            //GameObject.FindGameObjectWithTag("Reticle").SetActive(true);
        }
        if (checkRet == false)
        {
            ret.SetActive(false);
            //GameObject.FindGameObjectWithTag("Reticle").SetActive(false);
        }
    }

	// While targetting, be able to move to the next target in the direction of the joystick according to screen space.
	GameObject NextTarget (float r_joy_h_axis){
		//Debug.Log ("Searching for new target..");
		Vector3 center = player.transform.position;
		float radius = maxTargetDistance;

		// Get array of nearby object colliders.
		Collider[] hitColliders = Physics.OverlapSphere (center, radius);

		// Create list to hold GameObjects.
		List<GameObject> hitCollidersList = new List<GameObject>(); //List<Collider> hitCollidersList = new List<Collider>(hitColliders);
		int i = 0;

		// Put collider's gameObjects into list if it is tagged "Enemy". (For distance sorting)
		while (i < hitColliders.Length) {
			if (hitColliders [i].gameObject.tag == "Enemy") {
				hitCollidersList.Add (hitColliders [i].gameObject);
			}
			i++;
		}

		GameObject return_target = null;

		// Iterare through objects, and return the best choice.
		int ii = 0;
		while (ii < hitCollidersList.Count) {
			if (hitCollidersList [ii].gameObject != target) {
				// If object is tagged as an enemy.
				if (hitCollidersList [ii].tag == "Enemy") {
					// If object is visible on the screen.
					if (hitCollidersList [ii].gameObject.GetComponent<Renderer> ().isVisible == true) {
						GameObject temporary_target = hitCollidersList [ii].gameObject;
						// If there is currently a return_target..
						if (return_target != null) {
							// Check if temporary_target is closer to screen center x than the current return_target.
							if (Mathf.Abs(Camera.main.WorldToScreenPoint (temporary_target.transform.position).x - Screen.width/2) < Mathf.Abs(Camera.main.WorldToScreenPoint (return_target.transform.position).x - Screen.width/2)){

								// Check if temp_target.x is in the correct direction according to input direction.
								// RIGHT.
								if (r_joy_h_axis > 0) {
									if ((Camera.main.WorldToScreenPoint (temporary_target.transform.position).x - Screen.width / 2) > 0) {
										return_target = temporary_target;
									}
								}
								// LEFT.
								else if (r_joy_h_axis < 0) {
									if ((Camera.main.WorldToScreenPoint (temporary_target.transform.position).x - Screen.width / 2) < 0) {
										return_target = temporary_target;
									}
								}
							}
						} 
						// Else, by default set temporary_target as the new return_target.
						else {
							// Check if temp_target.x is in the correct direction according to input direction.
							// RIGHT.
							if (r_joy_h_axis > 0) {
								if ((Camera.main.WorldToScreenPoint (temporary_target.transform.position).x - Screen.width / 2) > 0) {
									return_target = temporary_target;
								}
							}
							// LEFT.
							else if (r_joy_h_axis < 0) {
								if ((Camera.main.WorldToScreenPoint (temporary_target.transform.position).x - Screen.width / 2) < 0) {
									return_target = temporary_target;
								}
							}
						}
					}
				}
			}
			ii++;
		}

		return return_target;
	}

	// Casts overlap sphere for list of nearby colliders, orders list of objects by distance. Starting from closest, iterate through list until object tagged "enemy" & isVisible found. Return target.
	GameObject FindTarget (){
		Vector3 center = player.transform.position;
		float radius = maxTargetDistance;

		// Get array of nearby object colliders.
		Collider[] hitColliders = Physics.OverlapSphere (center, radius);

		// Create list to hold GameObjects.
		List<GameObject> hitCollidersList = new List<GameObject>(); //List<Collider> hitCollidersList = new List<Collider>(hitColliders);
		int i = 0;

		// Put collider's gameObjects into list if it is tagged "Enemy". (For distance sorting)
		while (i < hitColliders.Length) {
			if (hitColliders [i].gameObject.tag == "Enemy") {
				hitCollidersList.Add (hitColliders [i].gameObject);
			}
			i++;
		}

		GameObject return_target = null;

		// Iterare through objects, and return the best choice.
		int ii = 0;
		while (ii < hitCollidersList.Count) {
			// If object is tagged as an enemy.
			if (hitCollidersList[ii].tag == "Enemy") {
				// If object is visible on the screen.
				if (hitCollidersList [ii].gameObject.activeInHierarchy) {
					GameObject temporary_target = hitCollidersList [ii].gameObject;

					// If temporary_target is within the player's view.
					float player_view_angle = 70f;
					if (Vector3.Angle ((temporary_target.transform.position - player.transform.position), player.transform.forward) < player_view_angle) {
						// If there is currently a return_target..
						if (return_target != null) {
							// Check if temporary_target is closer than the current return_target.
							if (Vector3.Distance (player.transform.position, temporary_target.transform.position) < Vector3.Distance (player.transform.position, return_target.transform.position)) {
								// If so, set temporary_target as the new return_target.
								return_target = temporary_target;
							}
						} 
						// Else, by default set temporary_target as the new return_target.
						else {
							return_target = temporary_target;
						}
					}
				}
			}
			ii++;
		}

		// If no target found, attempt to find target from closest to center X-axis on screen.
		if (return_target == null) {
			// Iterare through objects, and return the best choice.
			int iii = 0;
			while (iii < hitCollidersList.Count) {
				// If object is tagged as an enemy.
				if (hitCollidersList[iii].tag == "Enemy") {
					// If object is visible on the screen.
					if (hitCollidersList [iii].gameObject.GetComponent<Renderer> ().isVisible == true) {
						GameObject temporary_target = hitCollidersList [iii].gameObject;

						// If there is currently a return_target..
						if (return_target != null) {
							// Check if temporary_target is closer than the current return_target to the screen's center on the X axis.
							if (Mathf.Abs(Camera.main.WorldToScreenPoint (temporary_target.transform.position).x - Screen.width/2) < Mathf.Abs(Camera.main.WorldToScreenPoint (return_target.transform.position).x - Screen.width/2)) {
								// If so, set temporary_target as the new return_target.
								return_target = temporary_target;
							}
						} 
						// Else, by default set temporary_target as the new return_target.
						else {
							return_target = temporary_target;
						}
					}
				}
				iii++;
			}
		}

		return return_target;
	}

	//ADDED BY TJ
	public bool GetLockOn(){
		return target_locked;
	}

	public GameObject GetTargetedEnemy(){
		return target;
	}



    // ---------------
    //James Added Here
    // ---------------
    public void StoreAngle()
    {
        rotation_x = transform.rotation.x;
        rotation_y = transform.rotation.y;
        rotation_z = transform.rotation.z;
    }

    public void ResetAngle()
    {
        transform.rotation = Quaternion.Euler(rotation_x, rotation_y, rotation_z);
    }
}