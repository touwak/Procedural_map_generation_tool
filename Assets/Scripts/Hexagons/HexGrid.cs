﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

  public int width = 6;
  public int height = 6;

  public Color defaultColor = Color.white;
  public Color touchedColor = Color.magenta;

  public HexCell cellPrefab;
  public Text cellLabelPrefab;

  Canvas gridCanvas;

  private HexCell[] cells;
  private HexMesh hexMesh;

  private void Awake() {
    cells = new HexCell[height * width];
    gridCanvas = GetComponentInChildren<Canvas>();
    hexMesh = GetComponentInChildren<HexMesh>();

    for (int z = 0, i = 0; z < height; z++){
      for(int x = 0; x < width; x++) {
        CreateCell(x, z, i++);
      }
    }
  }

  private void Start() {
    hexMesh.Triangulate(cells);
  }

  private void Update() {
    if (Input.GetMouseButton(0)) {
      HandleInput();
    }
  }

  void CreateCell(int x, int z, int i) {
    Vector3 position;
    position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f) ;
    position.y = 0f;
    position.z = z * (HexMetrics.outerRadius * 1.5f);

    HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
    cell.transform.SetParent(transform, false);
    cell.transform.localPosition = position;
    cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
    cell.color = defaultColor;

    // label
    Text label = Instantiate<Text>(cellLabelPrefab);
    label.rectTransform.SetParent(gridCanvas.transform, false);
    label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
    label.text = cell.coordinates.ToStringOnSeparateLines();

  }

  void HandleInput() {
    Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    if(Physics.Raycast(inputRay, out hit)) {
      //TouchCell(hit.point);
    }
  }

  public void ColorCell(Vector3 position, Color color) {
    position = transform.InverseTransformPoint(position);
    HexCoordinates coordinates = HexCoordinates.FromPosition(position);
    int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
    HexCell cell = cells[index];
    cell.color = color;
    hexMesh.Triangulate(cells);
  }


}
