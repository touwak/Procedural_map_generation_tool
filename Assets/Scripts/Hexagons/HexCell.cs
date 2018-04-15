using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class HexCell : MonoBehaviour {

  public HexCoordinates coordinates;
  public RectTransform uiRect;
  public HexGridChunk chunk;

  private int elevation = int.MinValue;

  //river
  private bool hasIncomingRiver, hasOutgoingRiver;
  private HexDirection incomingRiver, outgoingRiver;

  //roads
  [SerializeField]
  bool[] roads;

  //water
  private int waterLevel;

  //features
  private int urbanLevel, farmLevel, plantLevel;

  // wall
  private bool walled;

  // special
  int specialIndex;

  // save
  int terrainTypeIndex;

  public int Elevation {
    get {
      return elevation;
    }
    set {
      if(elevation == value) {
        return;
      }

      elevation = value;
      RefreshPosition();

      //check that the rivers flow in the correct direction when
      //the elevation of a cell change
      ValidateRivers();

      //if the differnce is bigger than the allowed remove the roads
      for(int i = 0; i < roads.Length; i++) {
        if(roads[i] && GetElevationDifference((HexDirection)i)>1) {
          SetRoad(i, false);
        }
      }

      Refresh();
    }
  }

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

  public int GetElevationDifference(HexDirection direction) {
    int difference = elevation - GetNeighbor(direction).elevation;
    return difference >= 0 ? difference : -difference;
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

  void RefreshPosition() {
    Vector3 position = transform.localPosition;
    position.y = elevation * HexMetrics.elevationStep;
    position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) *
      HexMetrics.elevationPerturbStrengh;
    transform.localPosition = position;

    Vector3 uiPosition = uiRect.localPosition;
    uiPosition.z = -position.y;
    uiRect.localPosition = uiPosition;
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

  public HexDirection RiverBeginOrEndDirection {
    get {
      return hasIncomingRiver ? incomingRiver : outgoingRiver;
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

  public float RiverSurfaceY {
    get {
      return (elevation + 
        HexMetrics.waterElevationOffset) *
        HexMetrics.elevationStep;
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

  bool IsValidRiverDestination(HexCell neighbour) {
    return neighbour && (
      elevation >= neighbour.elevation || waterLevel == neighbour.elevation);
  }

  void ValidateRivers() {
    if(hasOutgoingRiver &&
      !IsValidRiverDestination(GetNeighbor(outgoingRiver))) {
      RemoveOutgoingRiver();
    }
    if(hasIncomingRiver &&
      !GetNeighbor(incomingRiver).IsValidRiverDestination(this)) {
      RemoveIncomingRiver();
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
    if(!IsValidRiverDestination(neigbour)) {
      return;
    }

    RemoveOutgoingRiver();
    if(hasIncomingRiver && incomingRiver == direction) {
      RemoveIncomingRiver();
    }

    hasOutgoingRiver = true;
    outgoingRiver = direction;
    specialIndex = 0;

    neigbour.RemoveIncomingRiver();
    neigbour.hasIncomingRiver = true;
    neigbour.incomingRiver = direction.Opposite();
    neigbour.SpecialIndex = 0;

    SetRoad((int)direction, false);
  }

  //-----------------ROADS---------------------------

  public bool HasRoadThroughEdge(HexDirection direction) {
    return roads[(int)direction];
  }

  public bool HasRoads {
    get {
      for(int i = 0; i < roads.Length; i++) {
        if (roads[i]) {
          return true;
        }
      }

      return false;
    }
  }

  void SetRoad(int index, bool state) {
    roads[index] = state;
    neightbors[index].roads[(int)((HexDirection)index).Opposite()] = state;
    neightbors[index].RefreshSelfOnly();
    RefreshSelfOnly();
  }

  //------REMOVE ROADS--------

  public void RemoveRoads() {
    for(int i = 0; i < roads.Length; i++) {
      if (roads[i]) {
        SetRoad(i, false);
      }
    }
  }

  //------ADDING ROADS--------

  public void AddRoad(HexDirection direction) {
    if (!roads[(int)direction] && !HasRiverThroughEdge(direction) &&
      !IsSpecial && !GetNeighbor(direction).IsSpecial &&
      GetElevationDifference(direction) <= 1) {
      SetRoad((int)direction, true);
    }
  }


  //-------------------WATER----------------------------
  public int WaterLevel {
    get {
      return waterLevel;
    }
    set {
      if(waterLevel == value) {
        return;
      }
      waterLevel = value;
      ValidateRivers();
      Refresh();
    }
  }

  public bool IsUnderWater {
    get {
      return waterLevel > elevation;
    }
  }

  public float WaterSurfaceY {
    get {
      return
        (waterLevel + HexMetrics.waterElevationOffset) *
        HexMetrics.elevationStep;
    }
  }

  //-----------------FEATURES--------------------------

  public int UrbanLevel {
    get {
      return urbanLevel;
    }
    set {
      if(urbanLevel != value) {
        urbanLevel = value;
        RefreshSelfOnly();
      }
    }
  }

  public int FarmLevel {
    get {
      return farmLevel;
    }
    set {
      if(farmLevel != value) {
        farmLevel = value;
        RefreshSelfOnly();
      }
    }
  }

  public int PlantLevel {
    get {
      return plantLevel;
    }
    set {
      if(farmLevel != value) {
        plantLevel = value;
        RefreshSelfOnly();
      }
    }
  }

  //-------------------------WALLS----------------------------

  public bool Walled {
    get {
      return walled;
    }
    set {
      if(walled != value) {
        walled = value;
        RefreshSelfOnly();
      }
    }
  }

  //----------------------SPECIALS--------------------------

  public int SpecialIndex {
    get {
      return specialIndex;
    }
    set {
      if (specialIndex != value && !HasRiver) {
        specialIndex = value;
        RemoveRoads();
        RefreshSelfOnly();
      }
    }
  }

  public bool IsSpecial {
    get {
      return specialIndex > 0;
    }
  }

  //--------------------SAVE-------------------------------
  
  public int TerrainTypeIndex {
    get {
      return terrainTypeIndex;
    }
    set {
      if(terrainTypeIndex != value) {
        terrainTypeIndex = value;
        Refresh();
      }
    }
  }

  public void Save(BinaryWriter writer) {
    writer.Write((byte)terrainTypeIndex);
    writer.Write((byte)elevation);
    writer.Write((byte)waterLevel);
    writer.Write((byte)urbanLevel);
    writer.Write((byte)farmLevel);
    writer.Write((byte)plantLevel);
    writer.Write((byte)specialIndex);
    writer.Write(walled);

    // rivers
    if (hasIncomingRiver) {
      writer.Write((byte)(incomingRiver + 128));
    }
    else {
      writer.Write((byte)0);
    }

    if (hasOutgoingRiver) {
      writer.Write((byte)(outgoingRiver + 128));
    }
    else {
      writer.Write((byte)0);
    }

    // roads
    int roadFlags = 0;
    for (int i = 0; i < roads.Length; i++) {
      if (roads[i]) {
        roadFlags |= 1 << i;
      }
    }
    writer.Write((byte)roadFlags);
  }

  public void Load(BinaryReader reader) {
    terrainTypeIndex = reader.ReadByte();
    elevation = reader.ReadByte();
    RefreshPosition();
    waterLevel = reader.ReadByte();
    urbanLevel = reader.ReadByte();
    farmLevel = reader.ReadByte();
    plantLevel = reader.ReadByte();
    specialIndex = reader.ReadByte();
    walled = reader.ReadBoolean();

    // rivers
    byte riverData = reader.ReadByte();
    if (riverData >= 128) {
      hasIncomingRiver = true;
      incomingRiver = (HexDirection)(riverData - 128);
    }
    else {
      hasIncomingRiver = false;
    }

    riverData = reader.ReadByte();
    if (riverData >= 128) {
      hasOutgoingRiver = true;
      outgoingRiver = (HexDirection)(riverData - 128);
    }
    else {
      hasOutgoingRiver = false;
    }

    // roads
    int roadFlags = reader.ReadByte();
    for (int i = 0; i < roads.Length; i++) {
      roads[i] = (roadFlags & (1 << i)) != 0;
    }
  }


}
