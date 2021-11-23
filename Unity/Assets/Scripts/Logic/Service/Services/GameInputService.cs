using System.Collections.Generic;
using Lockstep.Game;
using Lockstep.Logging;
using Lockstep.Math;
using Lockstep.Serialization;
using Lockstep.Util;
using NetMsg.Common;

namespace Lockstep.Game {
    public class GameInputService : IInputService {
        public static PlayerInput CurGameInput = new PlayerInput();

        public void Execute(InputCmd cmd, object entity){
            var input = new Deserializer(cmd.content).Parse<PlayerInput>();
            var playerInput = entity as PlayerInput;
            playerInput.mousePos = input.mousePos;
            playerInput.inputUV = input.inputUV;
            playerInput.isInputFire = input.isInputFire;
            playerInput.skillId = input.skillId;
            playerInput.isSpeedUp = input.isSpeedUp;
            playerInput.punch = input.punch;
            playerInput.kick = input.kick;
            playerInput.defend = input.defend;
            playerInput.jump = input.jump;
            //Debug.Log("InputUV  " + input.inputUV);
        }

        public List<InputCmd> GetInputCmds(){
            if (CurGameInput.Equals(PlayerInput.Empty)) {
                return null;
            }
            var ret = new List<InputCmd>() {
                new InputCmd() { content = CurGameInput.ToBytes() }
            };
            CurGameInput.Reset();
            return ret;
        }

        public List<InputCmd> GetDebugInputCmds(){
            return new List<InputCmd>() {
                new InputCmd() {
                    content = new PlayerInput() {
                        inputUV = new LVector2(LRandom.Range(-1,2),LRandom.Range(-1,2)),
                        skillId = LRandom.Range(0,3)
                    }.ToBytes()
                }
            };
        }
    }
}