using UnityEngine;
using System;
using System.Collections.Generic; 		
using Random = UnityEngine.Random; 		                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        

public class BoardManager : MonoBehaviour
{

	[Serializable]
	public class Count
	{
		public int minimum; 			
		public int maximum; 			  
		
		public Count (int min, int max)
		{
			minimum = min;
			maximum = max;
		}
	}

  private Player player;
  public int columns = 5;
  public int rows = 5;

  [HideInInspector]
  public bool is2D;
  //-------------2D---------------------
  public GameObject exit;
  public GameObject[] floorTiles;
  public GameObject[] wallTiles;
  public GameObject[] outerWallTiles;
  public GameObject chestTile;
  

  //--------------3D----------------------
  public GameObject[] floorTiles3D;
  public GameObject[] wallTiles3D;
  public GameObject[] outerWallTiles3D;
  public GameObject chestTile3D;
  public GameObject exit3D;

  private float chestDir = 270.0f;
  private float floorLevel = 0;

  //endless
  private Transform boardHolder;
  private Dictionary<Vector2, Vector2> gridPositions = 
    new Dictionary<Vector2, Vector2>();
  //dungeon
  private Transform dungeonBoardHolder;
  private Dictionary<Vector2, Vector2> dungeonGridPositions;
  //BSP
  private Transform dungeonBSPBoardHolder;
  private Dictionary<Vector2, Vector2> dungeonBSPGridPositions;

  private void Start() {
    player = GameManager.instance.GetPlayerOne();
  }

  //create the initial 6 tiles
  public void BoardSetup() {
    boardHolder = new GameObject("Board").transform;
    floorLevel = (floorTiles3D[0].transform.localScale.y / 2) + 0.5f;

    for (int x = 0; x < columns; x++) {
      for(int y = 0; y < rows; y++) {

        //2D
        if (is2D) {
          gridPositions.Add(new Vector2(x, y), new Vector2(x, y));
          GameManager.instance.InstanceTile(new Vector3(x, y), floorTiles[Random.Range(0, floorTiles.Length)],
            boardHolder);
        }
        else {
          //3D
          gridPositions.Add(new Vector2(x, y), new Vector2(x, y));
          GameManager.instance.InstanceTile(new Vector3(x, 0, y), floorTiles3D[Random.Range(0, floorTiles3D.Length)],
            boardHolder);
        }
      }
    }
  }

  public void AddToBoard(int horizontal, int vertical) {
    if (horizontal == 1) {
      int x = (int)player.Position.x;
      int sightX = x + 2;
      for (x += 1; x <= sightX; x++) {
        int y = (int)player.Position.y;
        int sightY = y + 1;
        for (y -= 1; y <= sightY; y++) {
          AddTiles(new Vector2(x, y));
        }
      }
    }
    else if (horizontal == -1) {
      int x = (int)player.Position.x;
      int sightX = x - 2;
      for (x -= 1; x >= sightX; x--) {
        int y = (int)player.Position.y;
        int sightY = y + 1;
        for (y -= 1; y <= sightY; y++) {
          AddTiles(new Vector2(x, y));
        }
      }
    }
    else if (vertical == 1) {
      int y = (int)player.Position.y;
      int sightY = y + 2;
      for(y += 1; y <= sightY; y++) {
        int x = (int)player.Position.x;
        int sightX = x + 1;
        for(x -= 1; x <= sightX; x++) {
          AddTiles(new Vector2(x, y));
        }
      }
    }
    else if (vertical == -1) {
      int y = (int)player.Position.y;
      int sightY = y - 2;
      for (y -= 1; y >= sightY; y--) {
        int x = (int)player.Position.x;
        int sightX = x + 1;
        for (x -= 1; x <= sightX; x++) {
          AddTiles(new Vector2(x, y));
        }
      }
    }
  }
  
  private void AddTiles(Vector2 tileToAdd) {
    if (!gridPositions.ContainsKey(tileToAdd)) {
      gridPositions.Add(tileToAdd, tileToAdd);

      //----------------------FLOOR-----------------------
      //2D
      if (is2D) {
        GameManager.instance.InstanceTile(tileToAdd, floorTiles[Random.Range(0, floorTiles.Length)],
          boardHolder);
      }
      else {
        //3D
        GameManager.instance.InstanceTile(new Vector3(tileToAdd.x, 0, tileToAdd.y), floorTiles3D[Random.Range(0, floorTiles3D.Length)],
          boardHolder);
      }

      //-------------------BREAKABLE WALLS-------------------
      if(Random.Range(0, 3) == 1) {
        if (is2D) {
          GameManager.instance.InstanceTile(tileToAdd, wallTiles[Random.Range(0, floorTiles.Length)],
            boardHolder);
        }
        else {
          GameManager.instance.InstanceTile(new Vector3(tileToAdd.x, floorLevel, tileToAdd.y), wallTiles3D[Random.Range(0, wallTiles3D.Length)],
            boardHolder);
        }
      }
    }

    //exit tile
    if(Random.Range(0, 75) == 1 /*&& !gridPositions.ContainsKey(tileToAdd)*/ ) {
      //2D
      if (is2D) {
        GameManager.instance.InstanceTile(tileToAdd, exit, boardHolder);
      }
      else {
        //3D
        floorLevel -= 0.5f;
        GameManager.instance.InstanceTile(new Vector3(tileToAdd.x, floorLevel, tileToAdd.y), 
          exit3D, boardHolder);

        floorLevel += 0.5f;
      }
    }
  }

  public void SetDungeonBoard(Dictionary<Vector2, TileType> dungeonTiles,
    int width, int height, Vector2 endpos) {

    floorLevel = (floorTiles3D[0].transform.localScale.y / 2);

    //boardHolder.gameObject.SetActive(false);
    dungeonBoardHolder = new GameObject("Dungeon").transform;
    
    //-----------------------------FLOOR-----------------------------
    foreach(KeyValuePair<Vector2, TileType> tile in dungeonTiles) {

      // 2D
      if (is2D) {
        GameManager.instance.InstanceTile(tile.Key, floorTiles[Random.Range(0, floorTiles.Length)], 
          dungeonBoardHolder);
      }
      else {
        // 3D
        GameManager.instance.InstanceTile(new Vector3(tile.Key.x, 0, tile.Key.y), floorTiles3D[Random.Range(0, floorTiles3D.Length)],
          dungeonBoardHolder);
      }
      //--------------------------CHEST--------------------------------
      if (tile.Value == TileType.chest) {
        // 2D
        if (is2D) {
          GameManager.instance.InstanceTile(tile.Key, chestTile,
            dungeonBoardHolder);
        }
        else {
          // 3D
          CheckNeighbours(tile.Key, dungeonTiles, false);
          Vector3 rotation = new Vector3(0, chestDir, 0);
          Quaternion chestRotation = Quaternion.Euler(rotation);
          GameManager.instance.InstanceTile(new Vector3(tile.Key.x, floorLevel, tile.Key.y),
            chestTile3D, dungeonBoardHolder, chestRotation);

          chestDir = 270.0f;
        }
      }
    }

    //--------------------------BORDERS------------------------------
    for (int x = -1; x < width + 1; x++) {
      for (int y = -1; y < height + 1; y++) {
        if (!dungeonTiles.ContainsKey(new Vector2(x, y)) &&
            CheckNeighbours(new Vector2(x, y), dungeonTiles)) {

          //2D
          if (is2D) {
            GameManager.instance.InstanceTile(new Vector2(x, y),
              outerWallTiles[Random.Range(0, outerWallTiles.Length)], dungeonBoardHolder);
          }
          else {
            //3D
            float posY = (outerWallTiles3D[0].transform.localScale.y / 2) - 0.5f;
            GameManager.instance.InstanceTile(new Vector3(x, posY, y),
              outerWallTiles3D[Random.Range(0, outerWallTiles3D.Length)], dungeonBoardHolder);
          }
        }
      }
    }

    //--------------------EXIT----------------------------------------------
    //2D
    if (is2D) {
      GameManager.instance.InstanceTile(endpos, exit, dungeonBoardHolder);
    }
    else {
      //3D
      GameManager.instance.InstanceTile(new Vector3(endpos.x, floorLevel, endpos.y), exit3D, dungeonBoardHolder);
    }
  }

  private bool CheckNeighbours(Vector2 pos, Dictionary<Vector2, TileType> dungeonTiles, bool isFill = true) {
    //right
    if(dungeonTiles.ContainsKey(new Vector2(pos.x + 1, pos.y)) == isFill){
      chestDir = 270f;
      return true;
    }
    //top
    else if (dungeonTiles.ContainsKey(new Vector2(pos.x, pos.y + 1)) == isFill) {
      chestDir = 180f;
      return true;
    }
    //left
    else if(dungeonTiles.ContainsKey(new Vector2(pos.x - 1, pos.y)) == isFill) {
      chestDir = 90f;
      return true;
    }
    //bottom
    else if(dungeonTiles.ContainsKey(new Vector2(pos.x, pos.y - 1)) == isFill) {
      chestDir = 0f;
      return true;
    }

    return false;
  }

  public void SetWorldBoard() {
    Destroy(dungeonBoardHolder.gameObject);
    boardHolder.gameObject.SetActive(true);
  }

  // reset the map
  public void ResetEndlessMap() {
    if (boardHolder != null) {
      Destroy(boardHolder.gameObject);
    }
    gridPositions.Clear();
    BoardSetup();
    GameManager.instance.GetPlayerOne().Position = new Vector2(2, 2);
  }

  public void ResetMap() {

    // endless
    if (boardHolder != null) {
      Destroy(boardHolder.gameObject);
      gridPositions.Clear();
      GameManager.instance.GetPlayerOne().Position = new Vector2(2, 2);
    }
    //dungeons
    else if (dungeonBoardHolder != null) {
      Destroy(dungeonBoardHolder.gameObject);
    }
    
  }

}
