using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMenu : MonoBehaviour {

  int mapType = 0, minSize, maxSize;

  public void SetType(int type) {
    mapType = type;
  }

	public void GenerateMapType() {
    GameManager.instance.mode = mapType;

    if(mapType == 1) {
      GameManager.instance.EnterDungeon(minSize, maxSize);
    }
    else if(mapType == 2) {
      GameManager.instance.EnterBSPDungeon(minSize, maxSize);
    }
    GameManager.instance.RefreshGame();
  }
}
