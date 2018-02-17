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
  private TextHandle textHandle;
  private BSPDungeonManager dungeonBSPScript;

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
    dungeonBSPScript = GetComponent<BSPDungeonManager>();
    playerOne = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    textHandle = new TextHandle();
    textHandle.ReadFile("seeds");

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

  public Player GetPlayerOne() {
    return playerOne;
  }

  public TextHandle GetTextHandle() {
    return textHandle;
  }

  public void EnterDungeon() {
    dungeonScript.StartDungeon(playerOne.GetPosition());
    boardScript.SetDungeonBoard(dungeonScript.gridPositions,
      dungeonScript.maxBound, dungeonScript.endPos);
    playerOne.dungeonTransition = false;
  }

  public void EnterBSPDungeon() {
    dungeonBSPScript.StartDungeon();
    boardScript.SetDungeonBoard(dungeonBSPScript.gridPositions,
      dungeonBSPScript.height, new Vector2(0, 0));
  }

  public void ExitDungeon() {
    boardScript.SetWorldBoard();
    playerOne.dungeonTransition = false;
  }

  //TODO MOVE IT TO A BETTER PLACE
  public void InstanceTile(Vector2 position, GameObject tile, Transform parent) {
    GameObject toInstantiate = tile;
    GameObject instance = Instantiate(toInstantiate,
      new Vector3(position.x, position.y, 0f), Quaternion.identity) as GameObject;

    instance.transform.SetParent(parent);
  }
}