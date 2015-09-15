using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour {

	public float reviveTime;
	float deadTime;
	bool die = false;
//	GameObject player;

	// Use this for initialization
	public virtual void Start () {
//		player = GameObject.FindGameObjectWithTag ("Player");
	}

	// Update is called once per frame
	void Update () {

//		float distance =Vector3.Distance(transform.GetChild(0).gameObject.transform.position,player.transform.position);
//		Debug.Log (distance);
//		if (distance <= 10.0f) {
			Revive ();
//		} else {
//			Die();
//		}
	}

	public void Revive () {
		if ( die && Time.time - deadTime > reviveTime) {
			transform.GetChild(0).gameObject.SetActive(true);
			transform.GetChild(0).SendMessage("Revive");
			die = false;
		}
	}

	public void Die(){
		die = true;
		deadTime = Time.time;
		transform.GetChild(0).gameObject.SetActive(false);
	}

//	void OnBecameVisible() {
//		visible = true;
//	}
//
//	void OnBecameInvisible() {
//		visible = false;
//	}
}
