
using Lockstep.Math;
using Lockstep.Serialization;

namespace Lockstep.Game {
    public partial class PlayerInput : BaseFormater,IComponent {
        public LVector2 mousePos;
        public LVector2 inputUV;
        public bool isInputFire;
        public int skillId;
        public bool isSpeedUp;

        public bool punch;
        public bool kick;
        public bool defend;
        public bool jump;

        public override void Serialize(Serializer writer){
            writer.Write(mousePos);
            writer.Write(inputUV);
            writer.Write(isInputFire);
            writer.Write(skillId);
            writer.Write(isSpeedUp);
            writer.Write(punch);
            writer.Write(kick);
            writer.Write(defend);
            writer.Write(jump);
        }

        public void Reset(){
            mousePos = LVector2.zero;
            inputUV = LVector2.zero;
            isInputFire = false;
            skillId = 0;
            isSpeedUp = false;
            punch = false;
            kick = false;
            defend = false;
            jump = false;
        }

        public void Fetch()
        {
            punch = false;
            kick = false;
            jump = false;
        }

        public override void Deserialize(Deserializer reader){
            mousePos = reader.ReadLVector2();
            inputUV = reader.ReadLVector2();
            isInputFire = reader.ReadBoolean();
            skillId = reader.ReadInt32();
            isSpeedUp = reader.ReadBoolean();
            punch = reader.ReadBoolean();
            kick = reader.ReadBoolean();
            defend = reader.ReadBoolean();
            jump = reader.ReadBoolean();
        }

        public static PlayerInput Empty = new PlayerInput();
        public override bool Equals(object obj){
            if (ReferenceEquals(this,obj)) return true;
            var other = obj as PlayerInput;
            return Equals(other);
        }

        public bool Equals(PlayerInput other){
            if (other == null) return false;
            if (mousePos != other.mousePos) return false;
            if (inputUV != other.inputUV) return false;
            if (isInputFire != other.isInputFire) return false;
            if (skillId != other.skillId) return false;
            if (isSpeedUp != other.isSpeedUp) return false;
            if (punch != other.punch) return false;
            if (kick != other.kick) return false;
            if (defend != other.defend) return false;
            if (jump != other.jump) return false;
            return true;
        }

        public PlayerInput Clone(){
            var tThis = this;
            return new PlayerInput() {
                mousePos = tThis.mousePos,
                inputUV = tThis.inputUV,
                isInputFire = tThis.isInputFire,
                skillId = tThis.skillId,
                isSpeedUp = tThis.isSpeedUp,
                punch = tThis.punch,
                kick = tThis.kick,
                jump = tThis.jump,
                defend = tThis.defend,
            };
        }
    }
}