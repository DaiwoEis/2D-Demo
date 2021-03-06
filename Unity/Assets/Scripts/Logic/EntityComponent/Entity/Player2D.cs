using System;
using Lockstep.Math;
using UnityEngine;
using System.Collections.Generic;
using Lockstep.UnsafeCollision2D;

namespace Lockstep.Game
{
    [Serializable]
    public partial class Player2D : BaseEntity
    {
		public int localId;
		public PlayerInput input = new PlayerInput();

		public LFloat walkSpeed = new LFloat(true, 1000);
		public LFloat jumpTime = new LFloat(true, 9000);
		public LFloat jumpHeight = new LFloat(true, 2500);

		public Direction currentDirection = Direction.Right;
		public LVector2 inputDirection;

		[Backup] private bool isDead = false;
		[Backup] private LFloat screenEdgeHorizontal = new LFloat(true, 80000);
		[Backup] private LFloat screenEdgeVertical = new LFloat(true, 18000);
		[Backup] private bool isGrounded;

		public PLAYERSTATE currentState = PLAYERSTATE.IDLE;

		[Backup] private LFloat stateTimer = LFloat.zero;

		[Backup] private bool jumping;
		[Backup] private LFloat jumpTimer;
		[Backup] private bool jumpKick;

		public CTimeLine timeLineCop = new CTimeLine();

		[ReRefBackup] public IPlayer2DView view;

		public CCollider attackCollider = new CCollider();
		public CCollider hitCollider = new CCollider();

		protected override void BindRef()
        {
            base.BindRef();

			view = null;
			RegisterComponent(timeLineCop);
			RegisterComponent(attackCollider);
			RegisterComponent(hitCollider);
			InitTimeLineRef();
			InitCollider();
			PhysicSystem.Instance.RebindCollider(attackCollider);
			PhysicSystem.Instance.RebindCollider(hitCollider);
		}

        private void InitCollider()
        {
			attackCollider.layer = 0;
			attackCollider.handler = this;
			attackCollider.type = CCollider.Type.Attack;

			hitCollider.layer = 0;
			hitCollider.handler = this;
			hitCollider.type = CCollider.Type.Hit;
		}

		private void InitTimeLineRef()
        {
			var dic = new Dictionary<string, Action<object[]>>
			{
				{ "PlaySound", objs => view?.PlaySound(objs[0] as string) },
				{ "UpdateCollider", objs => 
				{
					var attack = (int) objs[0];
					var center = (LVector2) objs[1];
					var size = (LVector2) objs[2];
					if (attack == 1)
						attackCollider.SetBound(new LRect(center, size));
					else
						hitCollider.SetBound(new LRect(center, size));
				} }
			};
			timeLineCop.SetCallBackDic(dic);
			timeLineCop.AddNode("punch", new TimeLineNode
			{
				time = LFloat.zero,
				parmas = new object[] { "Whoosh" },
				callBackName = "PlaySound"
			});
			timeLineCop.AddNode("punch", new TimeLineNode
			{
				time = new LFloat(true, 16),
				parmas = new object[] 
				{ 
					1, 
					new LVector2(new LFloat(true, 1300), new LFloat(true, 1300)), 
					new LVector2(new LFloat(true, 700), new LFloat(true, 350)) 
				},
				callBackName = "UpdateCollider"
			});
			timeLineCop.AddNode("kick", new TimeLineNode
			{
				time = new LFloat(true, 50),
				parmas = new object[] { "Whoosh" },
				callBackName = "PlaySound"
			});
			timeLineCop.AddNode("jump kick", new TimeLineNode
			{
				time = LFloat.zero,
				parmas = new object[] { "Whoosh" },
				callBackName = "PlaySound"
			});
			timeLineCop.ReBindRef();
		}

		public Player2D() : base()
        {
			InitTimeLineData();
		}

		private void InitTimeLineData()
        {
			timeLineCop.Clear();
			timeLineCop.AddTimeLine(new TimeLine
			{
				name = "punch",
				length = new LFloat(true, 233)
			});
			timeLineCop.AddTimeLine(new TimeLine
			{
				name = "kick",
				length = new LFloat(true, 350)
			});
			timeLineCop.AddTimeLine(new TimeLine
			{
				name = "jump kick",
				length = new LFloat(true, 517)
			});
		}

        public void SetState(PLAYERSTATE state, LFloat deltaTime)
		{
			OnStateExit(currentState);
			currentState = state;
			OnStateEnter(currentState);
			OnStateUpdate(currentState, deltaTime);
		}

        public override void DoUpdate(LFloat deltaTime)
        {
            base.DoUpdate(deltaTime);

			attackCollider.ClearBound();
			hitCollider.ClearBound();
			inputDirection = input.inputUV;
			OnStateUpdate(currentState, deltaTime);
		}

		private void OnStateEnter(PLAYERSTATE state)
        {
			stateTimer = LFloat.zero;
			if (state == PLAYERSTATE.IDLE)
            {
				view?.PlayAnim("Idle");
			}
			else if (state == PLAYERSTATE.JUMPING)
            {
				jumping = true;
				jumpTimer = LFloat.zero;
				isGrounded = false;
				jumpKick = false;
				view?.PlayAnim("Jump");
			}
			else if (state == PLAYERSTATE.PUNCH)
            {
				timeLineCop.StartTimeLine("punch");
				view?.PlayAnim("Punch");
            }
			else if (state == PLAYERSTATE.KICK)
			{
				timeLineCop.StartTimeLine("kick");
				view?.PlayAnim("Kick");
			}
			else if (state == PLAYERSTATE.DEFENDING)
			{
				view?.PlayAnim("Defend");
			}
			else if (state == PLAYERSTATE.MOVING)
            {
				view?.PlayAnim("Walk");
			}
		}

		private void OnStateExit(PLAYERSTATE state)
		{
			if (state == PLAYERSTATE.JUMPING)
            {
				transform.y = 0;
				ShowDustEffect();
				isGrounded = true;
			}
		}

		private void OnStateUpdate(PLAYERSTATE state, LFloat deltaTime)
		{
			stateTimer += deltaTime;
			if (currentState == PLAYERSTATE.IDLE)
            {
				if (input.jump)
					SetState(PLAYERSTATE.JUMPING, deltaTime);
				else if (input.punch)
					SetState(PLAYERSTATE.PUNCH, deltaTime);
				else if (input.kick)
					SetState(PLAYERSTATE.KICK, deltaTime);
				else if (input.defend)
					SetState(PLAYERSTATE.DEFENDING, deltaTime);
				else if (inputDirection.magnitude > 0)
					SetState(PLAYERSTATE.MOVING, deltaTime);
			}
			else if (currentState == PLAYERSTATE.MOVING)
			{
				Move(deltaTime);

				if (input.jump)
					SetState(PLAYERSTATE.JUMPING, deltaTime);
				else if (input.punch)
					SetState(PLAYERSTATE.PUNCH, deltaTime);
				else if (input.kick)
					SetState(PLAYERSTATE.KICK, deltaTime);
				else if (input.defend)
					SetState(PLAYERSTATE.DEFENDING, deltaTime);
				else if (inputDirection.magnitude == 0)
					SetState(PLAYERSTATE.IDLE, deltaTime);
			}
			else if (currentState == PLAYERSTATE.JUMPING)
			{
				Move(deltaTime);
				DoJump(deltaTime);
				CheckJumpKick();

				if (jumpTimer < LFloat.zero)
					SetState(PLAYERSTATE.IDLE, deltaTime);
			}
			else if (currentState == PLAYERSTATE.DEFENDING)
            {
				if (!input.defend)
					SetState(PLAYERSTATE.IDLE, deltaTime);
			}
			else if (currentState == PLAYERSTATE.PUNCH)
			{
				if (stateTimer >= new LFloat(true, 233))
					SetState(PLAYERSTATE.IDLE, deltaTime);
			}
			else if (currentState == PLAYERSTATE.KICK)
			{
				if (stateTimer >= new LFloat(true, 350))
					SetState(PLAYERSTATE.IDLE, deltaTime);
			}
		}

		private void CheckJumpKick()
        {
			if (!jumpKick && (input.kick || input.punch))
            {
				jumpKick = true;
				timeLineCop.StartTimeLine("jump kick");
				view?.PlayAnim("JumpKick");
            }
        }

        private void DoJump(LFloat deltaTime)
        {
			if (jumping)
            {
				jumpTimer += deltaTime;
				transform.y = LMath.Lerp(LFloat.zero, jumpHeight, LMath.SinLerp(LFloat.zero, LFloat.one, jumpTimer));
				if (jumpTimer > jumpTime / 2)
					jumping = false;
			}
			else
            {
				jumpTimer -= deltaTime;
				transform.y = LMath.Lerp(LFloat.zero, jumpHeight, LMath.SinLerp(LFloat.zero, LFloat.one, jumpTimer));
			}
        }

        private void Move(LFloat deltaTime)
		{
			LVector2 vector = new LVector2(inputDirection.x, inputDirection.y * new LFloat(true, 700)) * walkSpeed * deltaTime;
			transform.pos.x += vector.x;
			transform.pos.y += vector.y;

			if (inputDirection.magnitude > 0)
				UpdateDirection();
			LookToDir(currentDirection);

			view?.OnMove();
			//KeepPlayerInCameraView();
		}

		//void KeepPlayerInCameraView()
		//{
		//	Vector2 playerPosScreen = Camera.main.WorldToScreenPoint(transform.position);

		//	if (playerPosScreen.x + screenEdgeHorizontal > Screen.width && (playerPosScreen.y - screenEdgeVertical < 0))
		//	{
		//		transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width - screenEdgeHorizontal, screenEdgeVertical, transform.position.z - Camera.main.transform.position.z));

		//	}
		//	else if (playerPosScreen.x + screenEdgeHorizontal > Screen.width)
		//	{
		//		transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width - screenEdgeHorizontal, playerPosScreen.y, transform.position.z - Camera.main.transform.position.z));

		//	}
		//	else if (playerPosScreen.x - screenEdgeHorizontal < 0f && (playerPosScreen.y - screenEdgeVertical < 0))
		//	{
		//		transform.position = Camera.main.ScreenToWorldPoint(new Vector3(screenEdgeHorizontal, screenEdgeVertical, transform.position.z - Camera.main.transform.position.z));

		//	}
		//	else if (playerPosScreen.x - screenEdgeHorizontal < 0f)
		//	{
		//		transform.position = Camera.main.ScreenToWorldPoint(new Vector3(screenEdgeHorizontal, playerPosScreen.y, transform.position.z - Camera.main.transform.position.z));

		//	}
		//	else if ((playerPosScreen.y - screenEdgeVertical < 0) && (playerPosScreen.x + screenEdgeHorizontal > Screen.width))
		//	{
		//		transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width - screenEdgeHorizontal, screenEdgeVertical, transform.position.z - Camera.main.transform.position.z));

		//	}
		//	else if ((playerPosScreen.y - screenEdgeVertical < 0) && (playerPosScreen.x - screenEdgeHorizontal < 0f))
		//	{
		//		transform.position = Camera.main.ScreenToWorldPoint(new Vector3(screenEdgeHorizontal, screenEdgeVertical, transform.position.z - Camera.main.transform.position.z));

		//	}
		//	else if (playerPosScreen.y - screenEdgeVertical < 0)
		//	{
		//		transform.position = Camera.main.ScreenToWorldPoint(new Vector3(playerPosScreen.x, screenEdgeVertical, transform.position.z - Camera.main.transform.position.z));
		//	}
		//}

		public void LookToDir(Direction dir)
		{
			view?.LookToDir(dir);
		}

		public Direction GetCurrentDirection()
		{
			return currentDirection;
		}

		public void UpdateDirection()
		{
			int i = Mathf.Clamp(Mathf.RoundToInt(inputDirection.x.ToFloat()), -1, 1);
			currentDirection = (Direction)i;
		}

		public void Death()
		{
			isDead = true;
			PhysicSystem.Instance.RemoveCollider(attackCollider);
			PhysicSystem.Instance.RemoveCollider(hitCollider);
		}

		public bool PlayerIsGrounded()
		{
			return isGrounded;
		}

		private void ShowDustEffect()
        {
			view?.ShowDustEffect();
        }
	}
}
