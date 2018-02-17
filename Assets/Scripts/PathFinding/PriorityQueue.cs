using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue {

  private ArrayList nodes = new ArrayList();

  public int Length {
    get{ return nodes.Count; }
  }

  public bool Contains(object node) {
    Node other = (Node)node, current;
    for(int i = 0; i < nodes.Count; i++) {
      current = (Node)nodes[i];
      if(current.position == other.position) {
        return true;
      }
    }

    return false;
  }

  public Node First() {
    if (nodes.Count > 0) {
      return (Node)nodes[0];
    }
    return null;
  }

  public void Push(Node tile) {
    nodes.Add(tile);
    nodes.Sort();
  }

  public void Remove(Node tile) {
    nodes.Remove(tile);
    nodes.Sort();
  }
}
