using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour {

	[Header("Keyboard keys")]
	public KeyCode Left = KeyCode.LeftArrow;
	public KeyCode Right = KeyCode.RightArrow;
	public KeyCode Up = KeyCode.UpArrow;
	public KeyCode Down = KeyCode.DownArrow;
	public KeyCode PunchKey = KeyCode.Z;
	public KeyCode KickKey = KeyCode.X;
	public KeyCode DefendKey = KeyCode.C;
	public KeyCode JumpKey = KeyCode.Space;

	[Header("Joypad keys")]
	public KeyCode JoypadPunch = KeyCode.JoystickButton2;
	public KeyCode JoypadKick = KeyCode.JoystickButton3;
	public KeyCode JoypadDefend = KeyCode.JoystickButton1;
	public KeyCode JoypadJump = KeyCode.JoystickButton0;

	//delegates
	public delegate void InputEventHandler(Vector2 dir);
	public static event InputEventHandler onInputEvent;
	public delegate void CombatInputEventHandler(string action);
	public static event CombatInputEventHandler onCombatInputEvent;
	private GameSettings settings;

	void OnEnable(){
		EnemyWaveSystem.onLevelStart += TouchScreenControls;
	}

	void OnDisable(){
		EnemyWaveSystem.onLevelStart -= TouchScreenControls;
	}

	public static void InputEvent(Vector2 dir){
		if( onInputEvent != null) onInputEvent(dir);
	}

	public static void CombatInputEvent(string action){
		if( onCombatInputEvent != null) onCombatInputEvent(action);
	}

	void Awake(){
		settings = Resources.Load("GameSettings", typeof(GameSettings)) as GameSettings;
	}

	void Update(){

		//Use keyboard
		if(settings != null && settings.Player1_Input == INPUTTYPE.KEYBOARD) KeyboardControls();

		//use joypad
		if(settings != null && settings.Player1_Input == INPUTTYPE.JOYPAD) JoyPadControls();

	}

	void KeyboardControls(){
		
		//movement
		float x = 0f;
	 	float y = 0f;

		if(Input.GetKey(Left)) x = -1f;
		if(Input.GetKey(Right)) x = 1f;
		if(Input.GetKey(Up)) y = 1f;
		if(Input.GetKey(Down)) y = -1f;

		Vector2 dir = new Vector2(x,y);
		InputEvent(dir);


		//Combat input
		if(Input.GetKeyDown(PunchKey)){
			CombatInputEvent("Punch");
		}

		if(Input.GetKeyDown(KickKey)){
			CombatInputEvent("Kick");
		}

		if(Input.GetKey(DefendKey)){
			CombatInputEvent("Defend");
		}

		if(Input.GetKeyDown(JumpKey)){
			CombatInputEvent("Jump");
		}
	}

	void JoyPadControls(){
	 	float x = Input.GetAxis("Horizontal");
	 	float y = Input.GetAxis("Vertical");
		Vector2 dir = new Vector2(x,y);
		InputEvent(dir.normalized);

		if(Input.GetKeyDown(JoypadPunch)){
			CombatInputEvent("Punch");
		}

		if(Input.GetKeyDown(JoypadKick)){
			CombatInputEvent("Kick");
		}

		if(Input.GetKey(JoypadDefend)){
			CombatInputEvent("Defend");
		}

		if(Input.GetKey(JoypadJump)){
			CombatInputEvent("Jump");
		}
	}

	void TouchScreenControls(){

		//creates touchscreencontrols in the UI canvas
		if(settings != null && settings.Player1_Input == INPUTTYPE.TOUCHSCREEN){

			GameObject canvas = GameObject.FindObjectOfType<Canvas>().gameObject;
			if(canvas != null) {
				GameObject TSC = GameObject.Instantiate(Resources.Load("UI_TouchScreenControls")) as GameObject;
				TSC.transform.SetParent(canvas.transform, false);
			}
		}
	}
}

public enum INPUTTYPE {	
	KEYBOARD = 0,	
	JOYPAD = 2,	
	TOUCHSCREEN = 4, 
}
