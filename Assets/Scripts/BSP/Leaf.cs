using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Leaf {

  private const uint MIN_LEAF_SIZE = 5;

  public int  x, y, width, height;
  public Leaf leftChild, rightChild;
  public Room room;
  //public Corridor corridor;

  public Leaf(int startX, int startY, int widthSize, int heightSize) {
    x = startX;
    width = widthSize;
    y = startY;
    height = heightSize;
  }

  public bool Split() {

    //check if the leaf is already splited
    if(leftChild != null || rightChild != null) {
      return false;
    }
    else {

      //determine the direction of the split
      bool splitH = Random.Range(0, 2) == 1;
      if(width > height && width / height >= 1.25f) {
        splitH = false;
      }
      else if(height > width && height / width >= 1.25f) {
        splitH = true;
      }

      int max = (splitH ? height : width) - (int)MIN_LEAF_SIZE;
      if (max < MIN_LEAF_SIZE) {
        return false; //is too small
      }

      //determina where is the split
      int split = Random.Range((int)MIN_LEAF_SIZE, max);

      if (splitH) {


        leftChild = new Leaf(x, y, width, split);
        rightChild = new Leaf(x, y + split, width, height - split);
      }
      else {
        leftChild = new Leaf(x, y, split, height);
        rightChild = new Leaf(x + split, y, width - split, height);
      }

      leftChild.Split();
      rightChild.Split();

      return true;
    } 
  }	

  public void CreateRooms(Dictionary<Vector2, TileType> grid) {
    if(leftChild != null || rightChild != null) {

      if(leftChild != null){
        leftChild.CreateRooms(grid);
      }
      if(rightChild != null) {
        rightChild.CreateRooms(grid);
      }
      if(leftChild != null && rightChild != null) {
        CreateCorridor(leftChild.GetRoom() , rightChild.GetRoom(), grid);
      }
    }
    else {
      Vector2 roomSize;
      Vector2 roomPos;

      //size between 3x3 and the size of the leaf - 2
      roomSize = new Vector2(Random.Range(3, width - 2), Random.Range(3, height - 2));
      roomPos = new Vector2(Random.Range(1, width - roomSize.x - 1),
        Random.Range(1, height - roomSize.y - 1));

      room = new Room( x + (int)roomPos.x, y + (int)roomPos.y, (int)roomSize.x, (int)roomSize.y);

      DicToDic(room.roomPositions, grid);
    }
  }

  public Room GetRoom() {
    if(room != null) {
      return room;
    }
    else {
      Room leftRoom = new Room(0, 0, 0, 0);
      Room rightRoom = new Room(0, 0, 0, 0);

      if(leftChild != null) {
        leftRoom = leftChild.GetRoom();
      }
      if(rightChild != null) {
        rightRoom = rightChild.GetRoom();
      }

      if(leftChild == null && rightChild == null) {
        return null;
      }
      else if(rightRoom == null) {
        return leftRoom;
      }
      else if(leftRoom == null) {
        return rightRoom;
      }
      else if(Random.value > .5) {
        return leftRoom;
      }
      else {
        return rightRoom;
      }
    }
  }

  public void CreateCorridor(Room left, Room right, Dictionary<Vector2, TileType> grid) {

    Dictionary<Vector2, TileType> corridors =
    new Dictionary<Vector2, TileType>();

    Vector2 pointOne = new Vector2(Random.Range(left.left + 1, left.right - 2),
      Random.Range(left.top + 1, left.bottom - 2));
    Vector2 pointTwo = new Vector2(Random.Range(right.left + 1, right.right - 2),
      Random.Range(right.top + 1, right.bottom - 2));

    float w = pointTwo.x - pointOne.x;
    float h = pointTwo.y - pointOne.y;

    if(w < 0) {

      if(h < 0) {

        if(Random.value < 0.5) {
          DicToDic(CreateTiles((int)pointTwo.x, (int)pointOne.y, (int)Mathf.Abs(w), 1), corridors);
          DicToDic(CreateTiles((int)pointTwo.x, (int)pointTwo.y, 1, (int)Mathf.Abs(h)), corridors);
        }
        else {
          DicToDic(CreateTiles((int)pointTwo.x, (int)pointTwo.y, (int)Mathf.Abs(w), 1), corridors);
          DicToDic(CreateTiles((int)pointOne.x, (int)pointTwo.y, 1, (int)Mathf.Abs(h)), corridors);
        }
      }
      else if( h > 0) {
        if(Random.value < 0.5) {
          DicToDic(CreateTiles((int)pointTwo.x, (int)pointOne.y, (int)Mathf.Abs(w), 1), corridors);
          DicToDic(CreateTiles((int)pointTwo.x, (int)pointOne.y, 1, (int)Mathf.Abs(h)), corridors);
        }
        else {
          DicToDic(CreateTiles((int)pointTwo.x, (int)pointTwo.y, (int)Mathf.Abs(w), 1), corridors);
          DicToDic(CreateTiles((int)pointOne.x, (int)pointOne.y, 1, (int)Mathf.Abs(h)), corridors);
        }
      }
      else { // H == 0
        DicToDic(CreateTiles((int)pointTwo.x, (int)pointTwo.y, (int)Mathf.Abs(w), 1), corridors);
      }
    }
    else if( w > 0) {
      if(h < 0) {
        if(Random.value < 0.5) {
          DicToDic(CreateTiles((int)pointOne.x, (int)pointTwo.y, (int)Mathf.Abs(w), 1), corridors);
          DicToDic(CreateTiles((int)pointOne.x, (int)pointTwo.y, 1, (int)Mathf.Abs(h)), corridors);
        }
        else {
          DicToDic(CreateTiles((int)pointOne.x, (int)pointOne.y, (int)Mathf.Abs(w), 1), corridors);
          DicToDic(CreateTiles((int)pointTwo.x, (int)pointTwo.y, 1, (int)Mathf.Abs(h)), corridors);
        }
      }
      else if( h > 0) {
        if(Random.value < 0.5) {
          DicToDic(CreateTiles((int)pointOne.x, (int)pointOne.y, (int)Mathf.Abs(w), 1), corridors);
          DicToDic(CreateTiles((int)pointTwo.x, (int)pointOne.y, 1, (int)Mathf.Abs(h)), corridors);
        }
        else {
          DicToDic(CreateTiles((int)pointOne.x, (int)pointTwo.y, (int)Mathf.Abs(w), 1), corridors);
          DicToDic(CreateTiles((int)pointOne.x, (int)pointOne.y, 1, (int)Mathf.Abs(h)), corridors);
        }
      }
      else { // H == 0
        DicToDic(CreateTiles((int)pointOne.x, (int)pointOne.y, (int)Mathf.Abs(w), 1), corridors);
      }
    }
    else { // W == 0
      if(h < 0) {
        DicToDic(CreateTiles((int)pointTwo.x, (int)pointTwo.y, 1, (int)Mathf.Abs(h)), corridors);
      }
      else if(h > 0) {
        DicToDic(CreateTiles((int)pointOne.x, (int)pointOne.y, 1, (int)Mathf.Abs(h)), corridors);
      }
    }

    DicToDic(corridors, grid);
  }

  public Dictionary<Vector2, TileType> CreateTiles(int posX, int posY, int widthSize, int heigthSize) {
    Dictionary<Vector2, TileType> tilePos =
    new Dictionary<Vector2, TileType>();

    Vector2 pos = new Vector2(posX, posY);

    for (int y = 0; y < heigthSize; y++) {
      for (int x = 0; x < widthSize; x++) {

        tilePos.Add(pos, TileType.essential);
        pos.x++;
      }
      pos.y++;
      pos.x = posX;
    }

    return tilePos;
  }

  void DicToDic(Dictionary<Vector2, TileType> origin, Dictionary<Vector2, TileType> destine) {
    foreach (KeyValuePair<Vector2, TileType> pos in origin) {
      if (!destine.ContainsKey(pos.Key)) {
        destine.Add(pos.Key, pos.Value);
      }
      else {
        Vector2 n = pos.Key;
        Debug.LogError(string.Format("Repeted Tile {0} , {1}", n.x, n.y));
      }
    }
  }

}
