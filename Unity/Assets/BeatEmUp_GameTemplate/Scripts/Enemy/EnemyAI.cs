using UnityEngine;
using System.Collections.Generic;

public class EnemyAI : EnemyActions
{

	[Header("AI")]
	public Range range; //the range to target
	public EnemyTactic enemyTactic;//the current tactic
	public float XDistance = 0;
	public float YDistance = 0;
	public bool enableAI;
	private List<EnemyState> ActiveAIStates = new List<EnemyState> { EnemyState.Idle, EnemyState.Run, EnemyState.Walk }; //a list of states where the AI is executed
	private List<EnemyState> HitStates = new List<EnemyState> { EnemyState.Death, EnemyState.Knockdown, EnemyState.KnockdownGrounded }; //a list of states where the enemy is hit

	void Start()
	{
		animator = GFX.GetComponent<EnemyAnimator>();
		rb = GetComponent<Rigidbody2D>();
		EnemyManager.enemyList.Add(gameObject);
		RandomizeValues();
	}

	void OnEnable()
	{
		SetTarget2Player();
	}

	void Update()
	{
		if (!isDead && enableAI)
		{
			if (ActiveAIStates.Contains(enemyState) && targetSpotted)
			{
				AI();

			}
			else
			{

				//look for a target
				Look4Target();
			}
		}
		UpdateSpriteSorting();
	}

	void AI()
	{
		LookAtTarget();
		range = GetRangeToTarget();

		//attack range
		if (range == Range.AttackRange)
		{
			if (enemyTactic == EnemyTactic.Engage) Attack();
			if (enemyTactic == EnemyTactic.KeepShortDistance) MoveTo(closeRangeDistance, walkSpeed);
			if (enemyTactic == EnemyTactic.KeepMidDistance) MoveTo(midRangeDistance, walkSpeed);
			if (enemyTactic == EnemyTactic.KeepFarDistance) MoveTo(farRangeDistance, walkSpeed);
			if (enemyTactic == EnemyTactic.StandStill) Idle();
		}

		//close range
		if (range == Range.CloseRange)
		{
			if (enemyTactic == EnemyTactic.Engage) MoveTo(attackRange - .2f, walkSpeed);
			if (enemyTactic == EnemyTactic.KeepShortDistance) MoveTo(closeRangeDistance, walkSpeed);
			if (enemyTactic == EnemyTactic.KeepMidDistance) MoveTo(midRangeDistance, walkSpeed);
			if (enemyTactic == EnemyTactic.KeepFarDistance) MoveTo(farRangeDistance, walkSpeed);
			if (enemyTactic == EnemyTactic.StandStill) Idle();
		}

		//mid range
		if (range == Range.MidRange)
		{
			if (enemyTactic == EnemyTactic.Engage) MoveTo(attackRange - .2f, walkSpeed);
			if (enemyTactic == EnemyTactic.KeepShortDistance) MoveTo(closeRangeDistance, walkSpeed);
			if (enemyTactic == EnemyTactic.KeepMidDistance) MoveTo(midRangeDistance, walkSpeed);
			if (enemyTactic == EnemyTactic.KeepFarDistance) MoveTo(farRangeDistance, walkSpeed);
			if (enemyTactic == EnemyTactic.StandStill) Idle();
		}

		//far range
		if (range == Range.FarRange)
		{
			if (enemyTactic == EnemyTactic.Engage) MoveTo(attackRange - .2f, walkSpeed);
			if (enemyTactic == EnemyTactic.KeepShortDistance) MoveTo(closeRangeDistance, walkSpeed);
			if (enemyTactic == EnemyTactic.KeepMidDistance) MoveTo(midRangeDistance, walkSpeed);
			if (enemyTactic == EnemyTactic.KeepFarDistance) MoveTo(farRangeDistance, walkSpeed);
			if (enemyTactic == EnemyTactic.StandStill) Idle();
		}
	}

	public void Hit(DamageObject d)
	{

		//stop moving
		Move(Vector3.zero, 0);

		//look towards inflictor
		if (target != null) target = d.inflictor;
		LookAtTarget();

		//show hit effect
		if (!isDead)
		{
			ShowHitEffectAtPosition(transform.position + Vector3.up * Random.Range(1.0f, 2.0f));
			GlobalAudioPlayer.PlaySFX("PunchHit");
		}

		//enemy can be hit
		if (!HitStates.Contains(enemyState) && !isDead)
		{

			//showHitEffectAtPosition
			ShowHitEffectAtPosition(transform.position + Vector3.up * Random.Range(1.0f, 2.0f));

			//sfx
			GlobalAudioPlayer.PlaySFX("PunchHit");

			//knockdown
			if (d.attackType == AttackType.KnockDown)
			{

				enemyState = EnemyState.Knockdown;
				StartCoroutine(KnockDown(DirectionToPos(d.inflictor.transform.position.x)));

			}
			else
			{

				//normal hit
				animator.Hit();
				enemyState = EnemyState.Hit;
			}
		}

		//unit is dead
		if (GetComponent<HealthSystem>() != null && GetComponent<HealthSystem>().CurrentHp == 0 && !isDead)
		{
			Move(Vector3.zero, 0);
			UnitIsDead();
		}
	}

	//Unit has died
	void UnitIsDead()
	{
		isDead = true;
		enableAI = false;
		Move(Vector3.zero, 0);
		enemyState = EnemyState.Death;
		animator.Death();
		StartCoroutine(RemoveEnemy());
		EnemyManager.RemoveEnemyFromList(gameObject);
	}

	//sets the current range
	private Range GetRangeToTarget()
	{
		XDistance = DistanceToTargetX();
		YDistance = DistanceToTargetY();

		//AttackRange
		if (XDistance <= attackRange && YDistance <= .2f) return Range.AttackRange;

		//Close Range
		if (XDistance > attackRange && XDistance < midRangeDistance) return Range.CloseRange;

		//Mid range
		if (XDistance > closeRangeDistance && XDistance < farRangeDistance) return Range.MidRange;

		//Far range
		if (XDistance > farRangeDistance) return Range.FarRange;

		return Range.FarRange;
	}

	//set an enemy tactic
	public void SetEnemyTactic(EnemyTactic tactic)
	{
		enemyTactic = tactic;
	}

	//checks if the target is in sight
	private void Look4Target()
	{
		targetSpotted = DistanceToTargetX() < sightDistance;
	}

	//randomizes values for variation
	private void RandomizeValues()
	{
		walkSpeed *= Random.Range(.8f, 1.2f);
		attackInterval *= Random.Range(.8f, 1.2f);
	}
}

public enum EnemyTactic
{
	Engage = 0,
	KeepShortDistance = 1,
	KeepMidDistance = 2,
	KeepFarDistance = 3,
	StandStill = 4,
}

public enum Range
{
	AttackRange,
	CloseRange,
	MidRange,
	FarRange,
}
