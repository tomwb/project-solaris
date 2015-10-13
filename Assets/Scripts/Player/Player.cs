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
	public int maxShootInScreen = 3;
	int shootInScreen = 0;
	float lastShootTime;
	bool fired = false;

	[Header ("Ataque meele")]
	public float distanceMeele = 2.5f;
	public float damageMeele = 5f;
	public LayerMask enemyLayer;

	[Header ("Configurações")]
	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;
	public float moveSpeed = 8;
	
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
		controller.useColliderFunctions = true;

		// defino a gravidade baseado no tamanho do pulo que quero e no tempo de queda
		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);

		shootInScreen = 0;
	}
	
	void Update() {
		flip ();
		atack ();

		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		int wallDirX = (controller.collisions.left) ? -1 : 1;
		
		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

		// caso eu não estiver de jetpack
		if (! allowJetpack) {
			bool wallSliding = false;
			if ( ( ( controller.collisions.left && velocity.x < 0 )
			      || controller.collisions.right && velocity.x > 0 ) 
			    && !controller.collisions.below
			    && velocity.y < 0) {
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
					if ( velocity.y <= 0 ){
						velocity.y = (gravity * Time.deltaTime) * -1;
					}
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

	void atack(){
		//verifico se vai ser meele
		Debug.DrawRay (transform.position, Vector2.right * transform.localScale.x * distanceMeele, Color.red);
		Debug.DrawRay (transform.position, (Vector2.up * ( distanceMeele / 2)) + (Vector2.right * transform.localScale.x * ( distanceMeele / 2) )  , Color.red);
		Debug.DrawRay (transform.position, (Vector2.down * ( distanceMeele / 2)) + (Vector2.right * transform.localScale.x * ( distanceMeele / 2) ), Color.red);

		RaycastHit2D hitCollisionRight = Physics2D.Raycast(transform.position, Vector2.right * transform.localScale.x, distanceMeele,enemyLayer);
		RaycastHit2D hitCollisionUp = Physics2D.Raycast(transform.position, Vector2.up + (Vector2.right * transform.localScale.x) , distanceMeele / 2, enemyLayer);
		RaycastHit2D hitCollisionDown = Physics2D.Raycast(transform.position, Vector2.down + (Vector2.right * transform.localScale.x), distanceMeele / 2, enemyLayer);
		// caso seja meele
		if ( hitCollisionRight ) {
			Meele ( hitCollisionRight.collider.gameObject );
		} else if ( hitCollisionUp ) {
			Meele ( hitCollisionUp.collider.gameObject );
		} else if ( hitCollisionDown ) {
			Meele ( hitCollisionDown.collider.gameObject	);
		} else {
			// caso não seja
			Shoot ();
		}
	}

	void Meele( GameObject enemy ) {
		if (Input.GetKeyDown (KeyCode.Space)) {
			enemy.SendMessage("ApplyDamage",damageMeele );
			GetComponent<Animator>().SetTrigger("meeleAtack");
		}
	}

	// atiro
	void Shoot(){
		fired = false;
		if ( Input.GetKeyDown (KeyCode.Space) && allowShooter && Time.time - lastShootTime > delayShoot) {
			fired = true;
			// caso tenha chegado ao limite de tiros 
			if ( shootInScreen < maxShootInScreen ) {
				shootInScreen++;
				lastShootTime = Time.time;
				GameObject instance = Instantiate (bullet, new Vector2 (transform.position.x, transform.position.y), transform.rotation) as GameObject;
				instance.SendMessage("changeInvoker",gameObject);
				instance.transform.localScale = new Vector2 (instance.transform.localScale.x * transform.localScale.x, instance.transform.localScale.y);

			}
		}
	}

	public void destroyShoot( ){
		shootInScreen--;
		if (shootInScreen < 0) {
			shootInScreen = 0;
		}
	}

	public void ApplyDamage( float damage ){
		gameControl.SendMessage("ApplyDamage",damage);
	}


	public void RaycastOnCollisionEnter( RaycastHit2D hit ){
		hit.collider.SendMessage("OnPlayerCollisionEnter", gameObject,SendMessageOptions.DontRequireReceiver );
	}

	public void RaycastOnCollisionStay( RaycastHit2D hit ){

	}

}