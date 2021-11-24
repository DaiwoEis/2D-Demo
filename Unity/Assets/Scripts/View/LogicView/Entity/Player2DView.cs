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
            var pos = new Vector3(Player.transform.pos.x.ToFloat(), Player.transform.pos.y.ToFloat() + Player.transform.y.ToFloat());
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

        public void ShowDustEffect()
        {
            Instantiate(Resources.Load("SmokePuffEffect"), transform.position, Quaternion.identity);
        }

        public void PlayAnim(string animName)
        {
            animator.Play(animName);
        }
    }
}