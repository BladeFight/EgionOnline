using UnityEngine;
using System.Collections;

public class MobController3D : AtavismMobController {
	
	#region Animation Fields
	
	public AnimationClip idleAnimation;
	public AnimationClip walkAnimation;
	public AnimationClip runAnimation;
	public AnimationClip jumpPoseAnimation;
	public AnimationClip combatIdleAnimation;
	public AnimationClip unarmedAttackedAnimation;
	public AnimationClip deathAnimation;
	public float walkMaxAnimationSpeed = 0.75f;
	public float trotMaxAnimationSpeed = 1.0f;
	public float runMaxAnimationSpeed = 1.0f;
	public float jumpAnimationSpeed = 1.15f;
	public float landAnimationSpeed = 1.0f;
	private Animation _animation;
	private Animator _animator;
	private bool useAnimator;
	
	enum CharacterState
	{
		Idle = 0,
		Walking = 1,
		Trotting = 2,
		Running = 3,
		Jumping = 4,
	}
	
	private CharacterState _characterState;
	bool dead = false;
	bool inCombat = false;
	AnimationClip overrideAnimation;
	string overrideAnimationName;
	float overrideAnimationExpires;
	
	#endregion Animation Fields

	#region Movement Fields
	// Is Walk on
	bool walk = false;
	// The speed when walking
	float walkSpeed = 2.0f;
	float inAirControlAcceleration = 3.0f;
	public float runThreshold = 2.5f;
	
	// How high do we jump when pressing jump and letting go immediately
	public float jumpHeight = 1.5f;
	
	// The gravity for the character
	float gravity = 20.0f;
	// The gravity in controlled descent mode
	float speedSmoothing = 10.0f;
	float rotateSpeed = 5.0f; // was 250
	float trotAfterSeconds = 3.0f;
	bool canJump = true;
	private float jumpRepeatTime = 0.05f;
	private float jumpTimeout = 0.15f;
	private float groundedTimeout = 0.25f;
	
	// The camera doesnt start following the target immediately but waits for a split second to avoid too much waving around.
	private float lockCameraTimer = 0.0f;
	
	// The current move direction in x-z
	private Vector3 moveDirection = Vector3.zero;
	// The current vertical speed
	private float verticalSpeed = 0.0f;
	// The current x-z move speed
	private float moveSpeed = 0.0f;
	Vector3 direction = Vector3.zero;
	Quaternion rotation = Quaternion.identity;
	
	// The last collision flags returned from controller.Move
	private CollisionFlags collisionFlags; 
	
	// Are we moving backwards (This locks the camera to not do a 180 degree spin)
	private bool movingBack = false;
	// Is the user pressing any keys?
	private bool isMoving = false;
	// When did the user start walking (Used for going into trot after a while)
	private float walkTimeStart = 0.0f;
	// Last time we performed a jump
	private float lastJumpTime = -1.0f;
	
	// the height we jumped from (Used to determine for how long to apply extra jump power after jumping.)
	private float lastJumpStartHeight = 0.0f;
	private Vector3 inAirVelocity = Vector3.zero;
	private float lastGroundedTime = 0.0f;
	private bool isControllable = true;
	private bool grounded;
	public float groundedCheckOffset = 0.2f;
	private const float groundedDistance = 0.5f;
	private const float groundDrag = 5.0f;
	public LayerMask groundLayers = 0;
	private bool mouseOverUI = false;
	bool leftButtonDown = false;
	bool rightButtonDown = false;
	bool mouseRun = false;

	protected float fallingSpeed = 0;
	protected bool isFalling = false;
	#endregion Movement Fields
	
	#region UI Fields
	public GUISkin customSkin = null;
	public string styleName = "Box";
	public float fadeDistance = 30.0f, hideDistance = 35.0f;
	public float maxViewAngle = 90.0f;
	public float nameHeight = 2.2f;
	string combatText = "";
	float combatTextExpiration;
	#endregion UI Fields
	
	// Use this for initialization
	void Start ()
	{
		useAnimator = false;
		_animation = (Animation)GetComponent ("Animation");
		if (!_animation) {
			_animator = (Animator)GetComponentInChildren (typeof(Animator));
			useAnimator = true;
			if (!_animator)
				Debug.Log ("The character you would like to control doesn't have animations. Moving her might look weird.");
			else
				Debug.Log("Got animator: " + _animator);
		}
		
		if (!idleAnimation) {
			_animation = null;
			Debug.Log ("No idle animation found. Turning off animations.");
		}
		if (!walkAnimation) {
			_animation = null;
			Debug.Log ("No walk animation found. Turning off animations.");
		}
		if (!runAnimation) {
			_animation = null;
			Debug.Log ("No run animation found. Turning off animations.");
		}
		if (!jumpPoseAnimation && canJump) {
			_animation = null;
			Debug.Log ("No jump animation found and the character has canJump enabled. Turning off animations.");
		}
		
		moveSpeed = runSpeed;
		
		// Register for dead message
		Client.Instance.WorldManager.RegisterObjectPropertyChangeHandler ("deadstate", HandleDeadState);
		Client.Instance.WorldManager.RegisterObjectPropertyChangeHandler("combatstate", HandleCombatState);
		
		// Get this mob to ignore collisions with other mobs/players
		foreach (GameObject go in GameObject.FindGameObjectsWithTag(Client.Instance.mobTag)) {
			if (go != gameObject)
				Physics.IgnoreCollision (collider, go.collider);
		}
		foreach (GameObject go in GameObject.FindGameObjectsWithTag(Client.Instance.playerTag)) {
			if (go != gameObject)
				Physics.IgnoreCollision (collider, go.collider);
		}
	}
	
	void ObjectNodeReady ()
	{
		this.oid = GetComponent<AtavismNode> ().Oid;
		GetComponent<AtavismNode>().SetMobController(this);
		if (GetComponent<AtavismNode>().PropertyExists("deadstate")) {
			dead = (bool)GetComponent<AtavismNode>().GetProperty("deadstate");
		}
		//GetComponent<AtavismNode>().RegisterObjectPropertyChangeHandler("currentAction", null);
		transform.position = new Vector3(transform.position.x, transform.position.y + 100, transform.position.x);
	}
	
	// Update is called once per frame
	void Update ()
	{
		Vector3 movement = Vector3.zero;
		if (isPlayer) {
			if (!dead) {
				movement = MovePlayer ();
			} else {
				// Still need to get player movement even when dead
				AtavismInputController inputManager = ClientAPI.GetInputController();
				inputManager.GetPlayerMovement ();
				ApplyGravity ();
				movement = new Vector3(0, verticalSpeed * Time.deltaTime, 0) + inAirVelocity;
			}
		} else if (!dead) {
			movement = MoveMob ();
		} else {
			ApplyGravity ();
			movement = new Vector3(0, verticalSpeed * Time.deltaTime, 0) + inAirVelocity;
		}
		// Move the controller
		CharacterController controller = (CharacterController)GetComponent ("CharacterController");
		collisionFlags = controller.Move (movement);
		
		// Update facing if needed
		if (!isPlayer && target != -1 && !dead) {
			ObjectNode targetNode = Client.Instance.WorldManager.GetObjectNode (target);
			if (targetNode != null)
				gameObject.transform.LookAt (targetNode.GameObject.transform);
		}
		
		// ANIMATION sector
		if (_animation && !dead) {
			//Debug.Log("Using animation for mob: " + name);
			if (jumping) {
				if (!jumpingReachedApex) {
					_animation [jumpPoseAnimation.name].speed = jumpAnimationSpeed;
					_animation [jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
					_animation.CrossFade (jumpPoseAnimation.name);
				} else {
					_animation [jumpPoseAnimation.name].speed = -landAnimationSpeed;
					_animation [jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
					_animation.CrossFade (jumpPoseAnimation.name);				
				}
			} else {
				if (controller.velocity.sqrMagnitude > 0.1) {
					if (controller.velocity.magnitude > runThreshold) {
						_animation [runAnimation.name].speed = Mathf.Clamp (controller.velocity.magnitude, 0.0f, runMaxAnimationSpeed);
						_animation.CrossFade (runAnimation.name);	
					} else {
						_animation [walkAnimation.name].speed = Mathf.Clamp (controller.velocity.magnitude, 0.0f, walkMaxAnimationSpeed);
						_animation.CrossFade (walkAnimation.name);	
					}
				} else if (overrideAnimation != null) {
					//_animation [overrideAnimation.name].speed = Mathf.Clamp (controller.velocity.magnitude, 0.0f, runMaxAnimationSpeed);
					_animation.CrossFade (overrideAnimation.name);
					if (Time.time > overrideAnimationExpires) {
						overrideAnimation = null;
					}
				} else {
					_animation.CrossFade (idleAnimation.name);
				}
			}
		} else if (useAnimator) {
			if (_animator == null)
			{
				_animator = (Animator)GetComponentInChildren(typeof(Animator));
			}
			// Debug.Log("Using animator for mob: " + name);
			if (_animator && dead)
			{
				_animator.SetBool ("Dead", true);
				_animator.SetBool ("SpecialAttack2", false);
				_animator.SetBool ("Attack", false);
			} else if (_animator) {
				_animator.SetBool ("Dead", false);
				_animator.SetFloat ("Speed", controller.velocity.magnitude);
				if (jumping) {
					_animator.SetBool ("Jump", true);
				} else {
					_animator.SetBool ("Jump", false);
				}
				if (overrideAnimationName == "waving") {
					if (Time.time > overrideAnimationExpires) {
						_animator.SetBool ("Waving", false);
						overrideAnimationName = "";
					} else {
						_animator.SetBool ("Waving", true);
					}
				} else if (overrideAnimationName == "mining") {
					if (Time.time > overrideAnimationExpires) {
						_animator.SetBool ("Mining", false);
						overrideAnimationName = "";
					} else {
						_animator.SetBool ("Mining", true);
					}
				} else if (overrideAnimationName == "attack_normal") {
					if (Time.time > overrideAnimationExpires) {
						_animator.SetBool ("Attack", false);
						overrideAnimationName = "";
					} else {
						_animator.SetBool ("Wound", false);
						_animator.SetBool ("SpecialAttack2", false);
						_animator.SetBool ("Attack", true);
					}
				} else if (overrideAnimationName == "attack_special") {
					if (Time.time > overrideAnimationExpires) {
						_animator.SetBool ("SpecialAttack", false);
						overrideAnimationName = "";
					} else {
						_animator.SetBool ("SpecialAttack", true);
					}
				} else if (overrideAnimationName == "attack_special2") {
					if (Time.time > overrideAnimationExpires) {
						_animator.SetBool ("SpecialAttack2", false);
						overrideAnimationName = "";
					} else {
						_animator.SetBool ("Wound", false);
						_animator.SetBool ("Attack", false);
						_animator.SetBool ("SpecialAttack2", true);
					}
				} else if (overrideAnimationName == "wound") {
					if (Time.time > overrideAnimationExpires) {
						_animator.SetBool ("Wound", false);
						overrideAnimationName = "";
					} else {
						_animator.SetBool ("Wound", true);
					}
				} else if (overrideAnimationName != null && overrideAnimationName != "") {
					if (Time.time > overrideAnimationExpires) {
						_animator.SetBool (overrideAnimationName, false);
						overrideAnimationName = "";
					} else {
						_animator.SetBool (overrideAnimationName, true);
					}
				}
			}
		}
		// ANIMATION sector
		
		// Set rotation to the move direction
		/*if (IsGrounded ()) {
			transform.rotation = rotation;
			//transform.rotation = Quaternion.LookRotation (moveDirection);
			
		} else {
			var xzMove = movement;
			xzMove.y = 0;
			if (xzMove.sqrMagnitude > 0.001) {
				transform.rotation = Quaternion.LookRotation (xzMove);
			}
		}*/	
		
		// We are in jump mode but just became grounded
		if (IsGrounded ()) {
			lastGroundedTime = Time.time;
			inAirVelocity = Vector3.zero;
			if (jumping) {
				jumping = false;
				SendMessage ("DidLand", SendMessageOptions.DontRequireReceiver);
			}
		}
		
		// Update camera if it is the player
		if (isPlayer) {
			AtavismInputController inputManager = ClientAPI.GetInputController();
			inputManager.RunCameraUpdate();
		}
	}
	
	public override Vector3 MoveMob ()
	{
		float timeDifference = (Time.time - lastLocTimestamp);
		if (pathInterpolator != null) {
			PathLocAndDir locAndDir = pathInterpolator.Interpolate (Time.time);
			float interpolateSpeed = pathInterpolator.Speed;
			//UnityEngine.Debug.Log("MobNode.ComputePosition: oid " + oid + ", followTerrain " + followTerrain 
			//	+ ", pathInterpolator ");// + (locAndDir == null) ? "null" : locAndDir.ToString ());
			if (locAndDir != null) {
				/*if (locAndDir.LengthLeft > 0.25f) {
					transform.forward = locAndDir.Direction;
					//transform.rotation = Quaternion.LookRotation(locAndDir.Direction);
					//transform.rotation = Quaternion.LookRotation(LastDirection.normalized);
					//UnityEngine.Debug.Log("Set rotation to: " + transform.rotation);
				}*/
				lastDirection = locAndDir.Direction;
				lastDirTimestamp = Time.time;
				lastLocTimestamp = Time.time;
				Vector3 loc = locAndDir.Location;
				if (MobNode.useMoveMobNodeForPathInterpolator) {
					Vector3 diff = loc - transform.position;
					diff.y = 0;
					//desiredDisplacement = diff * timeDifference;
					if (diff.magnitude > 1)
						diff = diff.normalized;
					desiredDisplacement = diff * interpolateSpeed * timeDifference;
					//UnityEngine.Debug.Log("displacement: " + desiredDisplacement + " with loc: " + loc + " and current position: " + transform.position);
					if (desiredDisplacement != Vector3.zero) {
						//transform.forward = locAndDir.Direction;
						transform.rotation = Quaternion.LookRotation (desiredDisplacement);
					}
				} else {
					desiredDisplacement = Vector3.zero;
				}
			} else {
				// This interpolator has expired, so get rid of it
				pathInterpolator = null;
				lastDirection = Vector3.zero;
				desiredDisplacement = Vector3.zero;
				//UnityEngine.Debug.Log("Path interpolator for mob: " + oid + " has expired");
			}
		} else {
			lastLocTimestamp = Time.time;
			Vector3 pos = transform.position + (Time.deltaTime * lastDirection);
			desiredDisplacement = pos - transform.position;
		}
		
		// Apply gravity
		// - extra power jump modifies gravity
		// - controlledDescent mode modifies gravity
		//if (!grounded) {
		ApplyGravity ();
		//UnityEngine.Debug.Log("Mob grounded? false");
		//} else {
		//	verticalSpeed = 0.0f;
		//}
		
		// Apply jumping logic
		ApplyJumping ();
		
		// Calculate actual motion
		float speed = moveSpeed;
		if (movingBack)
			speed = walkSpeed;
		// Multiply inAirVelocity by delta time as we don't multiply the whole movement
		inAirVelocity *= Time.deltaTime;
		//if (verticalSpeed > 0)
		//	UnityEngine.Debug.Log("Vertical speed: " + verticalSpeed);
		Vector3 movement = desiredDisplacement + new Vector3 (0, verticalSpeed * Time.deltaTime, 0) + inAirVelocity;
		//UnityEngine.Debug.Log("Moving mob: " + this.name + " by: " + movement);
		//movement *= Time.deltaTime;
		
		if (lastDirection != Vector3.zero) {
			if (desiredDisplacement.magnitude < runThreshold)
				_characterState = CharacterState.Walking;
			else
				_characterState = CharacterState.Running;
		} else {
			_characterState = CharacterState.Idle;
		}
		
		return movement;
	}
	
	public override Vector3 MovePlayer ()
	{
		if (Input.GetButtonDown ("Jump")) {
			lastJumpButtonTime = Time.time;
			Client.Instance.WorldManager.SendJumpStarted ();
		}
		AtavismInputController inputManager = ClientAPI.GetInputController();
		Vector3 direction = inputManager.GetPlayerMovement ();
		
		// Apply gravity
		// - extra power jump modifies gravity
		// - controlledDescent mode modifies gravity
		ApplyGravity ();
		
		// Apply jumping logic
		ApplyJumping ();
		
		// Calculate actual motion
		direction.Normalize ();
		float speed = runSpeed;
		if (walk || movingBack)
			speed = walkSpeed;
		//Vector3 displacement = (transform.rotation * direction) * speed;
		Vector3 displacement = (direction * speed);
		Vector3 movement = displacement + new Vector3 (0, verticalSpeed, 0) + inAirVelocity;
		movement *= Time.deltaTime;
		
		// Update player direction - Used for MMO
		Client.Instance.WorldManager.Player.SetDirection (displacement, transform.position, Time.time);
		Client.Instance.WorldManager.Player.Orientation = transform.rotation;
		
		if (direction != Vector3.zero) {
			if (walk)
				_characterState = CharacterState.Walking;
			else
				_characterState = CharacterState.Running;
		} else {
			_characterState = CharacterState.Idle;
		}
		
		return movement;
	}
	
	void OnGUI ()
	{
		if (this.oid == 0)
			return;
		Vector3 worldPosition = new Vector3 (collider.bounds.center.x, collider.bounds.min.y, collider.bounds.center.z);
		worldPosition += Vector3.up * nameHeight;
		float cameraDistance = (worldPosition - Camera.main.transform.position).magnitude;
		
		// If the world position is outside of the field of view or further away than hideDistance, don't render the label
		if (
			cameraDistance > hideDistance ||
			Vector3.Angle (
			Camera.main.transform.forward,
			worldPosition - Camera.main.transform.position
			) >
			maxViewAngle
			) {
			return;
		}
		
		// If the distance to the label position is greater than the fade distance, apply the needed fade to the label
		if (cameraDistance > fadeDistance) {
			GUI.color = new Color (1.0f, 1.0f, 1.0f, 1.0f - (cameraDistance - fadeDistance) / (hideDistance - fadeDistance));
		}
		
		Vector2 position = Camera.main.WorldToScreenPoint (worldPosition);
		position = new Vector2 (position.x, Screen.height - position.y);
		// Get the GUI space position
		
		GUI.skin = customSkin;
		// Set the custom skin. If no custom skin is set (null), Unity will use the default skin
		
		Vector2 size = GUI.skin.GetStyle (styleName).CalcSize (new GUIContent (gameObject.name));
		// Get the content size with the selected style
		
		Rect rect = new Rect (position.x - size.x * 0.5f, position.y - size.y, size.x, size.y);
		// Construct a rect based on the calculated position and size
		if (!isPlayer)
			GUI.Label (rect, gameObject.name);
		
		// Draw combat text if any
		if (combatText == "")
			return;
		if (Time.time > combatTextExpiration) {
			combatText = "";
			return;
		}
		// Move up another 40cm
		worldPosition += Vector3.up * 0.4f;
		position = Camera.main.WorldToScreenPoint (worldPosition);
		position = new Vector2 (position.x, Screen.height - position.y);
		size = GUI.skin.GetStyle (styleName).CalcSize (new GUIContent (combatText));
		rect = new Rect (position.x - size.x * 0.5f, position.y - size.y, size.x, size.y);
		// Construct a rect based on the calculated position and size
		GUI.Label (rect, combatText);
	}
	
	#region Movement
	
	public void ApplyJumping ()
	{
		// Prevent jumping too fast after each other
		if (lastJumpTime + jumpRepeatTime > Time.time) {
			return;
		}
		
		if (IsGrounded ()) {
			// Jump
			// - Only when pressing the button down
			// - With a timeout so you can press the button slightly before landing		
			if (canJump && Time.time < lastJumpButtonTime + jumpTimeout) {
				UnityEngine.Debug.Log ("Applying jump for mob: " + name);
				verticalSpeed = CalculateJumpVerticalSpeed (jumpHeight);
				SendMessage ("DidJump", SendMessageOptions.DontRequireReceiver);
				jumping = true;
			}
		}
	}
	
	public void ApplyGravity ()
	{
		if (isControllable) {	// don't move player at all if not controllable.
			// Apply gravity
			var jumpButton = Input.GetButton ("Jump");
			
			
			// When we reach the apex of the jump we send out a message
			if (jumping && !jumpingReachedApex && verticalSpeed <= 0.0) {
				jumpingReachedApex = true;
				SendMessage ("DidJumpReachApex", SendMessageOptions.DontRequireReceiver);
			}
			
			if (IsGrounded ())
				verticalSpeed = 0.0f;
			else
				verticalSpeed -= gravity * Time.deltaTime;
		}
	}
	
	public float CalculateJumpVerticalSpeed (float targetJumpHeight)
	{
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt (2 * targetJumpHeight * gravity);
	}
	
	public void OnControllerColliderHit (ControllerColliderHit hit)
	{
		//	Debug.DrawRay(hit.point, hit.normal);
		if (hit.moveDirection.y > 0.01) 
			return;
	}
	
	public float GetSpeed ()
	{
		return moveSpeed;
	}
	
	public Vector3 GetDirection ()
	{
		return moveDirection;
	}
	
	public bool IsMovingBackwards ()
	{
		return movingBack;
	}
	
	public bool IsMoving ()
	{
		return Mathf.Abs (Input.GetAxisRaw ("Vertical")) + Mathf.Abs (Input.GetAxisRaw ("Horizontal")) > 0.5;
	}
	
	public bool IsJumping ()
	{
		return jumping;
	}
	
	public bool IsGrounded ()
	{
		return (collisionFlags & CollisionFlags.CollidedBelow) != 0;
	}
	
	public bool HasJumpReachedApex ()
	{
		return jumpingReachedApex;
	}
	
	public bool IsGroundedWithTimeout ()
	{
		return lastGroundedTime + groundedTimeout > Time.time;
	}
	
	#endregion Movement
	
	#region Other Functions
	
	public override void GotDamageMessage (int messageType, string damageAmount)
	{
		Debug.Log ("Got damage message for " + name + " with amount: " + damageAmount);
		combatText = damageAmount;
		combatTextExpiration = Time.time + 1.5f;
	}
	
	public override void PlayMeleeAttackAnimation (string attackType, string result)
	{
		if (attackType == "normal") {
			overrideAnimationName = "attack_normal";
		} else if (attackType == "special") {
			overrideAnimationName = "attack_special";
		} else if (attackType == "special2") {
			overrideAnimationName = "attack_special2";
		}
		overrideAnimation = unarmedAttackedAnimation;
		overrideAnimationExpires = Time.time + 1.0f; //overrideAnimation.length;
	}
	
	public override void PlayMeleeRecoilAnimation (string result)
	{
		overrideAnimationName = "wound";
		overrideAnimationExpires = Time.time + 0.5f;
	}
	
	public void Wave (bool wave)
	{
		overrideAnimationName = "waving";
		overrideAnimationExpires = Time.time + 2;
	}
	
	#endregion Other Functions
	
	#region Property Handlers
	public void HandleDeadState (object sender, ObjectPropertyChangeEventArgs args)
	{
		Debug.Log ("Got dead update: " + oid);
		if (args.Oid != oid)
			return;
		dead = (bool)Client.Instance.WorldManager.GetObjectNode(oid).GetProperty("deadstate");
		if (dead) {
			// Play death animation
			if (_animation) {
				_animation.CrossFade (deathAnimation.name);
			} else if (_animator) {
				_animator.SetBool ("Dead", true);
			}
			target = -1;
		}
		Debug.Log ("Set dead state to: " + dead);
	}
	
	public void HandleCombatState (object sender, ObjectPropertyChangeEventArgs args)
	{
		if (args.Oid != oid)
			return;
		inCombat = (bool)Client.Instance.WorldManager.GetObjectNode(oid).GetProperty("combatstate");
		if (_animator != null) {
			Debug.Log("setting combat state for animator");
			_animator.SetBool ("Combat", inCombat);
		}
	}
	
	public override void PlayAnimation(string animationName, float length) {
		if (_animator != null && overrideAnimationName != null && overrideAnimationName != "") {
			Debug.Log("clearing old animation");
			_animator.SetBool (overrideAnimationName, false);
		}
		overrideAnimationName = animationName;
		overrideAnimationExpires = Time.time + length;
		
		if (animation) {
			if (animationName == "attack_normal") {
				overrideAnimation = unarmedAttackedAnimation;
			}
		}
	}
	#endregion Property Handlers
	
	#region Properties
	public Quaternion Rotation {
		get {
			return rotation;
		}
		set {
			rotation = value;
		}
	}
	
	public bool Walking {
		get {
			return walk;
		}
		set {
			walk = value;
		}
	}
	
	public float MobYaw {
		get {
			float yaw;
			yaw = transform.rotation.eulerAngles.y;
			return yaw;
		}
		set {
			Camera camera = Camera.main;
			Vector3 pitchYawRoll = transform.eulerAngles;
			pitchYawRoll.y = value;
			transform.eulerAngles = pitchYawRoll;
		}
	}
	#endregion Properties
}
