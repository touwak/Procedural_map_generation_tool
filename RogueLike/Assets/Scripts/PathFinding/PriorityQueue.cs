using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue {

  private ArrayList tiles = new ArrayList();

  public int Length {
    get{ return tiles.Count; }
  }

  public bool Contains(object node) {
    return tiles.Contains(node);
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
