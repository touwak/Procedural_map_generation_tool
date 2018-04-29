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
  public Camera camera2D;


  private BoardManager boardScript;         
	private List<Enemy> enemies;							
	private bool enemiesMoving;
  private DungeonManager dungeonScript;
  private Player playerOne;
  private TextHandle textHandle;
  private BSPDungeonManager dungeonBSPScript;
  private HexMapCamera camera3D;
  

  void Awake() {
    if (instance == null) {
      instance = this;
    }
    else if (instance != this) {
      Destroy(gameObject);
    }

    //DontDestroyOnLoad(gameObject);
		
		enemies = new List<Enemy>();
    boardScript = GetComponent<BoardManager>();
    dungeonScript = GetComponent<DungeonManager>();
    dungeonBSPScript = GetComponent<BSPDungeonManager>();
    if (GameObject.FindGameObjectWithTag("Player")) {
      playerOne = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }
    textHandle = new TextHandle();
    textHandle.ReadFile("seeds");

    camera3D = FindObjectOfType<HexMapCamera>();
    

    InitGame();
	}

  /// <summary>
  /// Initialice the grid with the map type selected
  /// </summary>
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

    AdjustCamera();

    //active or deactive the player
    if(mode == 0) {
      playerOne.gameObject.SetActive(true);
    }
    else {
      playerOne.gameObject.SetActive(false);
    }
  }

  /// <summary>
  /// reset the map
  /// </summary>
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

  /// <summary>
  /// Adjust the current camera
  /// </summary>
  void AdjustCamera() {
    //switch camera between 2D and 3D
    if (is2D) {
      camera3D.gameObject.SetActive(false);
      camera2D.gameObject.SetActive(true);

      Vector3 newPos = new Vector3(0, 0, -1);
      if (mode != 0) {
        newPos = new Vector3(FirstTilePosition().x, FirstTilePosition().y, -1); 
      }
      
      camera2D.transform.localPosition = newPos;
    }
    //adjust the camera in 3D
    else {
      camera2D.gameObject.SetActive(false);
      camera3D.gameObject.SetActive(true);
      
      if (mode == 0) {
        camera3D.AdjustPosition(new Vector2(0, 0));
      }
      else {
        camera3D.AdjustPosition(FirstTilePosition());
      }
    }
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

  /// <summary>
  /// Update the endless map
  /// </summary>
  /// <param name="horizontal"> horizontal position of the player </param>
  /// <param name="vertical"> vertical position of the player </param>
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

  /// <summary>
  /// Generate a new pathfinding dungeon when the player go trhough a exit tile
  /// </summary>
  /// <param name="minSize"> minimum size of the dungeon </param>
  /// <param name="maxSize"> maximum size of the dungeon </param>
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

  /// <summary>
  /// Generate a new BSP dungeon when the player go trhough a exit tile
  /// </summary>
  /// <param name="minSize"> minimum size of the dungeon </param>
  /// <param name="maxSize"> maximum size of the dungeon </param>
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

  /// <summary>
  /// Translate the player into the enless map
  /// </summary>
  public void ExitDungeon() {
    boardScript.SetWorldBoard();
    playerOne.dungeonTransition = false;
  }

  /// <summary>
  /// Instance a tile in the map
  /// </summary>
  /// <param name="position"> Position of the tile </param>
  /// <param name="tile"> tile to be instantiate </param>
  /// <param name="parent"> the parent of the tile </param>
  public void InstanceTile(Vector3 position, GameObject tile, Transform parent) {
    GameObject toInstantiate = tile;
    GameObject instance = Instantiate(toInstantiate, 
      position, Quaternion.identity) as GameObject;

    instance.transform.SetParent(parent);
  }

  /// <summary>
  /// Instance a tile in the map
  /// </summary>
  /// <param name="position"> Position of the tile </param>
  /// <param name="tile"> tile to be instantiate </param>
  /// <param name="parent"> the parent of the tile </param>
  /// <param name="rotation"> rotation of the tile </param>
  public void InstanceTile(Vector3 position, GameObject tile, 
    Transform parent, Quaternion rotation) {


    GameObject toInstantiate = tile;
    GameObject instance = Instantiate(toInstantiate,
      position, rotation) as GameObject;

    instance.transform.SetParent(parent);
  }

  /// <summary>
  /// Returns the position of the first tile of the grid
  /// </summary>
  /// <returns> a Vector2 with the position</returns>
  Vector2 FirstTilePosition() {
    Vector2 firstTilePos = new Vector2();
      foreach (Vector2 key in dungeonScript.gridPositions.Keys) {
        firstTilePos = key;
        break;
      }
    return firstTilePos;
  }

}