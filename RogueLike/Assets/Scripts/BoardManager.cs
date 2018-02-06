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
  //endless
  private Transform boardHolder;
  private Dictionary<Vector2, Vector2> gridPositions = 
    new Dictionary<Vector2, Vector2>();
  //dungeon
  private Transform dungeonBoardHolder;
  private Dictionary<Vector2, Vector2> dungeonGridPositions;
  

  private void Start() {
    player = GameManager.instance.GetPlayerOne();
  }

  //create the initial 6 tiles
  public void BoardSetup() {
    boardHolder = new GameObject("Board").transform;

    for(int x = 0; x < columns; x++) {
      for(int y = 0; y < rows; y++) {
        gridPositions.Add(new Vector2(x, y), new Vector2(x, y));
        InstanceTiled(new Vector2(x, y), floorTiles[Random.Range(0, floorTiles.Length)], 
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
      InstanceTiled(tileToAdd, floorTiles[Random.Range(0, floorTiles.Length)],
        boardHolder);

      //choose at random a wall tile to lay
      if(Random.Range(0, 3) == 1) {
        InstanceTiled(tileToAdd, wallTiles[Random.Range(0, floorTiles.Length)],
          boardHolder);
      }
    }

    //exit tile
    if(Random.Range(0, 50) == 1) {
      InstanceTiled(tileToAdd, exit, boardHolder);
    }
  }

  private void InstanceTiled(Vector2 position, GameObject tile, Transform parent) {
    GameObject toInstantiate = tile;
    GameObject instance = Instantiate(toInstantiate,
      new Vector3(position.x, position.y, 0f), Quaternion.identity) as GameObject;

    instance.transform.SetParent(parent);
  }

  public void SetDungeonBoard(Dictionary<Vector2, TileType> dungeonTiles,
    int bound, Vector2 endpos) {

    boardHolder.gameObject.SetActive(false);
    dungeonBoardHolder = new GameObject("Dungeon").transform;
    
    //floor
    foreach(KeyValuePair<Vector2, TileType> tile in dungeonTiles) {
      InstanceTiled(tile.Key, floorTiles[Random.Range(0, floorTiles.Length)], 
        dungeonBoardHolder);
    }

    //borders
    for (int x = -1; x < bound + 1; x++) {
      for (int y = -1; y < bound + 1; y++) {
        if (!dungeonTiles.ContainsKey(new Vector2(x, y)) &&
          ( dungeonTiles.ContainsKey(new Vector2(x + 1, y)) ||
          dungeonTiles.ContainsKey(new Vector2(x, y + 1)) ||
          dungeonTiles.ContainsKey(new Vector2(x - 1, y)) ||
          dungeonTiles.ContainsKey(new Vector2(x, y - 1)))) {

          InstanceTiled(new Vector2(x, y),
            outerWallTiles[Random.Range(0, outerWallTiles.Length)], dungeonBoardHolder);
        }
      }
    }

    //exit
    InstanceTiled(endpos, exit, dungeonBoardHolder);
  }

  public void SetWorldBoard() {
    Destroy(dungeonBoardHolder.gameObject);
    boardHolder.gameObject.SetActive(true);
  }

}
