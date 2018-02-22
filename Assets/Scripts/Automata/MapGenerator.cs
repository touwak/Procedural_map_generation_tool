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

    MeshGenerator meshGen = GetComponent<MeshGenerator>();
    meshGen.GenerateMesh(map, 1);
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
        if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height) {
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


}
