using UnityEngine;
using System.Collections;

public class SaveArea : MonoBehaviour {

	// Update is called once per frame
	public void OnPlayerCollisionEnter ( GameObject player ) {
		GameObject gameControl = GameObject.FindGameObjectWithTag ("GameController");
		gameControl.SendMessage ("Save");
	}
}
