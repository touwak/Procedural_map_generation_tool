using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room {

  private int xPos;
  private int yPos;
  private int width;
  private int heigth;
  public Dictionary<Vector2, TileType> roomPositions =
    new Dictionary<Vector2, TileType>();

  public Room(int posX, int posY, int widthSize, int heigthSize) {

    xPos = posX;
    yPos = posY;

    width = widthSize;
    heigth = heigthSize;

    roomPositions = CreateRoom();
  }

  //posible error in the positions
  private Dictionary<Vector2, TileType> CreateRoom() {
    Dictionary<Vector2, TileType> roomPos =
    new Dictionary<Vector2, TileType>();
    Vector2 pos = new Vector2(xPos, yPos);

    for (int y = 0; y <= heigth ; y++) {
      pos.y++;
      for(int x = 0; x <= width; x++) {
        pos.x++;

        roomPos.Add(pos, TileType.essential);
      }
      pos.x = xPos;
    }

    return roomPos;
  }

}
