using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Leaf {

  private const uint MIN_LEAF_SIZE = 6;

  public int y, x, width, height;
  public Leaf leftChild, rightChild;
  public Room room;
  //public Corridor corridor;

  public Leaf(int posX, int posY, int sizeWidth, int sizeHeight) {
    x = posX;
    y = posY;
    width = sizeWidth;
    height = sizeHeight;
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
      if(max < MIN_LEAF_SIZE) {
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
      return true;
    } 
  }	

  public void CreateRooms() {
    if(leftChild != null || rightChild != null) {

      if(leftChild != null){
        leftChild.CreateRooms();
      }
      if(rightChild != null) {
        rightChild.CreateRooms();
      }
    }
    else {
      Vector2 roomSize;
      Vector2 roomPos;

      //size between 3x3 and the size of the leaf - 2
      roomSize = new Vector2(Random.Range(3, width - 2), Random.Range(3, height - 2));
      //pos posible error
      roomPos = new Vector2(Random.Range(1, width - roomSize.x - 1),
        Random.Range(1, height - roomSize.y - 1));

      room = new Room( x + (int)roomPos.x, y + (int)roomPos.y, (int)roomSize.x, (int)roomSize.y);
    }
  }



}
