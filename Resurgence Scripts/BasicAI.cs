/*This script was written by James | Last edited by James | Modified on December 6, 2017
 *The purpose of this script is to have an enemy chase the player. This is implemented by using a Nav Mesh Agent.
 *It will then decide what to do with its list of actions.
 *

 * The way this script works is that it requires a nav mesh agent to follow the player. The ai will use the nav mesh when it is "alerted" 
 * so if you don't want the ai to follow the player anymore set "alerted" to false. Once the AI is within a certain distance
 * to the player, it will randomly decide whether to attack or dodge. If it attacks, it will access the BasicAI_Attack script.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ai_state //state number starts at 0. So for example ai_state[2] is dodge
{
    walking,
    idle,
    dodging,
    moveback,
    attack_1,
    attack_2,
    attack_3,
    staggered,
    dying
} // states of the AI: idle is when the ai will not be moving, walking is when it is on patrol, dodging is when the ai moves to the side
  // attacks 1, 2, and 3 are different ways of the ai's slash attacks. staggered is when the ai is hit in the middle of his attacks
  // dying is when the ai is about to die after reaching 0 health.

public enum dodge_direction //these are the states to determine whether the ai dodges to the left or right.
{
    left,
    right,
    not_dodging
}

public class BasicAI : MonoBehaviour 
{
    [Header("Timers for actions")]
    [Tooltip("This is how long the action takes to perform.")]
    public float performing_time = 0.5f;
    [Tooltip("This is the amount of time the AI dodges for.")]
    public float dodge_time = 0.5f;
    [Tooltip("how long the AI will take before it performs another action.")]
    public float cooldown_action = 1f;
    [Tooltip("The maximum amount of time it will take to perform another action.")]
    public float cooldown_action_max = 3f;
    [Tooltip("The minimum amount of time it will take to perform another action.")]
    public float cooldown_action_min = 1f;
    [Tooltip("The duration of the AI getting knocked back.")]
    public float knockback_duration = 0.2f;
    [Tooltip("The duration of staggering once the knockback is done.")]
    public float stagger_after_knockback_dur = 0.3f;
    [Tooltip("The duration of the AI when it chooses to move backwards.")]
    public float moveback_duration = 0.3f;
    [Tooltip("The amount of time after the ai reaches 0 hp to be destroyed.")]
    public float death_duration = 0.001f;
    private float current_stagger_dur = 0f; //the current time of being staggered.
    public bool getting_knockback; //used to stop the knockback once knockback_duration is done.

    // Neil: Putting this so that BasicAI can also play animations.
    public Animator animator;


    public float distance_to_player; //used to keep track between this AI and the player. useful for 
                                      //seeing how far the AI is from the player before deciding
                                      //to chase the player or when to attack/dodge.

    [Header("Attributes")]

    [Tooltip("The maximum speed of the enemy's movement.")]
    public float movement_speed = 15;
    [Tooltip("The rate of acceleration of the enemy. Higher number is faster time to max movement speed.")]
    public float acceleration = 50;
    [Tooltip("This is the speed in which the AI moves when deciding to step backwards.")]
    public float moveback_speed = 5f;
    [Tooltip("This is the speed in which the AI moves when dodging.")]
    public float dodge_speed = 1f;
    [Tooltip("This is how fast the AI can rotate to look at the player.")]
    public float turn_speed = 1f;
    [Tooltip("The health of the AI. It will die when it is 0.")]
    public int enemy_health = 100; //the health of the enemy.
    [Tooltip("The start health of the AI.")]
    public int enemy_start_health = 100;
    //[Tooltip("If this unit is advanced or not.")]
    //public bool advanced = false;
    
	[Tooltip("The force that pushes back the AI when it is hit.")]
	public float base_knockback_force = 18f;
	[HideInInspector]
	public float knockback_force;

	[Tooltip("This is used for testing. Check this to damage the enemy.")]
    public bool check_to_damage = false;
    [Tooltip("This is the percentage chance to dodge instead of attack. The lower the number, the more often it will dodge.")]
    public int chance_to_dodge = 35;


    [Header("Don't touch!")]
    //used to move to the game object when left or right dodging.
    public GameObject left_dodge;
    public GameObject right_dodge;
    public GameObject my_weapon;
    [Tooltip("Used to determine what action the AI will take. 35 and below will make the AI dodge, anything else will make it attack.")]
    public int action_selection = 0;

    private BattleArea_End end; /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public bool berserk_mode = false;
    public GameObject berserk_flame1, damaged_effect;
    [HideInInspector]
    public bool first_alert = false; //used to keep track if the AI has been alerted the first time.
    [HideInInspector]
    public bool alerted = false; //once the AI has been alerted, it will start chasing the player.
    [HideInInspector]
    public bool first_berserk = false; //used to keep track if the AI has been berserked for the first time.
    protected bool performing_action = false; //this is to keep track if the AI is performing an action.
    protected bool staggering = false; //used to set the AI into the staggered state
    [HideInInspector]
    public int incoming_damage = 0; //the damage that will be applied to the enemy.
    protected dodge_direction dodge = dodge_direction.not_dodging; //AI will decide to move left or right in the dodge.
    private Quaternion look_rotation; //used to rotate the direction the AI is looking at when the player is nearby.
    private Vector3 direction; //also used for the rotation of where the AI is looking at when the player is nearby.
    protected BasicAI_Weapon weapon_script; //to access its weapon to toggle collider on or off.
    
    private Transform target; //the target the AI will be chasing
    private Vector3 backward_dir; //the backward_dir of the AI. Used whenever the AI needs to get knocked back.
    protected ai_state current_state = ai_state.idle; //instantiates the ai with an idle state.

	//TJ add
	[HideInInspector] public bool player_countering = false;

    protected IEnumerator Dodge() //this is the dodge of the AI, it will randomly choose left or right dodges.
    {
        alerted = false; //set alert to false to make the AI stop chasing the player to allow dodging.
        dodge = (dodge_direction)Random.Range(0, 2); //randomly picks a direction to dodge. 0 is left, 1 is right.
        if (dodge == dodge_direction.left)
        {
            animator.Play("LeftDodge");
        }
        else if (dodge == dodge_direction.right)
        {
            animator.Play("RightDodge");
        }
        cooldown_action = Random.Range(cooldown_action_min, cooldown_action_max);
		yield return new WaitForSeconds(dodge_time); 
        alerted = true; //set alert back to true to make the AI chase the player again.
        dodge = dodge_direction.not_dodging; //reset the dodge's direction to no direction otherwise known as not_dodging.
        yield return new WaitForSeconds(cooldown_action);
		performing_action = false;  //the AI is done with the action
    }

    protected IEnumerator Attack_1() //this is the right swing attack. starts from the left arm position
    {
        animator.Play("JumpSwing");
        weapon_script.StartCoroutine("ToggleCollider");
        //move towards the player while the attack goes through.
        transform.GetComponent<UnityEngine.AI.NavMeshAgent>().stoppingDistance = 3f;
        cooldown_action = Random.Range(cooldown_action_min, cooldown_action_max);
        //move for half a second, then attack
        yield return new WaitForSeconds(0.5f);
        transform.GetComponent<UnityEngine.AI.NavMeshAgent>().stoppingDistance = 5f;
        yield return new WaitForSeconds(performing_time);
        //waits for performing time to be done... then add more things here if needed for after action
        yield return new WaitForSeconds(cooldown_action);
        //this is the time to wait for the next action to be performed.
		performing_action = false;  //the AI is done with the action
    }

    protected IEnumerator Attack_2() //this is the left swing attack. starts from the right arm position
    {
        animator.Play("JumpSwing");
        weapon_script.StartCoroutine("ToggleCollider");
        //move towards the player while the attack goes through.
        transform.GetComponent<UnityEngine.AI.NavMeshAgent>().stoppingDistance = 3f;
        cooldown_action = Random.Range(cooldown_action_min, cooldown_action_max);
        //move for half a second, then attack
        yield return new WaitForSeconds(0.5f);
        transform.GetComponent<UnityEngine.AI.NavMeshAgent>().stoppingDistance = 5f;
        yield return new WaitForSeconds(performing_time);
        //waits for performing time to be done... then add more things here if needed for after action
        yield return new WaitForSeconds(cooldown_action);
        //this is the time to wait for the next action to be performed.
        performing_action = false;  //the AI is done with the action
    }

    protected IEnumerator Attack_3() //this is the diagonal swing attack. stops from the top right arm position 
    {
        animator.Play("TripleAttack");
        weapon_script.StartCoroutine("ToggleCollider");
        //move towards the player while the attack goes through.
        transform.GetComponent<UnityEngine.AI.NavMeshAgent>().stoppingDistance = 3f;
        cooldown_action = Random.Range(cooldown_action_min, cooldown_action_max);
        //move for half a second, then attack
        yield return new WaitForSeconds(0.5f);
        transform.GetComponent<UnityEngine.AI.NavMeshAgent>().stoppingDistance = 5f;
        yield return new WaitForSeconds(performing_time);
        //waits for performing time to be done... then add more things here if needed for after action
        yield return new WaitForSeconds(cooldown_action);
        //this is the time to wait for the next action to be performed.
        performing_action = false;  //the AI is done with the action
    }

    protected IEnumerator MoveBack()
    {
        animator.Play("BackwardsWalk");
        //set alert to false to make the AI stop chasing the player to allow moving backwards and set nav mesh speed/acceleration to 0 to stop the AI completely.
        alerted = false;
        transform.GetComponent<UnityEngine.AI.NavMeshAgent>().acceleration = 0;
        transform.GetComponent<UnityEngine.AI.NavMeshAgent>().speed = 0;
        transform.GetComponent<UnityEngine.AI.NavMeshAgent>().velocity = Vector3.zero;
        cooldown_action = Random.Range(cooldown_action_min, cooldown_action_max);
        //start moving backwards
        getting_knockback = true;
        yield return new WaitForSeconds(moveback_duration);
        getting_knockback = false;
        yield return new WaitForSeconds(cooldown_action);
        transform.GetComponent<UnityEngine.AI.NavMeshAgent>().stoppingDistance = 5f;
        alerted = true; //set alert back to true to make the AI chase the player again.
        transform.GetComponent<UnityEngine.AI.NavMeshAgent>().acceleration = acceleration;
        transform.GetComponent<UnityEngine.AI.NavMeshAgent>().speed = movement_speed;
        performing_action = false;  //the AI is done with the action
    }

    //set up for the AI attacks and movement.
    private void Awake()
    {
        end = GetComponentInParent<BattleArea_End>();
        //find the backward direction for knockbacks.
		backward_dir = transform.TransformDirection(Vector3.forward);
        //access to the weapon script;
        weapon_script = my_weapon.GetComponent<BasicAI_Weapon>();
        //sets the target of this AI as the player
        target = GameObject.Find("Player").transform;
        //apply the acceleration variable of this to the nav mesh agent.
        transform.GetComponent<UnityEngine.AI.NavMeshAgent>().acceleration = acceleration;
        //apply the speed variable to the nav mesh agent.
        transform.GetComponent<UnityEngine.AI.NavMeshAgent>().speed = movement_speed;
		knockback_force = base_knockback_force;
    }
		

	public IEnumerator DamageEnemy(int incoming_damage) //first will apply damage, and then stagger the enemy for a certain duration
    {
        animator.Play("Stagger");
        //apply damage and checks if the enemy dies from the damage.
        enemy_health -= incoming_damage;
        //Debug.Log("POOP");
        //set the bools to allow knockback and prevent actions/movements.
        getting_knockback = true;
		knockback_force += incoming_damage;
		//Debug.Log (knockback_force);
        //create damage effect particles
		if (!player_countering) {
			GameObject effect = Instantiate (damaged_effect, transform.position, transform.rotation);
			Destroy (effect, 1f);
		}
        //change the AI state to staggered for animations.
        current_state = ai_state.staggered;
        //set staggering to true to affect fixedupdate to prevent the ai from doing any actions.
		staggering = true;
        //set to false to stop the AI from following the player with the navmesh.
        alerted = false;
        //stop the ai from moving by setting the acceleration, speed, and velocity to 0.
        transform.GetComponent<UnityEngine.AI.NavMeshAgent>().acceleration = 0;
        transform.GetComponent<UnityEngine.AI.NavMeshAgent>().speed = 0;
        transform.GetComponent<UnityEngine.AI.NavMeshAgent>().velocity = Vector3.zero;
		player_countering = false;
        yield return new WaitForSeconds(0.01f);
    }

    public IEnumerator Death() //the death coroutine that will play when the enemy hits 0 health.
    {
        //print("the enemy is dying...");
        yield return new WaitForSeconds(death_duration);
        //end.enemyList.Remove(gameObject);
        //this.transform.parent = null;
		if (end) {
			end.enemiesDead--;
		}
		ScoreSystem.Singleton_ScoreSystem.combo_addKill ();
        //end.enemyList.Remove(gameObject);
        this.gameObject.SetActive(false);
    }

	public IEnumerator reset()
	{
        //Debug.Log (transform.name);
        gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
        //Debug.Log("start the reset");
        enemy_health = enemy_start_health;
		first_alert = false;
        alerted = false;
        berserk_mode = false;
        berserk_flame1.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;
        //Debug.Log("Reset has finished.");
	}

    void FixedUpdate () 
	{
        if (staggering)
        {
            current_stagger_dur += Time.deltaTime;
            //move the ai state to staggered, and set alerted to false to stop their movement.
            current_state = ai_state.staggered;
            alerted = false;

            //get knocked backwards.
            if (getting_knockback)
            {
                transform.Translate(backward_dir * knockback_force * Time.smoothDeltaTime, Space.Self);
            }

            //once the current_stagger_dur is done with knockback, stop the knockback by turning off getting_knockback.
            if (current_stagger_dur >= knockback_duration && getting_knockback)
            {
                getting_knockback = false;
				knockback_force = base_knockback_force;
            }

            //once the stagger duration is up, restore all of the old values of the AI to move and attack.
            if (current_stagger_dur >= knockback_duration + stagger_after_knockback_dur)
            {
                transform.GetComponent<UnityEngine.AI.NavMeshAgent>().acceleration = acceleration;
                transform.GetComponent<UnityEngine.AI.NavMeshAgent>().speed = movement_speed;
                staggering = false;
                alerted = true;
                current_stagger_dur = 0;
                current_state = ai_state.idle;
            }
        }
        //it is not staggering so run through the usual routines.
        else
        {
            if(berserk_mode && !first_berserk)
            {
                chance_to_dodge = 15;
                berserk_flame1.SetActive(true);
                berserk_mode = false;
                first_berserk = true;
            }

            distance_to_player = Vector3.Distance(target.position, transform.position); //calculate distance to player

            //used for checking and testing purposes.
            if (check_to_damage)
            {
                StartCoroutine("DamageEnemy", 11);
                check_to_damage = false;
            }


            if (distance_to_player < 30 && !first_alert) //if the player is close enough, this will set the AI to be alerted. 
                                                         //if the enemy is alerted then it will chase the player. AKA aggro range
            {
                alerted = true;
                first_alert = true;
            }

            //if the enemy reached 0 hp, it is dead so it will be put into the dying state then go through the death coroutine.
            if (enemy_health <= 0)
            {
                current_state = ai_state.dying;
                StartCoroutine("Death");
            }


            if (distance_to_player <= 20) //if the player is close, this will keep the AI rotated towards the player
            {
                direction = (target.position - transform.position).normalized;
                look_rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, look_rotation, Time.deltaTime * turn_speed);
            }

            if (getting_knockback)
            {
                transform.Translate(backward_dir * moveback_speed * Time.smoothDeltaTime, Space.Self);
            }


            if (alerted && !performing_action && distance_to_player > 3f)  //if the AI is aggro, then it will chase the player.
            {
                //telling this object to chase after the target's position, otherwise known as the player's position.
                animator.Play("ForwardWalk");
                transform.GetComponent<UnityEngine.AI.NavMeshAgent>().destination = target.position;
                animator.SetBool("isMoving", true);
            }
            else
            {
                animator.SetBool("isMoving", false);
            }


            //if the ai is close enough and not already performing an action
            //then it will pick a random action to do
            //and then perform the selected action.
            if (distance_to_player <= 8f && !performing_action)
            {
                performing_action = true;  //the AI is now doing an action, used to make sure it is only doing one action.
                //randomly choose between an movement or attack behavior, there is a 35% chance of being a movement behavior and 65% chance of an attack.
                //comment the line below to "control" the AI.
                action_selection = Random.Range(0, 100);

                if(action_selection <= chance_to_dodge)
                {
                    ai_state current_state = (ai_state)Random.Range(2, 4); //randomly select to either dodge or moveback.
                    switch (current_state) //based on the choice, do the corresponding coroutines
                    {
                        case ai_state.dodging:
                            StartCoroutine("Dodge");
                            break;
                        case ai_state.moveback:
                            if (distance_to_player > 6f)
                            {
                                StartCoroutine("Dodge");
                            }
                            else
                            {
                                StartCoroutine("MoveBack");
                            }
                            break;
                    }// at the end of each coroutines, it will set performing_action back to false to allow for a loop if the ai is still within range.
                }
                else
                {
                    ai_state current_state = (ai_state)Random.Range(4, 7); //randomly select within the attack states.
                    switch (current_state) //based on the choice, do the corresponding coroutines
                    {
                        case ai_state.attack_1:
                            StartCoroutine("Attack_1");
                            break;
                        case ai_state.attack_2:
                            StartCoroutine("Attack_2");
                            break;
                        case ai_state.attack_3:
                            StartCoroutine("Attack_3");
                            break;
                    }
                }

                
            }

            // the dodge works by moving towards transform positions of empty game objects that are attached to the ai.
            if (dodge == dodge_direction.left) //this will make the ai move to the left or right when they are in that dodge state.
            {
                transform.position = Vector3.Lerp(transform.position, left_dodge.transform.position, dodge_speed * Time.deltaTime);
            }
            else if (dodge == dodge_direction.right)
            {
                transform.position = Vector3.Lerp(transform.position, right_dodge.transform.position, dodge_speed * Time.deltaTime);
            }
            
        }
    }

}