using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	GameObject gameControl;

	[Header ("Ativar Habilidades")]
	[Tooltip("Ativa o duplo pulo")]
	public bool allowDoubleJump = true;
	[Tooltip("Ativa o pulo na parede")]
	public bool allowWallSliding = true;
	[Tooltip("Ativa o dash")]
	public bool allowDash = true;
	[Tooltip("Ativa o dash no ar")]
	public bool alowDashInAir = true;
	[Tooltip("Ativa o jetpack")]
	public bool allowJetpack = false;
	[Tooltip("Ativa Arma de fogo")]
	public bool allowShooter = true;

	[Header ("Arma de fogo")]

	public GameObject bullet;
	public float delayShoot = 0.3f;
	float lastShootTime;
	bool fired = false;


	[Header ("Configurações")]
	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;
	float moveSpeed = 6;
	
	[Header ("Deslizar na parede")]
	[Tooltip("Subida")]
	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	[Tooltip("salto")]
	public Vector2 wallLeap;
	
	[Tooltip("Velocidade maxima de deslizar")]
	public float wallSlideSpeedMax = 1;
	public float wallStickTime = .25f;
	float timeToWallUnstick;
	
	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	Vector3 velocity;
	float velocityXSmoothing;

	bool useDoubleJump = false;

	[Header ("Dash")]
	public float timeToDash = .25f;
	float timeToDashInUse;
	public float dashForce = 3f;
	float dashDirection = 0;
	float lastTimeClickDash = 0;
	float catchTimeClickDash = 0.2f;
	public float delayToUseDashAgain = 1f;
	float delayToUseDashAgainInUse = 0;

	[Header ("JetPack")]
	public float jetPackMoveSpeed = 5;
	
	Controller2D controller;
	
	void Start() {
		gameControl = GameObject.FindGameObjectWithTag("GameController");

		controller = GetComponent<Controller2D> ();

		// defino a gravidade baseado no tamanho do pulo que quero e no tempo de queda
		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);
	}
	
	void Update() {
		flip ();
		shoot ();

		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		int wallDirX = (controller.collisions.left) ? -1 : 1;
		
		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

		// caso eu não estiver de jetpack
		if (! allowJetpack) {
			bool wallSliding = false;
			if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
				wallSliding = true;
				
				if (velocity.y < -wallSlideSpeedMax) {
					velocity.y = -wallSlideSpeedMax;
				}
				
				if (timeToWallUnstick > 0) {
					velocityXSmoothing = 0;
					velocity.x = 0;
					
					if (input.x != wallDirX && input.x != 0) {
						timeToWallUnstick -= Time.deltaTime;
					} else {
						timeToWallUnstick = wallStickTime;
					}
				} else {
					timeToWallUnstick = wallStickTime;
				}
				
			}
			
			if (Input.GetKeyDown (KeyCode.UpArrow)) {
				// pulo na parede
				if (wallSliding && allowWallSliding) {
					if (wallDirX == input.x) {
						velocity.x = -wallDirX * wallJumpClimb.x;
						velocity.y = wallJumpClimb.y;
					} else if (input.x == 0) {
						velocity.x = -wallDirX * wallJumpOff.x;
						velocity.y = wallJumpOff.y;
					} else {
						velocity.x = -wallDirX * wallLeap.x;
						velocity.y = wallLeap.y;
					}
				}
				// duplo pulo
				if (useDoubleJump && allowDoubleJump) {
					velocity.y = maxJumpVelocity;
					useDoubleJump = false;
				}
				// pulo
				if (controller.collisions.below) {
					velocity.y = maxJumpVelocity;
					useDoubleJump = true;
				}

			}
			// quando solto jogo a velocidade minima de pulo, para pular baixo se soltar rapido
			if (Input.GetKeyUp (KeyCode.UpArrow)) {
				if (velocity.y > minJumpVelocity) {
					velocity.y = minJumpVelocity;
				}
			}

			//dash
			if ( delayToUseDashAgainInUse <= 0 ) {

				if ( (transform.localScale.x == 1 && Input.GetAxisRaw ("Horizontal") < 0 ) 
				    || (transform.localScale.x == -1 && Input.GetAxisRaw ("Horizontal") > 0 )) {
					lastTimeClickDash = 0;
				}

				if ( Input.GetKeyDown (KeyCode.RightArrow) || Input.GetKeyDown (KeyCode.LeftArrow) ) {
					if ( Time.time - lastTimeClickDash < catchTimeClickDash ){
						timeToDashInUse = timeToDash;
						delayToUseDashAgainInUse = delayToUseDashAgain;
						dashDirection = (input.x > 0)? 1 : -1;
					}
					lastTimeClickDash =  Time.time;
				}
			} else {
				delayToUseDashAgainInUse -= Time.deltaTime;
			}
			if ( timeToDashInUse > 0 ){
				timeToDashInUse -= Time.deltaTime;

				if (! controller.collisions.below && alowDashInAir) {
					velocity.x += dashForce * dashDirection;
					velocity.y = (gravity * Time.deltaTime) * -1;
					timeToDashInUse -= ( 0.5f * Time.deltaTime);
				} else if ( controller.collisions.below ) {
					velocity.x += dashForce * dashDirection;
				} else {
					timeToDashInUse = 0;
				}
			}
		} else {
			// usando o jetpack
			if (input.y > 0) {
				float targetVelocityY = input.y * jetPackMoveSpeed;
				velocity.y = targetVelocityY;
			}
		}
		// aplico a gravidade
		velocity.y += gravity * Time.deltaTime;

		// chamo a função que movimenta de verdade levando em conta as colisões
		controller.Move (velocity * Time.deltaTime, input);
		
		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}
	}

	// inverto o personagem
	void flip(){
		if ( (transform.localScale.x == 1 && Input.GetAxisRaw ("Horizontal") < 0 ) 
		    || (transform.localScale.x == -1 && Input.GetAxisRaw ("Horizontal") > 0 )) {
			velocity.x = 0;
		}

		if (Input.GetAxisRaw ("Horizontal") > 0) {
			transform.localScale = new Vector3( 1.0f, transform.localScale.y,1f);
		}
		if (Input.GetAxisRaw ("Horizontal") < 0) {
			transform.localScale = new Vector3( -1.0f, transform.localScale.y,1f);
		}
	}

	// atiro
	void shoot(){
		fired = false;
		if ( Input.GetKeyDown (KeyCode.Space) && allowShooter && Time.time - lastShootTime > delayShoot) {
			fired = true;
			lastShootTime = Time.time;
			GameObject instance = Instantiate (bullet, new Vector2 (transform.position.x, transform.position.y), transform.rotation) as GameObject;
			instance.transform.localScale = new Vector2 (instance.transform.localScale.x * transform.localScale.x, instance.transform.localScale.y);
		}
	}

	public void ApplyDamage( float damage ){
		gameControl.SendMessage("ApplyDamage",damage);
	}
}