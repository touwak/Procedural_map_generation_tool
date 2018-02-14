using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public enum TileType {
  essential,
  random,
  empty,
  outerWall
}

public class DungeonManager : MonoBehaviour {
 

  [Serializable]
  public class PathTile {
    public TileType type;
    public Vector2 position;
    public List<Vector2> adjacentPathTiles;

    public PathTile(TileType t, Vector2 p, int min, int max,
      Dictionary<Vector2, TileType> currentTiles) {
      type = t;
      position = p;
      adjacentPathTiles = GetAdjacentPath(min, max, currentTiles);
    }
    //return a number of adjacent tiles if they are not already exist
    public List<Vector2> GetAdjacentPath(int minBound, int maxBound,
      Dictionary<Vector2, TileType> currentTiles) {
      List<Vector2> pathTiles = new List<Vector2>();

      if (position.y + 1 < maxBound &&
        !currentTiles.ContainsKey(new Vector2(position.x, position.y + 1))) {
        pathTiles.Add(new Vector2(position.x, position.y + 1));
      }
      if (position.x + 1 < maxBound &&
        !currentTiles.ContainsKey(new Vector2(position.x + 1, position.y))) {
        pathTiles.Add(new Vector2(position.x + 1, position.y));
      }
      if (position.y - 1 > minBound &&
        !currentTiles.ContainsKey(new Vector2(position.x, position.y - 1))) {
        pathTiles.Add(new Vector2(position.x, position.y - 1));
      }
      if (position.x - 1 >= minBound &&
        !currentTiles.ContainsKey(new Vector2(position.x - 1, position.y))
        && type != TileType.essential) {
        pathTiles.Add(new Vector2(position.x - 1, position.y));
      }

      return pathTiles;
    }
  }

  public Dictionary<Vector2, TileType> gridPositions =
    new Dictionary<Vector2, TileType>();
  public int minBound = 0, maxBound;
  public static Vector2 startPos;
  public Vector2 endPos;
  public bool seedDungeon = false;
  public int chamberSize = 3;

  //no delete dungeon entrance
  public void StartDungeon(Vector2 playerPos) {
    if (seedDungeon) {
      TextHandle textHandle = GameManager.instance.GetTextHandle();
      int seed = textHandle.FindDungeon(playerPos);

      //the dungeon doesn't exist
      if (seed == -1) {
        int posX = (int)playerPos.x;
        int posY = (int)playerPos.y;
        string pos = posX + "|" + posY + "|";
        seed = Random.Range(0, 100 + 1);

        string seedCount = pos + seed.ToString() + "|";
        textHandle.WriteFile("seeds", seedCount);
      }
        Random.seed = seed;
    }

    gridPositions.Clear();
    maxBound = Random.Range(50, 101);

    //BuildAStarPath();
    BuildEssentialPath();
    BuildRandomPath();
  }

  //TODO Add the possibility to switch Start/End points between horizontal and vertical 
  private void BuildEssentialPath() {
    //first node
    int randomY = Random.Range(0, maxBound + 1);
    PathTile ePath = new PathTile(TileType.essential,
      new Vector2(0, randomY), minBound, maxBound, gridPositions);
    startPos = ePath.position;

    int boundTracker = 0;
    //when boundTracker is equal to maxBound means that we reach the last column of the right
    while (boundTracker < maxBound) {
      gridPositions.Add(ePath.position, TileType.empty);
      int adjacentTileCount = ePath.adjacentPathTiles.Count;
      Vector2 nextEpathPos = new Vector2(0, 0);

      if (adjacentTileCount > 0) {
        int randomIndex = Random.Range(0, adjacentTileCount);
        nextEpathPos = ePath.adjacentPathTiles[randomIndex];
      }
      else {
        break;
      }

      PathTile nextEPath = new PathTile(TileType.essential, nextEpathPos,
        minBound, maxBound, gridPositions);
      //to change the start and end logic
      if (nextEPath.position.x > ePath.position.x ||
        (nextEPath.position.x == maxBound - 1 && Random.Range(0, 2) == 1)) {
        ++boundTracker;
      }
      ePath = nextEPath;
    }

    if (!gridPositions.ContainsKey(ePath.position)) {
      gridPositions.Add(ePath.position, TileType.empty);
    }

    endPos = new Vector2(ePath.position.x, ePath.position.y);
  }

  //--------------------------ASTAR------------------------------
  private void BuildAStarPath() {
    ArrayList essentialPath = new ArrayList();
    AStar aStar = new AStar();

    //start node
    int randomY = Random.Range(0, maxBound + 1);

    PathTile startTile = new PathTile(TileType.essential,
      new Vector2(0, randomY), minBound, maxBound, gridPositions);
    startPos = startTile.position;

    //end node
    int randomX = Random.Range(0, maxBound + 1);
    int endRandomY = Random.Range(0, maxBound + 1);
    
    PathTile goalTile = new PathTile(TileType.essential,
      new Vector2(maxBound, endRandomY), minBound, maxBound, gridPositions);

    essentialPath = aStar.BuildAStarPath(startTile.position, goalTile.position, 
                                          minBound, maxBound);

    Vector3 auxPos = new Vector3();
    for (int i = 0; i < essentialPath.Count; i++) {
      auxPos = (Vector3)essentialPath[i];
      gridPositions.Add(new Vector2(auxPos.x, auxPos.y), TileType.essential);
    }

    endPos = new Vector2(auxPos.x, auxPos.y);
  }

  //-------------------------RANDOM PATH-------------------------
  private void BuildRandomPath() {
    List<PathTile> patQueue = new List<PathTile>();

    foreach(KeyValuePair<Vector2, TileType> tile in gridPositions) {
      Vector2 tilePos = new Vector2(tile.Key.x, tile.Key.y);
      patQueue.Add(new PathTile(TileType.random, tilePos,
        minBound, maxBound, gridPositions));
    }

    patQueue.ForEach(delegate (PathTile tile) {
      int adjacentTileCount = tile.adjacentPathTiles.Count;

      if (adjacentTileCount != 0) {
        if (Random.Range(0, 5) == 1) {
          BuildRandomChamber(tile);
        }
        else if(Random.Range(0,5) == 1 || (tile.type == TileType.random &&
          adjacentTileCount > 1)) {

          int randomIndex = Random.Range(0, adjacentTileCount);
          Vector2 newRPathPos = tile.adjacentPathTiles[randomIndex];

          if (!gridPositions.ContainsKey(newRPathPos)) {
            gridPositions.Add(newRPathPos, TileType.empty);

            PathTile newRPath = new PathTile(TileType.random, newRPathPos,
              minBound, maxBound, gridPositions);
            patQueue.Add(newRPath);
          }
        }
      }
    });
  }

  private void BuildRandomChamber(PathTile tile) {
    int size = chamberSize, adjacentTileCount = tile.adjacentPathTiles.Count,
      randomIndex = Random.Range(0, adjacentTileCount);
    Vector2 chamberOrigin = tile.adjacentPathTiles[randomIndex];

    for(int x = (int)chamberOrigin.x; x < chamberOrigin.x + size; x++) {
      for(int y = (int)chamberOrigin.y; y < chamberOrigin.y + size; y++) {
        Vector2 chamberTilePos = new Vector2(x, y);
        if(!gridPositions.ContainsKey(chamberTilePos) && 
          chamberTilePos.x < maxBound && chamberTilePos.x > 0 &&
          chamberTilePos.y < maxBound && chamberTilePos.y > 0) {
          gridPositions.Add(chamberTilePos, TileType.empty);
        }
      }
    }
  }

	// Use this for initialization
	void Start () {
   
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
