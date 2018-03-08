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
  public GameObject exit;
  public GameObject[] floorTiles;
  public GameObject[] wallTiles;
  public GameObject[] outerWallTiles;
  public GameObject chestTile;

  //--------3D----------------------
  public GameObject[] floorTiles3D;
  public GameObject[] outerWallTiles3D;
  public GameObject chestTile3D;

  private float chestDir = 270;

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

    for(int x = 0; x < columns; x++) {
      for(int y = 0; y < rows; y++) {
        gridPositions.Add(new Vector2(x, y), new Vector2(x, y));
        GameManager.instance.InstanceTile(new Vector3(x, y), floorTiles[Random.Range(0, floorTiles.Length)], 
          boardHolder);
      }
    }
  }

  public void AddToBoard(int horizontal, int vertical) {
    if (horizontal == 1) {
      int x = (int)player.GetPosition().x;
      int sightX = x + 2;
      for (x += 1; x <= sightX; x++) {
        int y = (int)player.GetPosition().y;
        int sightY = y + 1;
        for (y -= 1; y <= sightY; y++) {
          AddTiles(new Vector2(x, y));
        }
      }
    }
    else if (horizontal == -1) {
      int x = (int)player.GetPosition().x;
      int sightX = x - 2;
      for (x -= 1; x >= sightX; x--) {
        int y = (int)player.GetPosition().y;
        int sightY = y + 1;
        for (y -= 1; y <= sightY; y++) {
          AddTiles(new Vector2(x, y));
        }
      }
    }
    else if (vertical == 1) {
      int y = (int)player.GetPosition().y;
      int sightY = y + 2;
      for(y += 1; y <= sightY; y++) {
        int x = (int)player.GetPosition().x;
        int sightX = x + 1;
        for(x -= 1; x <= sightX; x++) {
          AddTiles(new Vector2(x, y));
        }
      }
    }
    else if (vertical == -1) {
      int y = (int)player.GetPosition().y;
      int sightY = y - 2;
      for (y -= 1; y >= sightY; y--) {
        int x = (int)player.GetPosition().x;
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
      GameManager.instance.InstanceTile(tileToAdd, floorTiles[Random.Range(0, floorTiles.Length)],
        boardHolder);

      //choose at random a wall tile to lay
      if(Random.Range(0, 3) == 1) {
        GameManager.instance.InstanceTile(tileToAdd, wallTiles[Random.Range(0, floorTiles.Length)],
          boardHolder);
      }
    }

    //exit tile
    if(Random.Range(0, 50) == 1) {
      GameManager.instance.InstanceTile(tileToAdd, exit, boardHolder);
    }
  }

  public void SetDungeonBoard(Dictionary<Vector2, TileType> dungeonTiles,
    int width, int height, Vector2 endpos) {

    float floorLevel = (floorTiles3D[0].transform.localScale.y / 2);

    //boardHolder.gameObject.SetActive(false);
    dungeonBoardHolder = new GameObject("Dungeon").transform;
    
    //-----------------------------FLOOR-----------------------------
    foreach(KeyValuePair<Vector2, TileType> tile in dungeonTiles) {

      // 2D
      /*GameManager.instance.InstanceTile(tile.Key, floorTiles[Random.Range(0, floorTiles.Length)], 
        dungeonBoardHolder);*/

      // 3D
      GameManager.instance.InstanceTile(new Vector3(tile.Key.x, 0, tile.Key.y), floorTiles3D[Random.Range(0, floorTiles3D.Length)],
        dungeonBoardHolder);

      //--------------------------CHEST--------------------------------
      if (tile.Value == TileType.chest) {
        // 2D
        /*GameManager.instance.InstanceTile(tile.Key, chestTile,
          dungeonBoardHolder);*/

        // 3D
        CheckNeighbours(tile.Key, dungeonTiles, false);
        Vector3 rotation = new Vector3(0, chestDir, 0);
        Quaternion chestRotation = Quaternion.Euler(rotation);
        GameManager.instance.InstanceTile(new Vector3(tile.Key.x, floorLevel, tile.Key.y), 
          chestTile3D, dungeonBoardHolder, chestRotation);

        chestDir = 270;
      }
    }

    //--------------------------BORDERS------------------------------
    for (int x = -1; x < width + 1; x++) {
      for (int y = -1; y < height + 1; y++) {
        if (!dungeonTiles.ContainsKey(new Vector2(x, y)) &&
            CheckNeighbours(new Vector2(x, y), dungeonTiles)) {
          
          //2D
          //GameManager.instance.InstanceTile(new Vector2(x, y),
          //  outerWallTiles[Random.Range(0, outerWallTiles.Length)], dungeonBoardHolder);

          //3D
          float posY = (outerWallTiles3D[0].transform.localScale.y / 2) - 0.5f;
          GameManager.instance.InstanceTile(new Vector3(x, posY, y),
            outerWallTiles3D[Random.Range(0, outerWallTiles3D.Length)], dungeonBoardHolder);
        }
      }
    }

    //--------------------EXIT----------------------------------------------
    GameManager.instance.InstanceTile(endpos, exit, dungeonBoardHolder);
  }

  private bool CheckNeighbours(Vector2 pos, Dictionary<Vector2, TileType> dungeonTiles, bool isFill = true) {
    //right
    if(dungeonTiles.ContainsKey(new Vector2(pos.x + 1, pos.y)) == isFill){
      chestDir = 270;
      return true;
    }
    //top
    else if (dungeonTiles.ContainsKey(new Vector2(pos.x, pos.y + 1)) == isFill) {
      chestDir = 180;
      return true;
    }
    //left
    else if(dungeonTiles.ContainsKey(new Vector2(pos.x - 1, pos.y)) == isFill) {
      chestDir = 90;
      return true;
    }
    //bottom
    else if(dungeonTiles.ContainsKey(new Vector2(pos.x, pos.y - 1)) == isFill) {
      chestDir = 0;
      return true;
    }

    return false;
  }

  public void SetWorldBoard() {
    Destroy(dungeonBoardHolder.gameObject);
    boardHolder.gameObject.SetActive(true);
  }

}
