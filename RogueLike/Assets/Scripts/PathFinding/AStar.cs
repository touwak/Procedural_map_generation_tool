using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar {


  public ArrayList BuildAStarPath(DungeonManager.PathTile start, DungeonManager.PathTile goal,
    int min, int max, Dictionary<Vector2,TileType> currentTiles) {

    ArrayList essentialPath = new ArrayList();

    DungeonManager.PathTile startTile = start, goalTile = goal;

    PriorityQueue openList = new PriorityQueue();
    openList.Push(startTile);
    startTile.totalCost = 0;
    startTile.estimatedCost = HeuristicEstimateCost(startTile, goalTile);

    PriorityQueue closedList = new PriorityQueue();
    DungeonManager.PathTile currentTile = null;

    while (openList.Length != 0) {
      currentTile = openList.First();

      //check if the current node is the goal node
      if (currentTile.position == goalTile.position) {
        return CalculatePath(currentTile);
      }

      ArrayList neighbourList = GetNeighbours(currentTile.position, min, max, currentTiles);

      for (int i = 0; i < neighbourList.Count; i++) {
        DungeonManager.PathTile neighbourTile = (DungeonManager.PathTile)neighbourList[i];

        //posible problem
        if (!closedList.Contains(neighbourTile)) {
          float cost = HeuristicEstimateCost(currentTile, neighbourTile);

          float totalCost = currentTile.totalCost + cost;
          float neighbourEstCost = HeuristicEstimateCost(neighbourTile, goalTile);

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

    if (currentTile.position != goalTile.position) {
      Debug.LogError("Goal not found");
    }

    //the complete path
    return CalculatePath(currentTile);
  }


  private static float HeuristicEstimateCost(DungeonManager.PathTile currentTile, DungeonManager.PathTile goalTile) {
    Vector2 cost = currentTile.position - goalTile.position;
    return cost.magnitude;
  }

  private static ArrayList CalculatePath(DungeonManager.PathTile tile) {
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
      pathTiles.Add(new DungeonManager.PathTile(TileType.essential,
        new Vector2(position.x, position.y + 1), minBound, maxBound, currentTiles));
    }
    if (position.x + 1 < maxBound) {
      pathTiles.Add(new DungeonManager.PathTile(TileType.essential,
        new Vector2(position.x + 1, position.y), minBound, maxBound, currentTiles));
    }
    if (position.y - 1 > minBound) {
      pathTiles.Add(new DungeonManager.PathTile(TileType.essential,
        new Vector2(position.x, position.y - 1), minBound, maxBound, currentTiles));
    }
    if (position.x - 1 >= minBound) {
      pathTiles.Add(new DungeonManager.PathTile(TileType.essential,
        new Vector2(position.x - 1, position.y), minBound, maxBound, currentTiles));
    }

    return pathTiles;
  }

}
