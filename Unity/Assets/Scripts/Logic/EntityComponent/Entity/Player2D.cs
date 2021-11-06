using System;
using System.Collections.Generic;
using Lockstep.Math;
using UnityEngine;

namespace Lockstep.Game
{
    [Serializable]
    public partial class Player2D : BaseEntity
    {
		public int localId;
		public PlayerInput input = new PlayerInput();

		public LFloat walkSpeed = new LFloat(true, 1f);
		public LFloat jumpTime = new LFloat(true, 9f);
		public LFloat jumpHeight = new LFloat(true, 2.5f);

		public Direction currentDirection = Direction.Right;
		public LVector2 inputDirection;

		[Backup] private bool isDead = false;
		[Backup] private LFloat screenEdgeHorizontal = new LFloat(true, 80f);
		[Backup] private LFloat screenEdgeVertical = new LFloat(true, 18f);
		[Backup] private bool isGrounded;

		private List<PLAYERSTATE> MovementStates = new List<PLAYERSTATE> {
			PLAYERSTATE.IDLE,
			PLAYERSTATE.JUMPING,
			PLAYERSTATE.JUMPKICK,
			PLAYERSTATE.MOVING
		};

		public PLAYERSTATE currentState = PLAYERSTATE.IDLE;

		[ReRefBackup] public IPlayer2DView view;

        protected override void BindRef()
        {
            base.BindRef();

			view = null;
        }

        public void SetState(PLAYERSTATE state)
		{
			currentState = state;
		}

        public override void DoUpdate(LFloat deltaTime)
        {
            base.DoUpdate(deltaTime);

			inputDirection = input.inputUV;

			if (MovementStates.Contains(currentState) && !isDead)
			{
				var dir = new LVector2(inputDirection.x, inputDirection.y * new LFloat(true, 0.7f));
				Move(dir * walkSpeed * deltaTime);
			}
			else
			{
				Move(LVector2.zero);
			}

			if (input.jump && MovementStates.Contains(currentState) && !isDead)
			{
				inputDirection.y = 0;
				Move(inputDirection * walkSpeed * deltaTime);

				//if (currentState != PLAYERSTATE.JUMPING) 
				//	StartCoroutine(doJump());
			}
		}


		//jump sequence
		//IEnumerator doJump()
		//{
		//	float t = 0;
		//	Vector3 startPos = GFX.transform.localPosition;
		//	Vector3 endPos = new Vector3(startPos.x, startPos.y + jumpHeight, startPos.z);

		//	playerState.SetState(PLAYERSTATE.JUMPING);
		//	isGrounded = false;
		//	animator.Jump();

		//	//adjust the jump animation speed so it fits with the height and time parameters
		//	GFX.GetComponent<Animator>().SetFloat("AnimationSpeed", 1f / jumpTime);

		//	//going up
		//	while (t < 1)
		//	{
		//		GFX.transform.localPosition = Vector3.Lerp(startPos, endPos, MathUtilities.Sinerp(0, 1, t));
		//		t += Time.deltaTime / (jumpTime / 2);
		//		yield return null;
		//	}

		//	//going down
		//	while (t > 0)
		//	{
		//		GFX.transform.localPosition = Vector3.Lerp(startPos, endPos, MathUtilities.Sinerp(0, 1, t));
		//		t -= Time.deltaTime / (jumpTime / 2);
		//		yield return null;
		//	}

		//	GFX.transform.localPosition = startPos;

		//	//show dust particles
		//	animator.ShowDustEffect();
		//	isGrounded = true;
		//}

		private void Move(LVector2 vector)
		{
			if (currentState != PLAYERSTATE.JUMPING && currentState != PLAYERSTATE.JUMPKICK)
			{
				transform.pos += vector;

				if (inputDirection.magnitude > 0)
				{
					UpdateDirection();
					WalkAnim();
				}
				else
				{
					IdleAnim();
				}

				LookToDir(currentDirection);
				isGrounded = true;
				view?.OnMove();
			}
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

		public void Idle()
		{
			if (currentState != PLAYERSTATE.JUMPING)
			{
				SetState(PLAYERSTATE.IDLE);
				transform.pos = LVector2.zero;
				IdleAnim();
			}
		}

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
			LookToDir(currentDirection);
		}

		public void Death()
		{
			isDead = true;
		}

		public bool PlayerIsGrounded()
		{
			return isGrounded;
		}

		private void IdleAnim()
        {

        }

		private void WalkAnim()
        {

        }
	}
}
