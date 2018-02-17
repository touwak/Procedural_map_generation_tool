using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//heritage from Icomparable for override CompareTo
public class Node : IComparable {

  public float nodeTotalCost; //G
  public float estimatedCost; //H
  public bool bObstacle;
  public Node parent;
  public Vector3 position;

  public Node() {
    estimatedCost = 0.0f;
    nodeTotalCost = 1.0f;
    bObstacle = false;
    parent = null;
  }

  public Node(Vector3 pos) {
    estimatedCost = 0.0f;
    nodeTotalCost = 1.0f;
    bObstacle = false;
    parent = null;
    position = pos;
  }

  public void MarkAsObstacle() {
    bObstacle = true;
  }

  //sort method of Arraylist use CompareTo
  public int CompareTo(object obj) {
    Node node = (Node)obj;
    
    //negative value means object comes before this in the sort order
    if(estimatedCost < node.estimatedCost) {
      return -1;
    }
    //positive value means object comes after this in the sort order
    if (estimatedCost > node.estimatedCost) {
      return 1;
    }

    return 0;
  }
}
