using System.Collections.Generic;
using UnityEngine;

public partial class PlayerCombat
{
	// attack
	[Header("Attack Data")]
	public DamageObject[] PunchAttackData; //a list of punch attacks
	public DamageObject[] KickAttackData; //a list of kick Attacks
	public DamageObject JumpKickData; //jump kick Attack

	private int attackNum = 1; //the current attack number
	private bool continuePunchCombo; //true if a punch combo needs to continue
	private bool continueKickCombo; //true if the a kick combo needs to  continue
	private float LastAttackTime = 0; //time of the last attack
	private bool targetHit; //true if the last hit has hit a target
	private bool ChangeDirDuringCombo = false; //allows player to change direction at the start of an attack
	private bool ChangeDirAtLastHit = true; //allows player to change direction at the last hit
	private bool jumpKickActive; //true if a jump kick has been done
	private bool defend = false; //true if the defend button is down
	private List<PLAYERSTATE> AttackStates = new List<PLAYERSTATE> { PLAYERSTATE.IDLE, PLAYERSTATE.MOVING, PLAYERSTATE.JUMPING, PLAYERSTATE.PUNCH, PLAYERSTATE.KICK, PLAYERSTATE.DEFENDING }; //a list of states where the player can attack

	//do a punch attack
	private void doPunchAttack()
	{
		playerState.SetState(PLAYERSTATE.PUNCH);
		animator.Punch(GetNextAttackNum());
		LastAttackTime = Time.time;
	}

	//do a kick attack
	void doKickAttack()
	{
		playerState.SetState(PLAYERSTATE.KICK);
		animator.Kick(GetNextAttackNum());
		LastAttackTime = Time.time;
	}

	//do jump kick attack
	void doJumpKickAttack()
	{
		playerState.SetState(PLAYERSTATE.JUMPKICK);
		jumpKickActive = true;
		animator.JumpKick();
		LastAttackTime = Time.time;
	}

	//start defending
	private void Defend()
	{
		playerState.SetState(PLAYERSTATE.DEFENDING);
		animator.StartDefend();
	}

	//returns the next attack number in the combo chain
	private int GetNextAttackNum()
	{
		if (playerState.currentState == PLAYERSTATE.PUNCH)
		{
			attackNum = Mathf.Clamp(attackNum += 1, 0, PunchAttackData.Length - 1);
			if (Time.time - LastAttackTime > PunchAttackData[attackNum].comboResetTime || !targetHit)
				attackNum = 0;
			return attackNum;

		}
		else if (playerState.currentState == PLAYERSTATE.KICK)
		{
			attackNum = Mathf.Clamp(attackNum += 1, 0, KickAttackData.Length - 1);
			if (Time.time - LastAttackTime > KickAttackData[attackNum].comboResetTime || !targetHit)
				attackNum = 0;
			return attackNum;
		}
		return 0;
	}

	//deals damage to an enemy target
	private void DealDamageToEnemy(GameObject enemy)
	{
		DamageObject d = new DamageObject(0, gameObject);

		if (playerState.currentState == PLAYERSTATE.PUNCH)
		{
			d = PunchAttackData[attackNum];
		}
		else if (playerState.currentState == PLAYERSTATE.KICK)
		{
			d = KickAttackData[attackNum];
		}
		else if (playerState.currentState == PLAYERSTATE.THROWKNIFE)
		{
			d.damage = currentWeapon.data;
			d.attackType = AttackType.KnockDown;
		}
		else if (playerState.currentState == PLAYERSTATE.JUMPKICK)
		{
			d = JumpKickData;
			jumpKickActive = false; //hit only 1 enemy
		}

		d.inflictor = gameObject;

		//subsctract health from enemy
		HealthSystem hs = enemy.GetComponent<HealthSystem>();
		if (hs != null)
		{
			hs.SubstractHealth(d.damage);
		}

		enemy.GetComponent<EnemyAI>().Hit(d);
	}

	//checks if we have hit something (animation event)
	public void CheckForHit()
	{

		int dir = (int)GetComponent<PlayerMovement>().getCurrentDirection();
		Vector3 playerPos = transform.position + Vector3.up * 1.5f;
		LayerMask enemyLayerMask = LayerMask.NameToLayer("Enemy");
		LayerMask itemLayerMask = LayerMask.NameToLayer("Item");

		//do a raycast to see which enemies/objects are in attack range
		RaycastHit2D[] hits = Physics2D.RaycastAll(playerPos, Vector3.right * dir, getAttackRange(), 1 << enemyLayerMask | 1 << itemLayerMask);
		Debug.DrawRay(playerPos, Vector3.right * dir, Color.red, getAttackRange());

		//we have hit something
		for (int i = 0; i < hits.Length; i++)
		{

			LayerMask layermask = hits[i].collider.gameObject.layer;

			//we have hit an enemy
			if (layermask == enemyLayerMask)
			{
				GameObject enemy = hits[i].collider.gameObject;
				if (ObjInYRange(enemy))
				{
					DealDamageToEnemy(hits[i].collider.gameObject);
					targetHit = true;
				}
			}

			//we have hit an item
			if (layermask == itemLayerMask)
			{
				GameObject item = hits[i].collider.gameObject;
				if (ObjInYRange(item))
				{
					var cop = item.GetComponent<ItemInteractable>();
					if (!cop.item.isPickup)
                    {
						cop.ActivateItem(gameObject);
						ShowHitEffectAtPosition(hits[i].point);
					}
				}
			}
		}

		//we havent hit anything
		if (hits.Length == 0)
		{
			targetHit = false;
		}
	}

	//returns the attack range of the current attack
	private float getAttackRange()
	{
		if (playerState.currentState == PLAYERSTATE.PUNCH && attackNum <= PunchAttackData.Length)
		{
			return PunchAttackData[attackNum].range;
		}
		else if (playerState.currentState == PLAYERSTATE.KICK && attackNum <= KickAttackData.Length)
		{
			return KickAttackData[attackNum].range;
		}
		else if (jumpKickActive)
		{
			return JumpKickData.range;
		}
		else
		{
			return 0f;
		}
	}

	//spawns the hit effect
	private void ShowHitEffectAtPosition(Vector3 pos)
	{
		GameObject.Instantiate(Resources.Load("HitEffect"), pos, Quaternion.identity);
	}
}
