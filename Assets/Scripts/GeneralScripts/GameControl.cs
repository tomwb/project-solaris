using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary; 
using System.IO;

public class GameControl : MonoBehaviour {

	public static GameControl control;
	public static bool IsPaused = false;

	public float totalHealth = 30f;
	public float health;
	public float money;
	public int scenne;
	public Vector3 playerPosition;

	bool callLoadOnStart = false;
 	bool ChangeScenneOnStart = false;
	public string[] itemList;

	GameObject player;


	// Use this for initialization
	void Awake () {

		// caso nao tiver o control
		if (control == null) {
			// nao destroi ao trocar de cena
			DontDestroyOnLoad (gameObject);
			control = this;
		} else if (control != this && gameObject != null && Application.loadedLevel != 0) {
			// caso tenha outro controle e n seja este deleto este
			Destroy (gameObject);
		}
		player = GameObject.FindGameObjectWithTag("Player");

		// quando entro pego o salvo
		if ( control.callLoadOnStart ) {
			control.callLoadOnStart = false;
			Load();
		}

		// quando troco de cena
		if ( control.ChangeScenneOnStart ) {
			control.ChangeScenneOnStart = false;
			player.transform.position = control.playerPosition;
		}
	}
	
	// aplico dano
	public void ApplyDamage( float damage ){
		health -= damage;
		if ( health <= 0 ){
			Die();
		}
		this.gameObject.SendMessage ("setLifeBar", ( (100 * health) / totalHealth ) );
	}

	public void startGame( ){
		Save ();
		Application.LoadLevel( "scenne_1" );
	}

	public void loadGame( ){

		if ( Load () ) {
			if ( control.scenne == 0 ) {
				startGame( );
			} else {
				control.callLoadOnStart = true;
				Application.LoadLevel (control.scenne);
			}
		} else {
			startGame( );
		}
	}

	// dou reload apartir do ponto salvo
	public void Die(){
		Load ();
		control.callLoadOnStart = true;
		Application.LoadLevel (control.scenne);
	}

	// troco a posição da variavel que controla a posiçao do player nos loads
	public void changePlayerPositionOnChangeScenne(  Vector3 newPlayerPosition  ){
		control.ChangeScenneOnStart = true;
		control.playerPosition = newPlayerPosition;
	}

	// salvo o jogo
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

	// dou load
	public bool Load(){
		if (File.Exists (Application.persistentDataPath + "/playerInfo.dat")) {
			// abro o arquivo
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);

			if (file.Length > 0) {
				// descriptografo
				PlayerData data = (PlayerData)bf.Deserialize (file);
				file.Close ();

				// faço o de-para
				control.health = totalHealth;
				control.money = data.money;
				control.scenne = data.scenne;
				control.playerPosition = new Vector3 (data.playerPositionX, data.playerPositionY, 0);

				if (! player) {
					player = GameObject.FindGameObjectWithTag ("Player");
				}
				if (player) {
					player.transform.position = control.playerPosition;
				}
			}
			return true;
		} else {
			return false;
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
	public string[] itemList;
}
