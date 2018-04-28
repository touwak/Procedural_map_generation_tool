using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room {

  public int xPos;
  public int yPos;
  public int width;
  public int heigth;
  public int left;
  public int right;
  public int top;
  public int bottom;

  public Dictionary<Vector2, TileType> roomPositions =
    new Dictionary<Vector2, TileType>();

  /// <summary>
  /// Initialize the room
  /// </summary>
  /// <param name="posX"> position in X axis </param>
  /// <param name="posY"> position in Y axis </param>
  /// <param name="widthSize"> width size of the room </param>
  /// <param name="heigthSize"> height size of the room </param>
  public Room(int posX, int posY, int widthSize, int heigthSize) {

    xPos = posX;
    yPos = posY;

    width = widthSize;
    heigth = heigthSize;

    left = xPos;
    right = xPos + width - 1;
    top = yPos + heigth - 1;
    bottom = yPos;

    roomPositions = CreateRoom();
  }

  /// <summary>
  /// Create the room
  /// </summary>
  /// <returns> a dictionary with the tiles position and their type </returns>
  private Dictionary<Vector2, TileType> CreateRoom() {
    Dictionary<Vector2, TileType> roomPos =  new Dictionary<Vector2, TileType>();

    Vector2 pos = new Vector2(xPos, yPos);

    for (int y = 0; y < heigth ; y++) {
      for(int x = 0; x < width; x++) {

        roomPos.Add(pos, TileType.essential);
        pos.x++;
      }
      pos.y++;
      pos.x = xPos;
    }

    return roomPos;
  }

}
