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
  [Range(0, 2)] public int mode;
  public bool is2D;

  

  private BoardManager boardScript;         
	private List<Enemy> enemies;							
	private bool enemiesMoving;
  private DungeonManager dungeonScript;
  private Player playerOne;
  private TextHandle textHandle;
  private BSPDungeonManager dungeonBSPScript;
  private MapGenerator automataScript;

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
    automataScript = GetComponent<MapGenerator>();
    if (GameObject.FindGameObjectWithTag("Player")) {
      playerOne = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }
    textHandle = new TextHandle();
    textHandle.ReadFile("seeds");

    InitGame();
	}

  void InitGame() {
    boardScript.is2D = is2D;

    switch (mode) {
      case 0:
        boardScript.BoardSetup();
        break;

      case 1:
        EnterDungeon();
        break;

      case 2:
        EnterBSPDungeon();
        break;

      default:
        boardScript.BoardSetup();
        break;
    }

    if(mode != 0) {
      playerOne.gameObject.SetActive(false);
    }
  }

  public void RefreshGame() {
    boardScript.ResetMap();
    InitGame();
    
  }
	
	void Update()	{

    if (Input.GetKeyDown(KeyCode.Space)){
      RefreshGame();
    }

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
    if (playerOne != null) {
      return playerOne;
    }
    else {
      return null;
    }
  }

  public TextHandle GetTextHandle() {
    return textHandle;
  }

  public void EnterDungeon(int minSize = -1, int maxSize = -1) {
    if(maxSize > minSize && maxSize > 10 || minSize > 0) {
      dungeonScript.minSize = minSize;
      dungeonScript.maxSize = maxSize;
    }

    dungeonScript.StartDungeon(playerOne.Position);
    boardScript.SetDungeonBoard(dungeonScript.gridPositions,
      dungeonScript.maxSize, dungeonScript.maxSize, dungeonScript.endPos);
    playerOne.dungeonTransition = false;
  }

  public void EnterBSPDungeon(int minSize = -1, int maxSize = -1) {

    if (maxSize > minSize && maxSize > 10 || minSize > 0) {
      dungeonBSPScript.minSize = minSize;
      dungeonBSPScript.maxSize = maxSize;
    }

    dungeonBSPScript.StartDungeon();
    boardScript.SetDungeonBoard(dungeonBSPScript.gridPositions,
      dungeonBSPScript.width, dungeonBSPScript.height, new Vector2(100, 100));
    playerOne.dungeonTransition = false;
  }

  public void EnterAutomataCave() {
    automataScript.GenerateMap();
  }

  public void ExitDungeon() {
    boardScript.SetWorldBoard();
    playerOne.dungeonTransition = false;
  }

  //TODO MOVE IT TO A BETTER PLACE
  public void InstanceTile(Vector3 position, GameObject tile, Transform parent) {


    GameObject toInstantiate = tile;
    GameObject instance = Instantiate(toInstantiate, 
      position, Quaternion.identity) as GameObject;

    instance.transform.SetParent(parent);
  }

  public void InstanceTile(Vector3 position, GameObject tile, 
    Transform parent, Quaternion rotation) {


    GameObject toInstantiate = tile;
    GameObject instance = Instantiate(toInstantiate,
      position, rotation) as GameObject;

    instance.transform.SetParent(parent);
  }

}