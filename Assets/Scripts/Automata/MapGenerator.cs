using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour {

  public int width;
  public int height;

  public int seed;
  public bool useRandomSeed;

  [Range(44, 55)]
  public int randomFillPercent;

  int[,] map;

  private void Start() {

    GenerateMap();
  }

  /// <summary>
  /// Generate the map
  /// </summary>
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

  /// <summary>
  /// Fill randomly the grid with 1 (wall) or 0 (empty or playable)
  /// </summary>
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
          rnd = Random.Range(0, 100 + 1);
          map[x, y] = (rnd < randomFillPercent) ? 1 : 0;
        }
      }
    }
  }

  /// <summary>
  /// Go over the grid checking the neighbours of each tile, 
  /// if this neightbours 4 or more are walls switch this tile into wall or vice versa
  /// </summary>
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

  /// <summary>
  /// Given a position in the grid returns the the number of sorrounding walls.
  /// </summary>
  /// <param name="gridX"> X position on the grid </param>
  /// <param name="gridY"> Y position on the grid </param>
  /// <returns> number of surrounding walls</returns>
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

  /// <summary>
  /// Returns the tiles that compose a region
  /// </summary>
  /// <param name="startX"> X position inside the region </param>
  /// <param name="startY"> Y position inside the region </param>
  /// <returns></returns>
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

  /// <summary>
  /// Given a position check if is inside the map
  /// </summary>
  /// <param name="x"> X axis </param>
  /// <param name="y"> Y axis </param>
  /// <returns></returns>
  bool IsInMapeRange(int x, int y) {
    return x >= 0 && x < width && y >= 0 && y < height;
  }

  /// <summary>
  /// Given a tile type create a list with all the regions of this type
  /// </summary>
  /// <param name="tileType"> type of tile </param>
  /// <returns> a list with all the regions of a specific type</returns>
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

  /// <summary>
  /// Process the early generated map and erase the small rock regions,
  /// erase the small cave regions and connect all the rooms.
  /// </summary>
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

    // order the rooms and mark bigger like main
    survivingRooms.Sort();
    survivingRooms[0].mainRoom = true;
    survivingRooms[0].isAccesibleFromMainRoom = true;

    ConnectClosestRooms(survivingRooms);
  }

  //----------------------------------------ROOM-------------------------------------
  class Room : IComparable<Room> {
    public List<Coord> tiles;
    public List<Coord> edgeTiles;
    public List<Room> connectedRooms;
    public int roomSize;
    public bool isAccesibleFromMainRoom;
    public bool mainRoom;

    public Room() {

    }

    public Room(List <Coord> roomTiles, int [,] map) {
      tiles = roomTiles;
      roomSize = tiles.Count;
      connectedRooms = new List<Room>();

      // detect edge tiles
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

    // set accesible from main room true and every connected room to this
    public void SetAccesibleFromMainRoom() {
      if (!isAccesibleFromMainRoom) {
        isAccesibleFromMainRoom = true;

        foreach(Room connectedRoom in connectedRooms) {
          connectedRoom.SetAccesibleFromMainRoom();
        }
      }
    }

    // connect rooms and set access to the main room
    public static void ConnectRooms(Room roomA, Room roomB) {

      if (roomA.isAccesibleFromMainRoom) {
        roomB.SetAccesibleFromMainRoom();
      }
      else if (roomB.isAccesibleFromMainRoom) {
        roomA.SetAccesibleFromMainRoom();
      }

      roomA.connectedRooms.Add(roomB);
      roomB.connectedRooms.Add(roomA);
    }

    // return if is conected with other room
    public bool IsConnected(Room otherRoom) {
      return connectedRooms.Contains(otherRoom);
    }

    // to sort lists
    public int CompareTo(Room otherRoom) {
      return otherRoom.roomSize.CompareTo(roomSize);
    }
  }


  void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false) {

    List<Room> roomListA = new List<Room>(); 
    List<Room> roomListB = new List<Room>(); 

    if (forceAccessibilityFromMainRoom) {
      foreach(Room room in allRooms) {
        if (room.isAccesibleFromMainRoom) {
          roomListB.Add(room);  // accesible from main room
        }
        else{
          roomListA.Add(room);  // not accesible from main room
        }
      }
    }
    else {
      roomListA = allRooms;
      roomListB = allRooms;
    }

    int bestDistance = 0;
    Coord bestTileA = new Coord();
    Coord bestTileB = new Coord();
    Room bestRoomA = new Room();
    Room bestRoomB = new Room();
    bool possibleConectionFound = false;

    foreach(Room roomA in roomListA) {
      if (!forceAccessibilityFromMainRoom) {
        possibleConectionFound = false;

        if (roomA.connectedRooms.Count > 0) {
          continue;
        }
      }

      foreach (Room roomB in roomListB) {
        if(roomA == roomB || roomA.IsConnected(roomB)) {
          continue;
        }
        
        // compare the edge tiles of both rooms and connect to the nearest
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
      // if have found a conection with oter room conect both
      if (possibleConectionFound && !forceAccessibilityFromMainRoom) {
        CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
      }
    }
    // check again for better conections 
    if (possibleConectionFound && forceAccessibilityFromMainRoom) {
      CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
      ConnectClosestRooms(allRooms, true);
    }

    if (!forceAccessibilityFromMainRoom) {
      ConnectClosestRooms(allRooms, true);
    }

  }

  void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB) {
    Room.ConnectRooms(roomA, roomB);

    List<Coord> line = GetLine(tileA, tileB);
    foreach(Coord c in line) {
      DrawCircle(c, 1);
    }
  }

  // Create the path to conect the rooms / radius is the size of the path
  void DrawCircle(Coord c, int r) { //TODO editable
    for (int x = -r; x <= r; x++) {
      for (int y = -r; y <= r; y++) {
        if(x * x + y * y <= r * r) { // check if is inside the circle
          int drawX = c.tileX + x;
          int drawY = c.tileY + y;

          if (IsInMapeRange(drawX, drawY)) {
            map[drawX, drawY] = 0;
          }
        }
      }
    }
  }

  // Create the line that joins the caves
  List<Coord> GetLine(Coord from, Coord to) {

    List<Coord> line = new List<Coord>();

    int x = from.tileX;
    int y = from.tileY;

    int dx = to.tileX - from.tileX;
    int dy = to.tileY - from.tileY;

    bool inverted = false;
    int step = Math.Sign(dx);
    int gradientStep = Math.Sign(dy);

    int longest = Math.Abs(dx);
    int shortest = Math.Abs(dy);

    if(longest < shortest) {
      inverted = true;
      longest = Math.Abs(dy);
      shortest = Math.Abs(dx);

      step = Math.Sign(dy);
      gradientStep = Math.Sign(dx);
    }

    int gradientAccumulation = longest / 2;
    for(int i = 0; i < longest; i++) {
      line.Add(new Coord(x, y));

      if (inverted) {
        y += step;
      }
      else {
        x += step;
      }

      gradientAccumulation += shortest;
      if(gradientAccumulation >= longest) {
        if (inverted) {
          x += gradientStep;
        }
        else {
          y += gradientStep;
        }
        gradientAccumulation -= longest;
      }
    }

    return line;
  }

  Vector3 CoordToWorldPoint(Coord tile) {
    return new Vector3(-width / 2 + .5f + tile.tileX, 2, -height / 2 + .5f + tile.tileY);
  }

}
