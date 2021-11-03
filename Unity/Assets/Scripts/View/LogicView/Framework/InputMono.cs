using Lockstep.Collision2D;
using Lockstep.Game;
using Lockstep.Math;
using UnityEngine;
using Debug = Lockstep.Logging.Debug;

namespace Lockstep.Game {
    public class InputMono : UnityEngine.MonoBehaviour {
        private static bool IsReplay => Launcher.Instance?.IsVideoMode ?? false;
        [HideInInspector] public int floorMask;
        public float camRayLength = 100;

        public bool hasHitFloor;
        public LVector2 mousePos;
        public LVector2 inputUV;
        public bool isInputFire;
        public int skillId;
        public bool isSpeedUp;

        public bool punch;
        public bool kick;
        public bool defend;
        public bool jump;
        
        [Header("Keyboard keys")]
        public KeyCode Left = KeyCode.LeftArrow;
        public KeyCode Right = KeyCode.RightArrow;
        public KeyCode Up = KeyCode.UpArrow;
        public KeyCode Down = KeyCode.DownArrow;
        public KeyCode PunchKey = KeyCode.Z;
        public KeyCode KickKey = KeyCode.X;
        public KeyCode DefendKey = KeyCode.C;
        public KeyCode JumpKey = KeyCode.Space;

        void Start(){
            floorMask = LayerMask.GetMask("Floor");
        }

        public void Update(){
            if (World.Instance != null && !IsReplay) {
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");
                inputUV = new LVector2(h.ToLFloat(), v.ToLFloat());

                isInputFire = Input.GetButton("Fire1");
                hasHitFloor = Input.GetMouseButtonDown(1);
                if (hasHitFloor) {
                    Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit floorHit;
                    if (Physics.Raycast(camRay, out floorHit, camRayLength, floorMask)) {
                        mousePos = floorHit.point.ToLVector2XZ();
                    }
                }

                skillId = 0;
                for (int i = 0; i < 6; i++) {
                    if (Input.GetKey(KeyCode.Keypad1 + i)) {
                        skillId = i+1;
                    }
                }

                isSpeedUp = Input.GetKeyDown(KeyCode.Space);
                
                if (Input.GetKey(Left)) inputUV.x = -1.ToLFloat();
                if (Input.GetKey(Right)) inputUV.x = 1.ToLFloat();
                if (Input.GetKey(Up)) inputUV.y = 1.ToLFloat();
                if (Input.GetKey(Down)) inputUV.y = -1.ToLFloat();

                punch = Input.GetKeyDown(PunchKey);
                kick = Input.GetKeyDown(KickKey);
                defend = Input.GetKeyDown(DefendKey);
                jump = Input.GetKeyDown(JumpKey);
                
                GameInputService.CurGameInput = new PlayerInput() {
                    mousePos = mousePos,
                    inputUV = inputUV,
                    isInputFire = isInputFire,
                    skillId = skillId,
                    isSpeedUp = isSpeedUp,
                    punch = punch,
                    kick = kick,
                    defend = defend,
                    jump = jump
                };
            }
        }
    }
}