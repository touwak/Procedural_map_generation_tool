using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar {

  //change to 
  public ArrayList BuildAStarPath(Vector2 start, Vector2 goal, int min, int max) {
    Node startNode = new Node(start);
    Node goalNode = new Node(goal);

    PriorityQueue openList = new PriorityQueue();
    openList.Push(startNode);
    startNode.nodeTotalCost = 0;
    startNode.estimatedCost = HeuristicEstimateCost(startNode, goalNode);

    PriorityQueue closedList = new PriorityQueue();
    Node currentNode = null;

    while (openList.Length != 0) {
      currentNode = openList.First();

      //check if the current node is the goal node
      if (currentNode.position == goalNode.position) {
        return CalculatePath(currentNode);
      }

      ArrayList neighbourList = GetNeighbours(currentNode.position, min, max);

      for (int i = 0; i < neighbourList.Count; i++) {
        Node neighbourNode = (Node)neighbourList[i];

        //posible problem
        if (!closedList.Contains(neighbourNode)) {
          float cost = HeuristicEstimateCost(currentNode, neighbourNode);

          float totalCost = currentNode.nodeTotalCost + cost;
          float neighbourEstCost = HeuristicEstimateCost(neighbourNode, goalNode);

          neighbourNode.nodeTotalCost = totalCost;
          neighbourNode.parent = currentNode;
          neighbourNode.estimatedCost = totalCost + neighbourEstCost;

          if (!openList.Contains(neighbourNode)) {
            openList.Push(neighbourNode);
          }
        }
      }

      //Push the current node to the closedList
      closedList.Push(currentNode);
      //and remove it from the openList
      openList.Remove(currentNode);
    }

    if (currentNode.position != goalNode.position) {
      Debug.LogError("Goal not found");
    }

    //the complete path
    return CalculatePath(currentNode);
  }


  private static float HeuristicEstimateCost(Node currentNode, Node goalNode) {
    Vector2 cost = currentNode.position - goalNode.position;
    return cost.magnitude;
  }

  private static ArrayList CalculatePath(Node node) {
    ArrayList list = new ArrayList();
    while (node != null) {
      list.Add(node.position);
      node = node.parent;
    }

    list.Reverse();
    return list;
  }

  public ArrayList GetNeighbours(Vector2 position, int minBound, int maxBound) {
    ArrayList pathTiles = new ArrayList();

    if (position.y + 1 < maxBound) {
      pathTiles.Add(new Node (new Vector2(position.x, position.y + 1)));
    }
    if (position.x + 1 < maxBound) {
      pathTiles.Add(new Node (new Vector2(position.x + 1, position.y)));
    }
    if (position.y - 1 > minBound) {
      pathTiles.Add(new Node (new Vector2(position.x, position.y - 1)));
    }
    if (position.x - 1 >= minBound) {
      pathTiles.Add(new Node (new Vector2(position.x - 1, position.y)));
    }

    return pathTiles;
  }

}
