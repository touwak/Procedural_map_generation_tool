using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMapGenerator : MonoBehaviour {

  public HexGrid grid;

  int cellCount;


  public void GenerateMap(int x, int z) {
    cellCount = x * z;
    grid.CreateMap(x, z);
    RaiseTerrain(7);
    
  }

  void RaiseTerrain(int chunkSize) {
    for (int i = 0; i < chunkSize; i++) {
      GetRandomCell().TerrainTypeIndex = 1;
    }
  }

  HexCell GetRandomCell() {
    return grid.GetCell(Random.Range(0, cellCount));
  }
}
