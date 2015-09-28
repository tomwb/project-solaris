using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary; 
using System.IO;

public class GameControl : MonoBehaviour {

	public static GameControl control;

	public float health;
	public float money;
	public int scenne;
	public Vector3 playerPosition;

	public bool callLoadOnStart = false;
	public bool ChangeScenneOnStart = false;

	GameObject player;

	// Use this for initialization
	void Awake () {
		// caso nao tiver o control
		if(control == null) {
			// nao destroi ao trocar de cena
			DontDestroyOnLoad(gameObject);
			control = this;
		} else if( control != this && gameObject != null) {
			// caso tenha outro controle e n seja este deleto este
			Destroy (gameObject);
		}
		player = GameObject.FindGameObjectWithTag("Player");
		// quando entro pego o salvo
		if ( control.callLoadOnStart ) {
			control.callLoadOnStart = false;
			Load();
		}

		if ( control.ChangeScenneOnStart ) {
			control.ChangeScenneOnStart = false;
			player.transform.position = control.playerPosition;
		}
	}

	void OnGUI () {
		GUI.Label(new Rect(10,10,100,30), "Health: " + health);
	}

	void Update() {
		// ao apertar S salva o jogo
//		if (Input.GetKeyDown (KeyCode.S)) {
//			Save();
//		}

		// ao apertar L faz load no jogo
		if (Input.GetKeyDown (KeyCode.L)) {
			Load();
		}
	}

	public void ApplyDamage( float damage ){
		health -= damage;
		if ( health <= 0 ){
			Die();
		}
	}

	public void Die(){
		control.callLoadOnStart = true;
		Application.LoadLevel (scenne);
	}

	public void changePlayerPositionOnChangeScenne(  Vector3 newPlayerPosition  ){
		control.ChangeScenneOnStart = true;
		control.playerPosition = newPlayerPosition;
	}

	public void Save() {
		// abro o arquivo
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create ( Application.persistentDataPath + "/playerInfo.dat" );

		// faço o de-para atualizar as variaveis
		player = GameObject.FindGameObjectWithTag("Player");
		if (player != null) {
			control.playerPosition = player.transform.position;
		}

		PlayerData data = new PlayerData();
		data.health = control.health;
		data.money = control.money;
		data.playerPositionX = control.playerPosition.x;
		data.playerPositionY = control.playerPosition.y;
		data.scenne = Application.loadedLevel;

		// salvo
		bf.Serialize(file, data);
		file.Close();
	}

	public void Load(){
		if ( File.Exists ( Application.persistentDataPath + "/playerInfo.dat" ) ){
			// abro o arquivo
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open ( Application.persistentDataPath + "/playerInfo.dat", FileMode.Open );

			if ( file.Length  > 0  ) {
				// descriptografo
				PlayerData data = (PlayerData) bf.Deserialize(file);
				file.Close();

				// faço o de-para
	//			control.health = data.health;
				control.health = 15f;
				control.money = data.money;
				control.scenne = data.scenne;
				control.playerPosition = new Vector3( data.playerPositionX, data.playerPositionY, 0);

				if ( ! player ){
					player = GameObject.FindGameObjectWithTag("Player");
				}
				player.transform.position = control.playerPosition;
			}
		}
	}
}

[Serializable]
class PlayerData{
	public float health;
	public float money;
	public int scenne;
	public float playerPositionX;
	public float playerPositionY;
}
