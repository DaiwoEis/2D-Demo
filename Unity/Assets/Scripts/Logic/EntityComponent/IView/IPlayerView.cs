using Lockstep.Game;
using Lockstep.Math;

namespace Lockstep.Game {
    public interface IPlayerView : IEntityView  {
    }

    public interface IPlayer2DView : IEntityView
    {
        void OnMove();

        void LookToDir(Direction dir);
    }
}