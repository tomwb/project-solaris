using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour {
	
	public bool drawGizmo = true;
	public float reviveRadio = 8f;
	float deadTime;
	bool die = false;
	GameObject player;

	// Use this for initialization
	public virtual void Start () {
		player = GameObject.FindGameObjectWithTag ("Player");
	}

	// Update is called once per frame
	void Update () {

		float distance =Vector3.Distance(transform.position,player.transform.position);
		if ( distance <= reviveRadio && distance >= (reviveRadio-1)) {
			Revive ();
		} else if( distance > reviveRadio ) {
			Die();
		}
	}

	public void Revive ( ) {
		if (! transform.GetChild (0).gameObject.activeSelf) {
			transform.GetChild (0).gameObject.SetActive (true);
			transform.GetChild (0).SendMessage ("Revive");
			die = false;
		}
	}

	public void Die(){
		die = true;
		transform.GetChild(0).gameObject.SetActive(false);
	}

	public void OnDrawGizmos() {
		if (drawGizmo) {
			Gizmos.color = new Color (0, 1, 1, .5f);
			Gizmos.DrawWireSphere (transform.position, reviveRadio);
			Gizmos.DrawWireSphere (transform.position, reviveRadio - 1);
		}
	}
}
