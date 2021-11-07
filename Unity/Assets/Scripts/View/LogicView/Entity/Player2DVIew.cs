using Lockstep.Math;
using UnityEngine;

namespace Lockstep.Game
{
    public class Player2DView : MonoBehaviour, IEntityView, IPlayer2DView
    {
        [Header("Entity")]
        public Player2D Player;

        [Header("表现配置")]
        public SpriteRenderer GFX;
        public SpriteRenderer Shadow;
        public Animator animator;

        private AudioPlayer audioPlayer;

        public void BindEntity(BaseEntity e, BaseEntity oldEntity = null)
        {
            Player = e as Player2D;
            Player.view = this;
        }

        public void OnMove()
        {

        }

        private void Update()
        {
            var pos = Player.transform.Pos3.ToVector3();
            transform.position = Vector3.Lerp(transform.position, pos, 0.3f);
            UpdateSortingOrder();
        }

        private void UpdateSortingOrder()
        {
            GFX.sortingOrder = Mathf.RoundToInt(transform.position.y * -10f);
            Shadow.sortingOrder = GFX.sortingOrder - 1;
        }

        public void LookToDir(Direction dir)
        {
            if (dir == Direction.Left)
                GFX.transform.localScale = new Vector3(-1, 1, 1);
            else if (dir == Direction.Right)
                GFX.transform.localScale = new Vector3(1, 1, 1);
        }

        public void OnTakeDamage(int amount, LVector3 hitPoint)
        {
            
        }

        public void OnDead()
        {
            
        }

        public void OnRollbackDestroy()
        {
            
        }

        public void IdleAnim()
        {
            animator.SetTrigger("Idle");
            animator.SetBool("Walk", false);
        }

        public void WalkAnim()
        {
            animator.SetBool("Walk", true);
        }
    }
}