using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerCombat
{
	public delegate void PlayerEventHandler();
	public static event PlayerEventHandler OnPlayerDeath;

	private int HitKnockDownThreshold = 3; //the number of times the player can be hit before being knocked down
	private int HitKnockDownCount = 0; //the number of times the player is hit in a row
	private int HitKnockDownResetTime = 2; //the time before the hitknockdown counter resets
	private float LastHitTime = 0; // the time when we were hit 
	private bool BlockAttacksFromBehind = true; //block enemy attacks coming from behind
	private bool isDead = false; //true if this player has died
	private List<PLAYERSTATE> HitStates = new List<PLAYERSTATE> { PLAYERSTATE.HIT, PLAYERSTATE.DEATH, PLAYERSTATE.KNOCKDOWN }; //a list of states where the player was hit

	//we are hit
	private void Hit(DamageObject d)
	{
		if (!HitStates.Contains(playerState.currentState))
		{
			bool wasHit = true;
			UpdateHitCounter();

			//defend
			if (playerState.currentState == PLAYERSTATE.DEFENDING)
			{
				if (BlockAttacksFromBehind || isFacingTarget(d.inflictor)) wasHit = false;
				if (!wasHit)
				{
					GlobalAudioPlayer.PlaySFX("Defend");
					animator.ShowDefendEffect();
					animator.CamShakeSmall();

					if (isFacingTarget(d.inflictor))
					{
						animator.AddForce(-1.5f);
					}
					else
					{
						animator.AddForce(1.5f);
					}
				}
			}

			//knockdown hit
			if (HitKnockDownCount >= HitKnockDownThreshold)
			{
				d.attackType = AttackType.KnockDown;
				HitKnockDownCount = 0;
			}

			//getting hit while being in the air also causes a knockdown
			if (!playerMovement.playerIsGrounded())
			{
				d.attackType = AttackType.KnockDown;
				HitKnockDownCount = 0;
			}

			//we are dead
			if (GetComponent<HealthSystem>() != null && GetComponent<HealthSystem>().CurrentHp == 0)
			{
				gameObject.SendMessage("Death");
			}

			//play hit SFX
			if (wasHit) GlobalAudioPlayer.PlaySFX("PunchHit");

			//start knockDown sequence
			if (wasHit && playerState.currentState != PLAYERSTATE.KNOCKDOWN)
			{
				GetComponent<HealthSystem>().SubstractHealth(d.damage);
				animator.ShowHitEffect();

				if (d.attackType == AttackType.KnockDown)
				{
					playerState.SetState(PLAYERSTATE.KNOCKDOWN);
					StartCoroutine(KnockDown(d.inflictor));
				}
				else
				{
					playerState.SetState(PLAYERSTATE.HIT);
					animator.Hit();
				}
			}
		}
	}

	//updates the hit counter
	void UpdateHitCounter()
	{
		if (Time.time - LastHitTime < HitKnockDownResetTime)
		{
			HitKnockDownCount += 1;
		}
		else
		{
			HitKnockDownCount = 1;
		}
		LastHitTime = Time.time;
	}

	//player knockDown coroutine
	public IEnumerator KnockDown(GameObject inflictor)
	{
		animator.KnockDown();
		float t = 0;
		float travelSpeed = 2f;
		Rigidbody2D rb = GetComponent<Rigidbody2D>();

		//get the direction of the attack
		int dir = inflictor.transform.position.x > transform.position.x ? 1 : -1;

		//look towards the direction of the incoming attack
		playerMovement.LookToDir((Direction)dir);

		while (t < 1)
		{
			rb.velocity = Vector2.left * dir * travelSpeed;
			t += Time.deltaTime;
			yield return 0;
		}

		//stop traveling
		rb.velocity = Vector2.zero;
		yield return new WaitForSeconds(1);

		//reset
		playerState.currentState = PLAYERSTATE.IDLE;
		animator.Idle();
	}

	//the player has died
	void Death()
	{
		isDead = true;
		animator.Death();
		Invoke("GameOver", 2f);
		EnemyManager.PlayerHasDied();
	}

	//gameOver
	void GameOver()
	{
		OnPlayerDeath();
	}
}
