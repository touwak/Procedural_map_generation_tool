using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public enum TileType {
  essential,
  random,
  empty,
  chest,
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
  public int minSize = 50, maxSize = 100;
  public static Vector2 startPos;
  public Vector2 endPos;
  public bool useFixedSeed = false;
  public int seed = 0;
  public int chamberSize = 3;

  int maxBound;


  public void StartDungeon(Vector2 playerPos) {

    /*if (seedDungeon) {
      TextHandle textHandle = GameManager.instance.GetTextHandle();
      //int seed = textHandle.FindDungeon(playerPos);

      //the dungeon doesn't exist
      if (seed == -1) {
        int posX = (int)playerPos.x;
        int posY = (int)playerPos.y;
        string pos = posX + "|" + posY + "|";
        //seed = Random.Range(0, 100 + 1);

        string seedCount = pos + seed.ToString() + "|";
        textHandle.WriteFile("seeds", seedCount);
      }
        Random.InitState(seed);
    }*/

    //------------------SEED------------------------------
    Random.State originalRandomState = Random.state;
    if (!useFixedSeed) {
      seed = Random.Range(0, int.MaxValue);
      seed ^= (int)System.DateTime.Now.Ticks;
      seed ^= (int)Time.unscaledTime;
      seed &= int.MaxValue;
    }
    Random.InitState(seed);

    gridPositions.Clear();
    maxBound = Random.Range(minSize, maxSize);

    BuildEssentialPath();
    BuildRandomPath();

    Random.state = originalRandomState;
  }

  //-------------------------------ESSENTIAL PATH--------------------------------
  //TODO Add the possibility to switch Start/End points between horizontal and vertical 
  private void BuildEssentialPath() {
    //first node
    int randomY = Random.Range(0, maxSize + 1);
    PathTile ePath = new PathTile(TileType.essential,
      new Vector2(0, randomY), minSize, maxSize, gridPositions);
    startPos = ePath.position;

    int boundTracker = 0;
    //when boundTracker is equal to maxBound means that we reach the last column of the right
    while (boundTracker < maxSize) {
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
        minSize, maxSize, gridPositions);
      //to change the start and end logic
      if (nextEPath.position.x > ePath.position.x ||
        (nextEPath.position.x == maxSize - 1 && Random.Range(0, 2) == 1)) {
        ++boundTracker;
      }
      ePath = nextEPath;
    }

    if (!gridPositions.ContainsKey(ePath.position)) {
      gridPositions.Add(ePath.position, TileType.empty);
    }

    endPos = new Vector2(ePath.position.x, ePath.position.y);
  }


  //-------------------------RANDOM PATH-------------------------
  private void BuildRandomPath() {
    List<PathTile> patQueue = new List<PathTile>();

    foreach(KeyValuePair<Vector2, TileType> tile in gridPositions) {
      Vector2 tilePos = new Vector2(tile.Key.x, tile.Key.y);
      patQueue.Add(new PathTile(TileType.random, tilePos,
        minSize, maxBound, gridPositions));
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
              minSize, maxBound, gridPositions);
            patQueue.Add(newRPath);
          }
        }
      }
    });
  }

  //-----------------------RANDOM CHAMBER--------------------------------
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
          
          if(Random.Range(0, 70) == 1) {

            gridPositions.Add(chamberTilePos, TileType.chest);
          }
          else {
            gridPositions.Add(chamberTilePos, TileType.empty);
          }
        }
      }
    }
  }

}
