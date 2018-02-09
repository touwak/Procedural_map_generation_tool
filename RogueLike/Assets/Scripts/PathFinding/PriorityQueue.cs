using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue {

  private ArrayList tiles = new ArrayList();

  public int Length {
    get{ return tiles.Count; }
  }

  public bool Contains(object node) {
    DungeonManager.PathTile other = (DungeonManager.PathTile)node;

    for (int i = 0; i < tiles.Count; i++) {
      DungeonManager.PathTile actual = (DungeonManager.PathTile)tiles[i];
      if (actual.position == other.position) {
        return true;
      }
    }
    return false;
  }

  public DungeonManager.PathTile First() {
    if (tiles.Count > 0) {
      return (DungeonManager.PathTile)tiles[0];
    }
    return null;
  }

  public void Push(DungeonManager.PathTile tile) {
    tiles.Add(tile);
    tiles.Sort();
  }

  public void Remove(DungeonManager.PathTile tile) {
    tiles.Remove(tile);
    tiles.Sort();
  }
}
