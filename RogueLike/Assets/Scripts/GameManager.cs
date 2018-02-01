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
  

  private BoardManager boardScript;         
	private List<Enemy> enemies;							
	private bool enemiesMoving;
  private DungeonManager dungeonScript;
  private Player playerOne;

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
    dungeonScript = GetComponent<DungeonManager>();
    playerOne = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

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

  public void UpdateBoard(int horizontal, int vertical) {
    boardScript.AddToBoard(horizontal, vertical);
  }

  //public void SetPlayerOne(Player player) {
  //  playerOne = player;
  //}

  public Player GetPlayerOne() {
    return playerOne;
  }

  public void EnterDungeon() {
    dungeonScript.StartDungeon();
    boardScript.SetDungeonBoard(dungeonScript.gridPositions,
      dungeonScript.maxBound, dungeonScript.endPos);
    playerOne.dungeonTransition = false;
  }

  public void ExitDungeon() {
    boardScript.SetWorldBoard();
    playerOne.dungeonTransition = false;
  }
}