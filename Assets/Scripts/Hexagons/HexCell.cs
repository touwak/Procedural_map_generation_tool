using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour {

  public HexCoordinates coordinates;
  public RectTransform uiRect;
  public HexGridChunk chunk;

  private int elevation = int.MinValue;
  private bool hasIncomingRiver, hasOutgoingRiver;
  private HexDirection incomingRiver, outgoingRiver;

  public int Elevation {
    get {
      return elevation;
    }
    set {
      if(elevation == value) {
        return;
      }

      elevation = value;
      Vector3 position = transform.localPosition;
      position.y = value * HexMetrics.elevationStep;
      position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) *
        HexMetrics.elevationPerturbStrengh;
      transform.localPosition = position;

      Vector3 uiPosition = uiRect.localPosition;
      uiPosition.z = -position.y;
      uiRect.localPosition = uiPosition;

      //check that the rivers flow in the correct direction when
      //the elevation of a cell change
      if(hasOutgoingRiver &&
        elevation < GetNeighbor(outgoingRiver).elevation) {
        RemoveOutgoingRiver();
      }

      if(hasIncomingRiver && 
        elevation > GetNeighbor(incomingRiver).elevation) {
        RemoveIncomingRiver();
      }

      Refresh();
    }
  }

  public Color Color {
    get {
      return color;
    }
    set {
      if(color == value) {
        return;
      }
      color = value;
      Refresh();
    }
  }
  Color color;

  public Vector3 Position {
    get {
      return transform.localPosition;
    }
  }

  [SerializeField]
  private HexCell[] neightbors;

  public HexCell GetNeighbor(HexDirection direction) {
    return neightbors[(int)direction];
  }

  public void SetNeighbor(HexDirection direction, HexCell cell) {
    neightbors[(int)direction] = cell;
    cell.neightbors[(int)direction.Opposite()] = this;
  }

  public HexEdgeType GetEdgeType(HexDirection direction) {
    return HexMetrics.GetEdgeType(elevation, neightbors[(int)direction].elevation);
  }

  public HexEdgeType GetEdgeType(HexCell otherCell) {
    return HexMetrics.GetEdgeType(elevation, otherCell.Elevation);
  }

  void Refresh() {
    if (chunk) {
      chunk.Refresh();
      for(int i = 0; i < neightbors.Length; i++) {
        HexCell neighbor = neightbors[i];
        if(neighbor != null && neighbor.chunk != chunk) {
          neighbor.chunk.Refresh();
        }
      }
    }
  }

  //-----------------RIVER------------------
  public bool HasIncomingRiver {
    get {
      return hasIncomingRiver;
    }
  }

  public bool HasOutgoingRiver {
    get {
      return hasOutgoingRiver;
    }
  }

  public HexDirection IncomingRiver {
    get {
      return incomingRiver;
    }
  }

  public HexDirection OutgoingRiver {
    get {
      return outgoingRiver;
    }
  }

  public bool HasRiver {
    get {
      return hasIncomingRiver || HasOutgoingRiver;
    }
  }

  public bool HasRiverBeginOrEnd {
    get {
      return hasIncomingRiver != hasOutgoingRiver;
    }
  }

  public bool HasRiverThroughEdge(HexDirection direction) {
    return hasIncomingRiver && incomingRiver == direction ||
      hasOutgoingRiver && outgoingRiver == direction;
  }

  public float StreamBedY {
    get {
      return (elevation + HexMetrics.streamBedElevationOffset) *
        HexMetrics.elevationStep;
    }
  }

  //-------REMOVE RIVERS--------

  public void RemoveOutgoingRiver() {
    if (!hasOutgoingRiver) {
      return;
    }

    hasOutgoingRiver = false;
    RefreshSelfOnly();

    HexCell neigbour = GetNeighbor(outgoingRiver);
    neigbour.hasIncomingRiver = false;
    neigbour.RefreshSelfOnly();
  }

  void RefreshSelfOnly() {
    chunk.Refresh();
  }

  public void RemoveIncomingRiver() {
    if (!hasIncomingRiver) {
      return;
    }

    hasIncomingRiver = false;
    RefreshSelfOnly();

    HexCell neighbor = GetNeighbor(incomingRiver);
    neighbor.hasOutgoingRiver = false;
    neighbor.RefreshSelfOnly();
  }

  public void RemoveRiver() {
    RemoveIncomingRiver();
    RemoveOutgoingRiver();
  }

  //---------ADD RIVERS----------

  public void SetOutgoingRiver(HexDirection direction) {
    if(hasOutgoingRiver && outgoingRiver == direction) {
      return;
    }

    HexCell neigbour = GetNeighbor(direction);
    if(!neigbour || elevation < neigbour.elevation) {
      return;
    }

    RemoveOutgoingRiver();
    if(hasIncomingRiver && incomingRiver == direction) {
      RemoveIncomingRiver();
    }

    hasOutgoingRiver = true;
    outgoingRiver = direction;
    RefreshSelfOnly();

    neigbour.RemoveIncomingRiver();
    neigbour.hasIncomingRiver = true;
    neigbour.incomingRiver = direction.Opposite();
    neigbour.RefreshSelfOnly();
  }

}
