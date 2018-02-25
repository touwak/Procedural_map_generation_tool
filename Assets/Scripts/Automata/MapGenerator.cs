using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour {

  public int width;
  public int height;

  public int seed;
  public bool useRandomSeed;

  [Range(0, 100)]
  public int randomFillPercent;

  int[,] map;

  private void Start() {

    GenerateMap();
  }

  private void Update() {
    if (Input.GetMouseButtonDown(0)) {
      GenerateMap();
    }
  }

  public void GenerateMap() {
    map = new int[width, height];
    RandomFillMap();

    for (int i = 0; i < 5; i++) { //change the value
      SmoothMap();
    }

    ProcessMap();

    int borderSize = 1;
   // borderSize * 2 = both sides
    int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

    for (int x = 0; x < borderedMap.GetLength(0); x++) {
      for (int y = 0; y < borderedMap.GetLength(1); y++) {
        if(x >= borderSize && x < width + borderSize &&
          y >= borderSize && y < height + borderSize) {
          borderedMap[x, y] = map[x - borderSize, y - borderSize];
        }
        else {// border
          borderedMap[x, y] = 1;
        }

      }
    }

    MeshGenerator meshGen = GetComponent<MeshGenerator>();
    meshGen.GenerateMesh(borderedMap, 1);
  }

  void RandomFillMap() {
    if (useRandomSeed) {
      Random.InitState((int)Time.time);
    }

    int rnd;
    // 1 wall / 0 empty
    for(int x = 0; x < width; x++) {
      for(int y = 0; y < height; y++) {
        if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
          map[x, y] = 1;
        }
        else {
          rnd = Random.Range(0, 100);
          map[x, y] = (rnd < randomFillPercent) ? 1 : 0;
        }
      }
    }
  }

  void SmoothMap() {
    for (int x = 0; x < width; x++) {
      for (int y = 0; y < height; y++) {
        int neighbourWallTiles = GetSorroundingWallCount(x, y);

        if(neighbourWallTiles > 4) {
          map[x, y] = 1;
        }
        else if( neighbourWallTiles < 4) {
          map[x, y] = 0;
        }
      }
    }
  }

  int GetSorroundingWallCount(int gridX, int gridY) {
    int wallCount = 0;
    for( int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) {
      for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {
        if (IsInMapeRange(neighbourX, neighbourY) ) {
          if (neighbourX != gridX || neighbourY != gridY) {
            wallCount += map[neighbourX, neighbourY];
          }
        }
        else {
          wallCount++;
        }
      }
    }

    return wallCount;
  }

//-------------------------------------COORDS AND REGIONS----------------------------------
  struct Coord {
    public int tileX;
    public int tileY;

    public Coord(int x, int y) {
      tileX = x;
      tileY = y;
    }
  }

  List<Coord> GetRegionTiles(int startX, int startY) {
    List<Coord> tiles = new List<Coord>();
    int[,] mapFlags = new int[width, height];
    int tileType = map[startX, startY];

    Queue<Coord> queue = new Queue<Coord>();
    queue.Enqueue(new Coord(startX, startY));
    mapFlags[startX, startY] = 1;

    while(queue.Count > 0) {
      Coord tile = queue.Dequeue();
      tiles.Add(tile);

      for(int x = tile.tileX - 1; x <= tile.tileX + 1; x++) {
        for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++) {
          if (IsInMapeRange(x, y) && (y == tile.tileY || x == tile.tileX)) {
            if(mapFlags[x, y] == 0 && map[x, y] == tileType) {
              mapFlags[x, y] = 1;
              queue.Enqueue(new Coord(x, y));
            }
          }
        }
      }
    }

    return tiles;
  }

  bool IsInMapeRange(int x, int y) {
    return x >= 0 && x < width && y >= 0 && y < height;
  }

  //return the regions of "rock" 
  List<List<Coord>> GetRegions(int tileType) {
    List<List<Coord>> regions = new List<List<Coord>>();
    int[,] mapFlags = new int[width, height];

    for (int x = 0; x < width; x++) {
      for (int y = 0; y < height; y++) {
        if(mapFlags[x,y] == 0 && map[x,y] == tileType) {
          List<Coord> newRegion = GetRegionTiles(x, y);
          regions.Add(newRegion);

          foreach(Coord tile in newRegion) {
            mapFlags[tile.tileX, tile.tileY] = 1;
          }
        }
      }
    }

    return regions;
  }

  
  void ProcessMap() {

    //erase the small regions of "rock"
    List<List<Coord>> wallRegions = GetRegions(1);
    int wallThresholdSize = 50; //editable

    foreach (List<Coord> wallRegion in wallRegions) {
      if(wallRegion.Count < wallThresholdSize) {
        foreach(Coord tile in wallRegion) {
          map[tile.tileX, tile.tileY] = 0;
        }
      }
    }

    //erase the small "caves"
    List<List<Coord>> caveRegions = GetRegions(0);
    int caveThresholdSize = 50; //editable

    List<Room> survivingRooms = new List<Room>();

    foreach (List<Coord> caveRegion in caveRegions) {
      if (caveRegion.Count < caveThresholdSize) {
        foreach (Coord tile in caveRegion) {
          map[tile.tileX, tile.tileY] = 1;
        }
      }
      else {
        survivingRooms.Add(new Room(caveRegion, map));
      }
    }

    ConnectClosestRooms(survivingRooms);
  }

  //----------------------------------------ROOM-------------------------------------
  class Room {
    public List<Coord> tiles;
    public List<Coord> edgeTiles;
    public List<Room> connectedRooms;
    public int roomSize;

    public Room() {

    }

    public Room(List <Coord> roomTiles, int [,] map) {
      tiles = roomTiles;
      roomSize = tiles.Count;
      connectedRooms = new List<Room>();

      edgeTiles = new List<Coord>();
      foreach(Coord tile in tiles) {
        for(int x = tile.tileX - 1; x < tile.tileX + 1; x++) {
          for (int y = tile.tileY - 1; y < tile.tileY + 1; y++) {
            if(x == tile.tileX || y == tile.tileY) {
              if(map[x,y] == 1) {
                edgeTiles.Add(tile);
              }
            }
          }
        }
      }
    }

    public static void ConnectRooms(Room roomA, Room roomB) {
      roomA.connectedRooms.Add(roomB);
      roomB.connectedRooms.Add(roomA);
    }

    public bool IsConnected(Room otherRoom) {
      return connectedRooms.Contains(otherRoom);
    }
  }


  void ConnectClosestRooms(List<Room> allRooms) {
    int bestDistance = 0;
    Coord bestTileA = new Coord();
    Coord bestTileB = new Coord();
    Room bestRoomA = new Room();
    Room bestRoomB = new Room();
    bool possibleConectionFound = false;

    foreach(Room roomA in allRooms) {
      possibleConectionFound = false;

      foreach (Room roomB in allRooms) {
        if(roomA == roomB) {
          continue;
        }
        if (roomA.IsConnected(roomB)) {
          possibleConectionFound = false;
          break;
        }

        for(int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++) {
          for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++) {
            Coord tileA = roomA.edgeTiles[tileIndexA];
            Coord tileB = roomB.edgeTiles[tileIndexB];
            int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) +
              Mathf.Pow(tileA.tileY - tileB.tileY, 2));

            if(distanceBetweenRooms < bestDistance || !possibleConectionFound) {
              bestDistance = distanceBetweenRooms;
              possibleConectionFound = true;
              bestTileA = tileA;
              bestTileB = tileB;
              bestRoomA = roomA;
              bestRoomB = roomB;
            }
          }
        }
      }

      if (possibleConectionFound) {
        CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
      }
    }



  }

  void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB) {
    Room.ConnectRooms(roomA, roomB);
    Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.green, 100);
  }

  Vector3 CoordToWorldPoint(Coord tile) {

    return new Vector3(-width / 2 + .5f + tile.tileX, 2, -height / 2 + .5f + tile.tileY);
  }

}
