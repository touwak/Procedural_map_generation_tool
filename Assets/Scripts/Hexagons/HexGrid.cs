﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class HexGrid : MonoBehaviour {

  public int cellCountX = 20, cellCountZ = 15;

  private int chunkCountX, chunkCountZ;

  public HexCell cellPrefab;
  public Text cellLabelPrefab;

  public Texture2D noiseSource;

  public HexGridChunk chunkPrefab;

  private HexCell[] cells;
  private HexGridChunk[] chunks;

  // hash grid
  public int seed;

  private void Awake() {

    HexMetrics.noiseSource = noiseSource;
    HexMetrics.InitializeHashGrid(seed);

    CreateMap(cellCountX, cellCountZ);
  }

  private void OnEnable() {
    if (!HexMetrics.noiseSource) {
      HexMetrics.noiseSource = noiseSource;
      HexMetrics.InitializeHashGrid(seed);
    }

  }

  private void Start() {

  }

  private void Update() {
    if (Input.GetMouseButton(0)) {
      HandleInput();
    }
  }

  //----------------CHUNKS-----------------------

  void CreateChunks() {
    chunks = new HexGridChunk[chunkCountX * chunkCountZ];

    for(int z = 0, i = 0; z < chunkCountZ; z++) {
      for(int x = 0; x < chunkCountX; x++) {
        HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
        chunk.transform.SetParent(transform);
      }
    }
  }


  //-----------------CELLS---------------------

  void CreateCells() {
    cells = new HexCell[cellCountZ * cellCountX];

    for (int z = 0, i = 0; z < cellCountZ; z++) {
      for (int x = 0; x < cellCountX; x++) {
        CreateCell(x, z, i++);
      }
    }
  }

  void CreateCell(int x, int z, int i) {
    Vector3 position;
    position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f) ;
    position.y = 0f;
    position.z = z * (HexMetrics.outerRadius * 1.5f);

    HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
    cell.transform.localPosition = position;
    cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

    // --------------NEIGHBORS-------------------
    if(x > 0) {
      cell.SetNeighbor(HexDirection.W, cells[i - 1]);
    }
    if(z > 0) {

      // even
      if((z & 1) == 0) {
        cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);

        if(x > 0) {
          cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
        }
      }
      else { // odd
        cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
        if(x < cellCountX - 1) {
          cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
        }
      }
    }

    // label
    Text label = Instantiate<Text>(cellLabelPrefab);
    label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
    label.text = cell.coordinates.ToStringOnSeparateLines();

    cell.uiRect = label.rectTransform;
    cell.Elevation = 0;

    AddCellToChunk(x, z, cell);
  }

  void AddCellToChunk(int x, int z, HexCell cell) {
    int chunkX = x / HexMetrics.chunkSizeX;
    int chunkZ = z / HexMetrics.chunkSizeZ;
    HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

    int localX = x - chunkX * HexMetrics.chunkSizeX;
    int localZ = z - chunkZ * HexMetrics.chunkSizeZ;

    chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
  }

  public HexCell GetCell(Vector3 position) {
    position = transform.InverseTransformPoint(position);
    HexCoordinates coordinates = HexCoordinates.FromPosition(position);
    int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;

    return cells[index];
  }

  public HexCell GetCell(HexCoordinates coordinates) {
    int z = coordinates.Z;
    if(z < 0 || z >= cellCountZ) {
      return null;
    }

    int x = coordinates.X + z / 2;
    if (x < 0 || x >= cellCountX) {
      return null;
    }

    return cells[x + z * cellCountX];
  }

  public HexCell GetCell(int xOffset, int zOffset) {
    return cells[xOffset + zOffset * cellCountX];
  }

  public HexCell GetCell(int cellIndex) {
    return cells[cellIndex];
  }

  void HandleInput() {
    Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    if (Physics.Raycast(inputRay, out hit)) {
      //TouchCell(hit.point);
    }
  }

  public void ShowUI(bool visible) {
    for(int i = 0; i < chunks.Length; i++) {
      chunks[i].ShowUI(visible);
    }
  }
  //-----------------------SAVE----------------------------

  public void Save(BinaryWriter writer) {
    // map size
    writer.Write(cellCountX);
    writer.Write(cellCountZ);

    for (int i = 0; i < cells.Length; i++) {
      cells[i].Save(writer);
    }
  }

  public void Load(BinaryReader reader, int header) {
    int x = 20, z = 15;
    if (header >= 1) {
      x = reader.ReadInt32();
      z = reader.ReadInt32();
    }

    if (x != cellCountX || z != cellCountZ) {
      if (!CreateMap(x, z)) {
        return;
      }
    }

    for (int i = 0; i < cells.Length; i++) {
      cells[i].Load(reader);
    }

    for (int i = 0; i < chunks.Length; i++) {
      chunks[i].Refresh();
    }
  }

  //----------------------CREATE MAP-------------------------

  public bool CreateMap(int x, int z) {
    if (
      x <= 0 || x % HexMetrics.chunkSizeX != 0 ||
      z <= 0 || z % HexMetrics.chunkSizeZ != 0
    ) {
      Debug.LogError("Unsupported map size.");
      return false;
    }

    if (chunks != null) {
      for (int i = 0; i < chunks.Length; i++) {
        Destroy(chunks[i].gameObject);
      }
    }
    cellCountX = x;
    cellCountZ = z;
    chunkCountX = cellCountX / HexMetrics.chunkSizeX;
    chunkCountZ = cellCountZ / HexMetrics.chunkSizeZ;

    CreateChunks();
    CreateCells();

    return true;
  }

}
