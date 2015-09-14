﻿using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Enemy : MonoBehaviour {

	Vector3 velocity;
	Controller2D controller;
	GameObject player;

	[Tooltip("Vida")]
	public float life = 10;

	[Tooltip("Gravidade, caso não queira deixar 0")]
	public float gravity = -25;

	[Header ("Area de Visão")]

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
		controller = GetComponent<Controller2D> ();
		player = GameObject.FindGameObjectWithTag("Player");

		globalWaypoints = new float[localWaypoints.Length];
		for (int i =0; i < localWaypoints.Length; i++) {
			globalWaypoints[i] = localWaypoints[i] + transform.position.x;
		}
	}
	
	// Update is called once per frame
	public virtual void Update () {

		if ( ! checkVision() ) {
			// caso tenha visto o player recentemente
			if ( delayAfterSeePlayerInUse > 0 ){
				delayAfterSeePlayerInUse -= Time.deltaTime;
			} else {
				// caso não tenha visto o player
				defaultMovimentation ();
			}
		} else {
			delayAfterSeePlayerInUse = delayAfterSeePlayer;
			// caso ver o player
			seePlayer();
		}

	}

	public void seePlayer(){
		// por padrão ele não faz nada quando ve o player
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
		Gizmos.color = new Color (0, 1, 0, .5f);
		if (localWaypoints != null) {

			float size = .3f;
			
			for (int i =0; i < localWaypoints.Length; i ++) {
				float x = (Application.isPlaying)?globalWaypoints[i] : localWaypoints[i] + transform.position.x;
				Vector3 globalWaypointPos = new Vector3( x, transform.position.y,transform.position.z );
				Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
				Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
			}
		}

		Vector3 focusArea = focusAreaPosition;
		focusArea.x = focusArea.x * transform.localScale.x;
		focusArea += transform.position;
		Gizmos.DrawCube ( focusArea, focusAreaSize);
	}

	public void ApplyDamage( float damage ){
		Debug.Log (damage);
		life -= damage;
		if ( life <= 0 ){
			Destroy (gameObject);
		}
	}
}