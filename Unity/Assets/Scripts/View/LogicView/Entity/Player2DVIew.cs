using UnityEngine;

namespace Lockstep.Game
{
    public class Player2DVIew : EntityView, IPlayer2DView
    {
        public Player2D Player;

        public SpriteRenderer GFX;
        public SpriteRenderer Shadow;

        private AudioPlayer audioPlayer;


        public override void BindEntity(BaseEntity e, BaseEntity oldEntity = null)
        {
            base.BindEntity(e, oldEntity);
            Player = e as Player2D;
            Player.view = this;
        }

        public void OnMove()
        {

        }

        private void Update()
        {
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
    }
}