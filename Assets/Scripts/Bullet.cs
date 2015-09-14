using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	private float speed = 10.0f;
	public LayerMask collisionMask;

	void Update () {
		int direcao = 1;
		if ( transform.localScale.x < 0 ){
			direcao = -1;
		}
		transform.Translate(Vector2.right * speed * Time.deltaTime * direcao);

		if (Physics2D.Raycast(transform.position, transform.forward, 0, collisionMask.value)) {
			Destroy (gameObject);
		}
	}
}