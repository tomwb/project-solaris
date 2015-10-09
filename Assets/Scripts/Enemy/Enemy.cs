using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent (typeof (Controller2D))]
public class Enemy : MonoBehaviour {

	Vector3 velocity;
	Controller2D controller;
	GameObject player;

	[Tooltip("Vida")]
	public float health = 10;
	float defaultHealth;
	Vector3 defaultPosition;

	[Tooltip("Gravidade, caso não queira deixar 0")]
	public float gravity = -25;

	[Header ("Area de Visão")]
	public bool drawGizmo = true;
	[Tooltip("tempo de espera para voltar a movimentação")]
	public float delayAfterSeePlayer;
	float delayAfterSeePlayerInUse = 0;

	[Tooltip("posição")]
	public Vector3 focusAreaPosition;

	[Tooltip("tamanho")]
	public Vector2 focusAreaSize;

	[Header ("Movimentação")]
	[Tooltip("velocidade")]
	public float moveSpeed = 0.8f;
	[Tooltip("tempo de espera em um ponto")]
	public float walkWaitTime;
	float walkWaitTimeInUse = 0;
	[Tooltip("Pontos de movimentação")]
	public float[] localWaypoints;
	float[] globalWaypoints;
	float distance;

	int fromWaypointIndex = 0;
	int firstCollision = 0;

	// Use this for initialization
	public virtual void Start () {
		defaultHealth = health;
		defaultPosition = transform.position;

		controller = GetComponent<Controller2D> ();
		player = GameObject.FindGameObjectWithTag("Player");

		globalWaypoints = new float[localWaypoints.Length];
		for (int i =0; i < localWaypoints.Length; i++) {
			globalWaypoints[i] = localWaypoints[i] + transform.position.x;
		}
	}
	
	// Update is called once per frame
	public virtual void Update () {
		if (! checkVision ()) {
			// caso tenha visto o player recentemente
			if (delayAfterSeePlayerInUse > 0) {
				delayAfterSeePlayerInUse -= Time.deltaTime;
			} else {
				// caso não tenha visto o player
				defaultMovimentation ();
			}
		} else {
			delayAfterSeePlayerInUse = delayAfterSeePlayer;
			// caso ver o player
			seePlayer ();
		}
	}

	public virtual void seePlayer(){
		// por padrão ele não faz nada quando ve o player
	}

	public virtual void Revive(){
		health = defaultHealth;
		transform.position = defaultPosition;
	}

	bool checkVision(){

		Vector3 focusArea = focusAreaPosition;
		focusArea.x = focusArea.x * transform.localScale.x;
		focusArea += transform.position;

		float left = focusArea.x - (focusAreaSize.x / 2);
		float right = focusArea.x + (focusAreaSize.x / 2);
		float bottom = focusArea.y - (focusAreaSize.y / 2);
		float top = focusArea.y  + (focusAreaSize.y / 2);

		// verifico se o player entrou na area de visão
		if ( player.transform.position.x >= left
		    && player.transform.position.x <= right
		    && player.transform.position.y >= bottom 
		    && player.transform.position.y <= top ) {

			// jogo um raycast para ver se algum objeto na frente dele
			RaycastHit2D hit = Physics2D.Linecast(transform.position, player.transform.position, controller.collisionMask);
			Debug.DrawLine(transform.position, player.transform.position, Color.magenta);
			if (hit.collider.tag == "Player") {
				return true;
			}

		}

		return false;
	}

	void defaultMovimentation () {

		if ( localWaypoints.Length > 0 ){
			//ando em direçao do ponto
			distance = globalWaypoints[fromWaypointIndex] - transform.position.x;
			
			// verifico se ele bateu em algo antes
			if ( (controller.collisions.left || controller.collisions.right ) && firstCollision == 0) {
				callNextPoint();
				firstCollision++;
			}
			
			
			if (distance > 0.2f) {
				velocity.x += moveSpeed * Time.deltaTime;
				transform.localScale = new Vector3( 1.0f, transform.localScale.y,1f);
			} else if (distance < -0.2f) {
				velocity.x -= moveSpeed * Time.deltaTime;
				transform.localScale = new Vector3( -1.0f, transform.localScale.y,1f);
			} else {
				firstCollision = 0;
				velocity.x = 0;
				
				// verifico se vai ter tempo de espera
				if ( walkWaitTimeInUse < 0 && walkWaitTime > 0 ) {
					walkWaitTimeInUse = walkWaitTime;
				}
				if ( walkWaitTimeInUse > 0 ){
					walkWaitTimeInUse -= Time.deltaTime;
				}
				
				if ( walkWaitTimeInUse <= 0){ 
					// mudo o ponteiro
					callNextPoint();
				}
			}
			if ( ! controller.collisions.below) {
				velocity.y += gravity * Time.deltaTime;
			}
			
			//cheguei no ponto viro e vou pro outro
			controller.Move (velocity * Time.deltaTime, Vector2.zero);
		}
	}

	void callNextPoint(){
		fromWaypointIndex++;
		if (fromWaypointIndex == localWaypoints.Length ){
			fromWaypointIndex = 0;
		}
	}

	public void OnDrawGizmos() {
		if (drawGizmo) {
			Gizmos.color = new Color (1, 0, 0, .5f);
			if (localWaypoints != null) {

				float size = .3f;
				
				for (int i =0; i < localWaypoints.Length; i ++) {
					float x = (Application.isPlaying) ? globalWaypoints [i] : localWaypoints [i] + transform.position.x;
					Vector3 globalWaypointPos = new Vector3 (x, transform.position.y, transform.position.z);
					Gizmos.DrawLine (globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
					Gizmos.DrawLine (globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
				}
			}

			Vector3 focusArea = focusAreaPosition;
			focusArea.x = focusArea.x * transform.localScale.x;
			focusArea += transform.position;
			Gizmos.DrawWireCube (focusArea, focusAreaSize);
		}
	}

	public void ApplyDamage( float damage ){
		health -= damage;
		if ( health <= 0 ){
			transform.parent.SendMessage("Die");
			gameObject.SetActive (false);
		}

	
		transform.FindChild ("Canvas").transform.localScale = new Vector3 (transform.localScale.x, 1f, 1f);
		

		GameObject hitText = transform.FindChild ("Canvas/Hit").gameObject;
		GameObject temp = Instantiate (hitText) as GameObject;
		
		RectTransform tempRect = temp.GetComponent<RectTransform> ();
		temp.transform.SetParent (transform.FindChild ("Canvas"));
		
		tempRect.transform.localPosition = hitText.transform.localPosition;
		tempRect.transform.localScale = hitText.transform.localScale;
		tempRect.transform.localRotation = hitText.transform.localRotation;
		temp.GetComponent<Text> ().text = " - " + damage + " Hp";  
		temp.SetActive (true);
		Destroy (temp, 2.0f);
		// verifico se o tiro veio pelas costas, caso tenha vindo eu viro ele para ficar de cara com o player

	}
}
