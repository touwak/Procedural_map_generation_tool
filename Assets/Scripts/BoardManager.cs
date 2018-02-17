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
        GameManager.instance.InstanceTile(new Vector2(x, y), floorTiles[Random.Range(0, floorTiles.Length)], 
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
    int bound, Vector2 endpos) {

    boardHolder.gameObject.SetActive(false);
    dungeonBoardHolder = new GameObject("Dungeon").transform;
    
    //floor
    foreach(KeyValuePair<Vector2, TileType> tile in dungeonTiles) {
      GameManager.instance.InstanceTile(tile.Key, floorTiles[Random.Range(0, floorTiles.Length)], 
        dungeonBoardHolder);

      //chest
      if(tile.Value == TileType.chest) {
        GameManager.instance.InstanceTile(new Vector2(tile.Key.x, tile.Key.y), chestTile,
          dungeonBoardHolder);
      }
    }

    //borders
    for (int x = -1; x < bound + 1; x++) {
      for (int y = -1; y < bound + 1; y++) {
        if (!dungeonTiles.ContainsKey(new Vector2(x, y)) && 
            CheckBorders(new Vector2(x, y), dungeonTiles)) {

          GameManager.instance.InstanceTile(new Vector2(x, y),
            outerWallTiles[Random.Range(0, outerWallTiles.Length)], dungeonBoardHolder);
        }
      }
    }

    //exit
    GameManager.instance.InstanceTile(endpos, exit, dungeonBoardHolder);
  }

  private bool CheckBorders(Vector2 pos, Dictionary<Vector2, TileType> dungeonTiles) {
    //right
    if(dungeonTiles.ContainsKey(new Vector2(pos.x + 1, pos.y))){
      return true;
    }
    //top
    else if (dungeonTiles.ContainsKey(new Vector2(pos.x, pos.y + 1))) {
      return true;
    }
    //left
    else if(dungeonTiles.ContainsKey(new Vector2(pos.x - 1, pos.y))) {
      return true;
    }
    //bottom
    else if(dungeonTiles.ContainsKey(new Vector2(pos.x, pos.y - 1))) {
      return true;
    }

    return false;
  }

  public void SetWorldBoard() {
    Destroy(dungeonBoardHolder.gameObject);
    boardHolder.gameObject.SetActive(true);
  }

}
