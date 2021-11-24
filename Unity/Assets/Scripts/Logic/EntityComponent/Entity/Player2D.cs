using System;
using Lockstep.Math;
using UnityEngine;

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

		[ReRefBackup] public IPlayer2DView view;

        protected override void BindRef()
        {
            base.BindRef();

			view = null;
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
				view?.PlayAnim("Punch");
            }
			else if (state == PLAYERSTATE.KICK)
			{
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
