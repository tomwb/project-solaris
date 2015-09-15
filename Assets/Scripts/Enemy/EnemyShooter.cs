using UnityEngine;
using System.Collections;

public class EnemyShooter : Enemy {

	[Header ("Shoot")]
	public GameObject bullet;
	public float delayShoot = 0.3f;
	float lastShootTime;
	bool fired = false;

	// Use this for initialization
	public override void Start () {
		base.Start ();
	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update ();
	}

	public override void seePlayer(){
		base.seePlayer ();
		// por padrão ele não faz nada quando ve o player
		shoot ();
	}

	void shoot(){
		fired = false;
		if ( Time.time - lastShootTime > delayShoot) {
			fired = true;
			lastShootTime = Time.time;
			GameObject instance = Instantiate (bullet, new Vector2 (transform.position.x, transform.position.y), transform.rotation) as GameObject;
			instance.SendMessage("changeInvoker","Enemy");
			instance.transform.localScale = new Vector2 (instance.transform.localScale.x * transform.localScale.x, instance.transform.localScale.y);
		}
	}
}
