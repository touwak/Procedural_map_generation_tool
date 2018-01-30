using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public float turnDelay = 0.1f;							
	public int healthPoints = 100;							   
	public static GameManager instance = null;				
	[HideInInspector] public bool playersTurn = true;
  public Player playerOne;

  private BoardManager boardScript;         
	private List<Enemy> enemies;							
	private bool enemiesMoving;								                                                                                                                                                                                                                                                                                                               


	void Awake() {
    if (instance == null) {
      instance = this;
    }
    else if (instance != this) {
      Destroy(gameObject);
    }

    DontDestroyOnLoad(gameObject);
		
		enemies = new List<Enemy>();
    boardScript = GetComponent<BoardManager>();

    InitGame();
	}
	
	//This is called each time a scene is loaded.
	void OnLevelWasLoaded(int index) {
		//Call InitGame to initialize our level.
		InitGame();
	}
	

	void InitGame()	{

		enemies.Clear();
    boardScript.BoardSetup();
	}
	
	void Update()	{

    if (playersTurn || enemiesMoving) {
      return;
    }

		StartCoroutine (MoveEnemies ());
	}
	
	public void GameOver() {
		//Disable this GameManager.
		enabled = false;
	}
	

	IEnumerator MoveEnemies()	{

		enemiesMoving = true;
		
		yield return new WaitForSeconds(turnDelay);
		
		if (enemies.Count == 0) 
		{
			yield return new WaitForSeconds(turnDelay);
		}

		playersTurn = true;
		enemiesMoving = false;
	}

  public void updateBoard(int horizontal, int vertical) {

  }

  public void SetPlayerOne(Player player) {
    playerOne = player;
  }

  public Player GetPlayerOne() {
    return playerOne;
  }
}