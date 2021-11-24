using System.Collections.Generic;

using NetMsg.Common;

namespace Lockstep.Game {
    public class PureInputService : PureBaseService, IInputService {
        List<InputCmd> cmds = new List<InputCmd>();
        public void Execute(InputCmd cmd, object entity){ }
        public List<InputCmd> FetchInputCmds(){
            return cmds;
        }
        public List<InputCmd> GetDebugInputCmds(){
            return cmds;
        }
        public void Reset(){

        }
    }
}