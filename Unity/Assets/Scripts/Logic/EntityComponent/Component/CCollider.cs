using System;
using Lockstep.Collision2D;
using Lockstep.Math;
using Lockstep.UnsafeCollision2D;

namespace Lockstep.Game
{

    [Serializable]
    public partial class CCollider : Component
    {
        public enum Type
        {
            Attack,
            Hit,
            Defend
        }

        [NoBackup] public Type type;
        public int colliderProxyId;
        [NoBackup] public int layer;
        [NoBackup] public ILPTriggerEventHandler handler;
        private LRect _bound;

        public ColliderProxy GetProxy()
        {
            return PhysicSystem.GetCollider(colliderProxyId);
        }

        public LRect GetBound()
        {
            return _bound;
        }

        public void SetBound(LRect bound)
        {
            _bound = bound;
            GetProxy().SetBound(_bound);
        }

        public void ClearBound()
        {
            _bound.center = LVector2.zero;
            _bound.size = LVector2.zero;
            GetProxy().SetBound(_bound);
        }
    }
}