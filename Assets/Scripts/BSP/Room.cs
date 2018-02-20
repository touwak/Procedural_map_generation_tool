using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room {

  public int xPos;
  public int yPos;
  public int width;
  public int heigth;
  public Dictionary<Vector2, TileType> roomPositions =
    new Dictionary<Vector2, TileType>();

  public Room(int posX, int posY, int widthSize, int heigthSize) {

    xPos = posX;
    yPos = posY;

    width = widthSize;
    heigth = heigthSize;

    roomPositions = CreateRoom();
  }

  private Dictionary<Vector2, TileType> CreateRoom() {
    Dictionary<Vector2, TileType> roomPos =
    new Dictionary<Vector2, TileType>();
    Vector2 pos = new Vector2(xPos, yPos);

    for (int y = 0; y <= heigth ; y++) {
      for(int x = 0; x <= width; x++) {
        roomPos.Add(pos, TileType.essential);

        pos.x++;
      }
      pos.y++;
      pos.x = xPos;
    }

    return roomPos;
  }

}
