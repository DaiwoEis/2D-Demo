using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerState))]
public partial class PlayerCombat : MonoBehaviour
{

	// common
	private PlayerAnimator animator; //link to the animator component
	private PlayerState playerState; //the state of the player
	private float yInteractDistance = 0.4f;  //the Y distance from which the player is able to hit an enemy
	private PlayerMovement playerMovement;

	#region Control

	private void OnEnable()
	{
		InputManager.onCombatInputEvent += CombatInputEvent;
	}

	private void OnDisable()
	{
		InputManager.onCombatInputEvent -= CombatInputEvent;
	}

	private void Awake()
	{
		animator = GetComponentInChildren<PlayerAnimator>();
		playerState = GetComponent<PlayerState>();
		playerMovement = GetComponent<PlayerMovement>(); ;
	}

	private void Update()
	{

		//checks for a jump kick hit each frame when jumpKickActive is true
		if (jumpKickActive && !playerMovement.playerIsGrounded())
		{
			CheckForHit();
		}

		//checks if the defend button is being held down and otherwise goes back to the idle state
		if (defend)
		{
			defend = false;
			Defend();
		}
		else
		{
			if (playerState.currentState == PLAYERSTATE.DEFENDING)
			{
				playerMovement.Idle();
				animator.StopDefend();
			}
		}
	}

	//a combat input event has taken place
	private void CombatInputEvent(string action)
	{
		if (AttackStates.Contains(playerState.currentState) && !isDead)
		{
			if (playerState.currentState == PLAYERSTATE.JUMPING)
			{
				if (action == "Punch" || action == "Kick")
					doJumpKickAttack();
			}
			else if (playerState.currentState == PLAYERSTATE.IDLE)
			{
				if (action == "Punch")
					if (itemInRange != null && ObjInYRange(itemInRange))
						InteractWithItem();
					else if (currentWeapon != null && currentWeapon.itemName == "Knife")
						StartThrowAttack();
					else
						doPunchAttack();
				if (action == "Kick")
					if (itemInRange != null && ObjInYRange(itemInRange))
						InteractWithItem();
					else
						doKickAttack();
			}
			else if (playerState.currentState == PLAYERSTATE.MOVING)
			{
				if (action == "Punch")
					if (itemInRange != null && ObjInYRange(itemInRange))
						InteractWithItem();
					else if (currentWeapon != null && currentWeapon.itemName == "Knife")
						StartThrowAttack();
					else
						doPunchAttack();
				if (action == "Kick")
					if (itemInRange != null && ObjInYRange(itemInRange))
						InteractWithItem();
					else
						doKickAttack();
			}
			else if (playerState.currentState == PLAYERSTATE.DEFENDING)
			{
				if (action == "Punch")
					if (itemInRange != null && ObjInYRange(itemInRange))
						InteractWithItem();
					else if (currentWeapon != null && currentWeapon.itemName == "Knife")
						StartThrowAttack();
				if (action == "Kick")
					if (itemInRange != null && ObjInYRange(itemInRange))
						InteractWithItem();
			}
			else if (playerState.currentState == PLAYERSTATE.PUNCH)
			{
				if (action == "Punch")
					if (itemInRange != null && ObjInYRange(itemInRange))
						InteractWithItem();
					else if (attackNum < PunchAttackData.Length - 1)
					{
						continuePunchCombo = true;
						continueKickCombo = false;
					}
			}
			else if (playerState.currentState == PLAYERSTATE.KICK)
			{
				if (action == "Kick")
					if (itemInRange != null && ObjInYRange(itemInRange))
						InteractWithItem();
					else if (attackNum < KickAttackData.Length - 1)
					{
						continueKickCombo = true;
						continuePunchCombo = false;
					}
			}

			//the defend button was pressed
			if (action == "Defend" && playerMovement.playerIsGrounded())
			{
				defend = true;
			}
		}
	}

	//the attack is finished and the player is ready for new input
	public void Ready()
	{
		//continue to the next attack
		if (continuePunchCombo || continueKickCombo)
		{
			if (continuePunchCombo)
			{
				doPunchAttack();
				continuePunchCombo = false;
			}
			else if (continueKickCombo)
			{
				doKickAttack();
				continueKickCombo = false;
			}

			//allow direction change during a combo or if we haven't hit anything
			if (ChangeDirDuringCombo || !targetHit)
			{
				playerMovement.updateDirection();
			}

			//allow a direction change at the last attack of a combo
			if (playerState.currentState == PLAYERSTATE.PUNCH && ChangeDirAtLastHit && attackNum == PunchAttackData.Length - 1)
			{
				playerMovement.updateDirection();
			}
			else if (playerState.currentState == PLAYERSTATE.KICK && ChangeDirAtLastHit && attackNum == KickAttackData.Length - 1)
			{
				playerMovement.updateDirection();
			}
		}
		else
		{
			//go back to idle
			playerState.SetState(PLAYERSTATE.IDLE);
		}
		jumpKickActive = false;
	}

    #endregion

    #region Common

    //returns the closest enemy in front of us
    private GameObject GetClosestEnemyFacing()
	{
		List<GameObject> FacingEnemies = new List<GameObject>();
		GameObject nearestEnemy = null;
		Vector3 playerTrans = transform.position;
		float distance = Mathf.Infinity;

		//generate a list of enemies standing in front of us (on the same line (y))
		for (int i = EnemyManager.enemyList.Count - 1; i > -1; i--)
		{
			GameObject enemy = EnemyManager.enemyList[i];
			if (enemy.activeSelf && isFacingTarget(enemy))
			{
				float YDist = Mathf.Abs(enemy.transform.position.y - playerTrans.y);
				if (YDist < yInteractDistance)
					FacingEnemies.Add(enemy);
			}
		}

		//find closest enemy
		for (int i = 0; i < FacingEnemies.Count; i++)
		{
			float enemyDist = Mathf.Abs(FacingEnemies[i].transform.position.x - playerTrans.x);
			if (enemyDist < distance)
			{
				distance = enemyDist;
				nearestEnemy = FacingEnemies[i];
			}
		}

		return nearestEnemy;
	}

	//returns true if the object is within y range
	private bool ObjInYRange(GameObject obj)
	{
		float YDist = Mathf.Abs(obj.transform.position.y - transform.position.y);
		if (YDist < yInteractDistance)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	//returns true is the player is facing the enemy
	public bool isFacingTarget(GameObject g)
	{
		Direction dir = playerMovement.getCurrentDirection();
		if ((g.transform.position.x > transform.position.x && dir == Direction.Right) || (g.transform.position.x < transform.position.x && dir == Direction.Left))
			return true;
		else
			return false;
	}

	#endregion
}
