using UnityEngine;
using System.Collections.Generic;

public class CallMessageBox : MonoBehaviour {

	public List<string> phrases = new List<string>();

	// Update is called once per frame
	public void OnPlayerCollisionEnter ( GameObject player ) {
		GameObject gameControl = GameObject.FindGameObjectWithTag ("GameController");
		gameControl.SendMessage ("callMessageBox", phrases);
		Destroy (gameObject);
	}
}
