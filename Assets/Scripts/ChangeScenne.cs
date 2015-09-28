using UnityEngine;
using System.Collections;

public class ChangeScenne : MonoBehaviour {

	GameObject gameControl;

	public string scenneName;

	public Vector3 positionTarget;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	public void OnPlayerCollisionEnter ( GameObject player ) {
		gameControl = GameObject.FindGameObjectWithTag("GameController");

		gameControl.SendMessage ("changePlayerPositionOnChangeScenne", positionTarget);
		Application.LoadLevel (scenneName);

	}
}
