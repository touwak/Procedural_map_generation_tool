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
  public GameObject[] floorTiles;
  private Transform boardHolder;
  private Dictionary<Vector2, Vector2> gridPositions = 
    new Dictionary<Vector2, Vector2>();

  private void Start() {
    player = GameManager.instance.GetPlayerOne();
  }

  public void BoardSetup() {
    boardHolder = new GameObject("Board").transform;

    for(int x = 0; x < columns; x++) {
      for(int y = 0; y < rows; y++) {
        gridPositions.Add(new Vector2(x, y), new Vector2(x, y));
        GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
        GameObject instance = Instantiate (toInstantiate, 
          new Vector3(x,y, 0f), Quaternion.identity) as GameObject;
        instance.transform.SetParent(boardHolder);
      }
    }
  }

  //TODO debuging player reference
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
  //some parts of this func are repeated in other place TODO create a separate function
  private void AddTiles(Vector2 tileToAdd) {
    if (!gridPositions.ContainsKey(tileToAdd)) {
      gridPositions.Add(tileToAdd, tileToAdd);
      GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
      GameObject instance = Instantiate(toInstantiate, 
        new Vector3(tileToAdd.x, tileToAdd.y, 0f), Quaternion.identity) as GameObject;

      instance.transform.SetParent(boardHolder);
    }
  }

}
