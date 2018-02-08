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
  public class PathTile : IComparable {
    public TileType type;
    public Vector2 position;
    public List<Vector2> adjacentPathTiles;
    public float totalCost; //G
    public float estimatedCost; //H
    public PathTile parent;
    public ArrayList neighbours;

    public PathTile(TileType t, Vector2 p, int min, int max,
      Dictionary<Vector2, TileType> currentTiles) {
      type = t;
      position = p;
      adjacentPathTiles = GetAdjacentPath(min, max, currentTiles);
      totalCost = 0.0f;
      estimatedCost = 0.0f;
      parent = null;
      neighbours = new ArrayList();
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

    //public ArrayList GetNeighbours(int minBound, int maxBound,
    //  Dictionary<Vector2, TileType> currentTiles) {
    //  ArrayList pathTiles = new ArrayList();

    //  if (position.y + 1 < maxBound) {
    //    pathTiles.Add(new PathTile(TileType.essential, 
    //      new Vector2(position.x, position.y + 1), minBound, maxBound, currentTiles));
    //  }
    //  if (position.x + 1 < maxBound) {
    //    pathTiles.Add(new PathTile(TileType.essential, 
    //      new Vector2(position.x + 1, position.y), minBound, maxBound, currentTiles));
    //  }
    //  if (position.y - 1 > minBound) {
    //    pathTiles.Add(new PathTile(TileType.essential, 
    //      new Vector2(position.x, position.y - 1), minBound, maxBound, currentTiles));
    //  }
    //  if (position.x - 1 >= minBound) {
    //    pathTiles.Add(new PathTile(TileType.essential, 
    //      new Vector2(position.x - 1, position.y), minBound, maxBound, currentTiles));
    //  }

    //  return pathTiles;
    //}


    //sort method of Arraylist use CompareTo
    public int CompareTo(object obj) {
      PathTile tile = (PathTile)obj;

      //negative value means object comes before this in the sort order
      if (estimatedCost < tile.estimatedCost) {
        return -1;
      }
      //positive value means object comes after this in the sort order
      if (estimatedCost > tile.estimatedCost) {
        return 1;
      }

      return 0;
    }
  }

  public Dictionary<Vector2, TileType> gridPositions =
    new Dictionary<Vector2, TileType>();
  public int minBound = 0, maxBound;
  public static Vector2 startPos;
  public Vector2 endPos;

  public void StartDungeon() {
    gridPositions.Clear();
    maxBound = Random.Range(50, 101);

    BuildAStarPath();
    //BuildEssentialPath();
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

  private void BuildAStarPath() {
    ArrayList essentialPath = new ArrayList();

    //end node
    int randomX = Random.Range(0, maxBound + 1);
    int endRandomY = Random.Range(0, maxBound + 1);
    PathTile endPath = new PathTile(TileType.essential,
      new Vector2(randomX, endRandomY), minBound, maxBound, gridPositions);   

    //start node
    int randomY = Random.Range(0, maxBound + 1);
    PathTile ePath = new PathTile(TileType.essential,
      new Vector2(0, randomY), minBound, maxBound, gridPositions);
    startPos = ePath.position;
    ePath.totalCost = 0.0f;
    ePath.estimatedCost = HeuristicEstimateCost(ePath, endPath);

    PriorityQueue closedList = new PriorityQueue();
    PriorityQueue openList = new PriorityQueue();
    openList.Push(ePath);

    PathTile currentTile = null;
    while(openList.Length != 0) {
      currentTile = openList.First();

      //check if the current node is the goal node
      if(currentTile.position == endPath.position) {
        essentialPath = CalculatePath(currentTile);
      }

      ArrayList neighbourList = GetNeighbours(currentTile.position, minBound, maxBound, gridPositions);
      for(int i = 0; i < neighbourList.Count; i++) {
        PathTile neighbourTile = (PathTile)neighbourList[i];

        if (!closedList.Contains(neighbourTile)) {
          float cost = HeuristicEstimateCost(currentTile, neighbourTile);

          float totalCost = currentTile.totalCost + cost;
          float neighbourEstCost = HeuristicEstimateCost(neighbourTile, endPath);

          neighbourTile.totalCost = totalCost;
          neighbourTile.parent = currentTile;
          neighbourTile.estimatedCost = totalCost + neighbourEstCost;

          if (!openList.Contains(neighbourTile)) {
            openList.Push(neighbourTile);

          }
        }
      }

      //Push the current node to the closedList
      closedList.Push(currentTile);
      //and remove it from the openList
      openList.Remove(currentTile);
    }

    if(currentTile.position != endPath.position) {
      //error;
    }

    //the complete path
    essentialPath = CalculatePath(currentTile);

    for(int i = 0; i < essentialPath.Count; i++) {
      gridPositions.Add((Vector2)essentialPath[i] , TileType.essential);
    }

  }

  private static float HeuristicEstimateCost(PathTile currentTile, PathTile goalTile) {
    Vector2 cost = currentTile.position - goalTile.position;
    return cost.magnitude;
  }

  private static ArrayList CalculatePath(PathTile tile) {
    ArrayList list = new ArrayList();
    while (tile != null) {
      list.Add(tile.position);
      tile = tile.parent;
    }

    list.Reverse();
    return list;
  }

  public ArrayList GetNeighbours(Vector2 position, int minBound, int maxBound,
     Dictionary<Vector2, TileType> currentTiles) {
    ArrayList pathTiles = new ArrayList();

    if (position.y + 1 < maxBound) {
      pathTiles.Add(new PathTile(TileType.essential,
        new Vector2(position.x, position.y + 1), minBound, maxBound, currentTiles));
    }
    if (position.x + 1 < maxBound) {
      pathTiles.Add(new PathTile(TileType.essential,
        new Vector2(position.x + 1, position.y), minBound, maxBound, currentTiles));
    }
    if (position.y - 1 > minBound) {
      pathTiles.Add(new PathTile(TileType.essential,
        new Vector2(position.x, position.y - 1), minBound, maxBound, currentTiles));
    }
    if (position.x - 1 >= minBound) {
      pathTiles.Add(new PathTile(TileType.essential,
        new Vector2(position.x - 1, position.y), minBound, maxBound, currentTiles));
    }

    return pathTiles;
  }

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
    int chamberSize = 3, adjacentTileCount = tile.adjacentPathTiles.Count,
      randomIndex = Random.Range(0, adjacentTileCount);
    Vector2 chamberOrigin = tile.adjacentPathTiles[randomIndex];

    for(int x = (int)chamberOrigin.x; x < chamberOrigin.x + chamberSize; x++) {
      for(int y = (int)chamberOrigin.y; y < chamberOrigin.y + chamberSize; y++) {
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
