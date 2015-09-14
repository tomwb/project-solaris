using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	public float speed = 17.0f;
	public LayerMask collisionMask;
	public float destroyAfterTime = 0.8f;
	public float damage = 5f;
	float shotTime;


	void Start(){
		shotTime = Time.time;
	}

	void Update () {
		int direcao = 1;
		if ( transform.localScale.x < 0 ){
			direcao = -1;
		}
		transform.Translate(Vector2.right * speed * Time.deltaTime * direcao);

		RaycastHit2D hit = Physics2D.Raycast (transform.position, transform.forward, 0, collisionMask.value);
		if (hit) {
			callDestroy();
			if (hit.collider.tag == "Enemy") {
				hit.collider.SendMessage("ApplyDamage",damage);
			}
		}

		if ( Time.time - shotTime > destroyAfterTime ) {
			callDestroy();
		}
	}

	void callDestroy(){
		Destroy (gameObject);
	}
}