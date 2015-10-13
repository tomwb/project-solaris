using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	public bool drawGizmo = false;
	public float speed = 17.0f;
	public LayerMask collisionMask;
	public float destroyAfterDistance = 8f;

	public float damage = 5f;
	[HideInInspector]
	public GameObject invoker;


	void Start(){
	}

	void Update () {
		int direcao = 1;
		if ( transform.localScale.x < 0 ){
			direcao = -1;
		}
		transform.Translate(Vector2.right * speed * Time.deltaTime * direcao);

		RaycastHit2D hit = Physics2D.Raycast (transform.position, transform.forward, 0, collisionMask.value);
		if (hit) {
			if ( hit.collider.tag != invoker.tag){
				callDestroy();
				if ( hit.collider.tag == "Player"
				    || hit.collider.tag == "Enemy") {
					hit.collider.SendMessage("ApplyDamage",damage);
				}
			}
		}

		if ( Vector2.Distance(transform.position, invoker.transform.position) > destroyAfterDistance ) {
			callDestroy();
		}
	}

	void callDestroy(){
		if ("Player" == invoker.tag) {
			invoker.SendMessage ("destroyShoot");
		}
		Destroy (gameObject);
	}

	public void changeInvoker( GameObject newInvoker ){
		invoker = newInvoker;
	}

	public void OnDrawGizmos() {
		if (drawGizmo) {
			Gizmos.color = new Color (0, 1, 1, .5f);
			Gizmos.DrawWireSphere (transform.position, destroyAfterDistance);
		}
	}
}