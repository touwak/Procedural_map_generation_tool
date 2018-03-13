using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour {

  public HexCoordinates coordinates;
  public Color color;
  public RectTransform uiRect;

  private int elevation;

  public int Elevation {
    get {
      return elevation;
    }
    set {
      elevation = value;
      Vector3 position = transform.localPosition;
      position.y = value * HexMetrics.elevationStep;
      transform.localPosition = position;

      Vector3 uiPosition = uiRect.localPosition;
      uiPosition.z = elevation * -HexMetrics.elevationStep;
      uiRect.localPosition = uiPosition;
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

}
