using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class HudControl : MonoBehaviour {
	
	List<string> phrases = new List<string>();
	int iterator = 0;
	float phraseCallTime = 0;

	public float phrasesTimeInterval = 1f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Application.loadedLevel != 0) {
			transform.FindChild ("Canvas").gameObject.SetActive (true);
		} else {
			transform.FindChild ("Canvas").gameObject.SetActive (false);
		}
		
		// caso a variavel com as frases não esteja vazia
		if (phrases.Count > 0) {
			// quando passar o tempo eu troco de frase
			if ( Time.time > phraseCallTime + phrasesTimeInterval ) {

				// caso iterador for maior que a quantidade de frases
				if ( iterator >= phrases.Count ) {
					phrases = new List<string>();
					transform.FindChild ("Canvas/MessagePanel").gameObject.SetActive (false);

				} else {
					//pego o hud de texto
					transform.FindChild ("Canvas/MessagePanel").gameObject.SetActive (true);
					transform.FindChild ("Canvas/MessagePanel/Text").gameObject.GetComponent<Text> ().text = phrases [iterator];  
				
				
					phraseCallTime = Time.time;
					iterator += 1;
				}
			}
		} else {
			iterator = 0;
		}

	}

	void callMessageBox( List<string> newPhrases ){
		phrases = newPhrases;
	}

	void setLifeBar( int i ){
		transform.FindChild ("Canvas/Health").gameObject.GetComponent<Slider> ().value = i;
	}
}
