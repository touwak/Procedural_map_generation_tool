﻿using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour {

  public bool useCollider, useColors, useUVCordinates, useUV2Cordinates;
  public bool useTerrainTypes;


  private Mesh hexMesh;
  private MeshCollider meshCollider;
  [NonSerialized] List<Vector3> vertices, terrainTypes; //dont save the list during recompiles
  [NonSerialized] List<int> triangles;
  [NonSerialized] List<Color> colors;
  [NonSerialized] List<Vector2> uvs, uv2s;

  private void Awake() {
    GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();

    if (useCollider) {
      meshCollider = gameObject.AddComponent<MeshCollider>();
    }
    hexMesh.name = "HexMesh";

  }


  //add the triangle information to the lists
  public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
    int vertexIndex = vertices.Count;
    vertices.Add(HexMetrics.Perturb(v1));
    vertices.Add(HexMetrics.Perturb(v2));
    vertices.Add(HexMetrics.Perturb(v3));
    triangles.Add(vertexIndex);
    triangles.Add(vertexIndex + 1);
    triangles.Add(vertexIndex + 2);
  }

  public void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3) {
    int vertexIndex = vertices.Count;
    vertices.Add(v1);
    vertices.Add(v2);
    vertices.Add(v3);
    triangles.Add(vertexIndex);
    triangles.Add(vertexIndex + 1);
    triangles.Add(vertexIndex + 2);
  }

  public void AddTriangleColor(Color color) {
    colors.Add(color);
    colors.Add(color);
    colors.Add(color);
  }

  //a different color for each vertex
  public void AddTriangleColor(Color color1, Color color2, Color color3) {
    colors.Add(color1);
    colors.Add(color2);
    colors.Add(color3);
  }


  public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
    int vertexIndex = vertices.Count;
    vertices.Add(HexMetrics.Perturb(v1));
    vertices.Add(HexMetrics.Perturb(v2));
    vertices.Add(HexMetrics.Perturb(v3));
    vertices.Add(HexMetrics.Perturb(v4));
    triangles.Add(vertexIndex);
    triangles.Add(vertexIndex + 2);
    triangles.Add(vertexIndex + 1);
    triangles.Add(vertexIndex + 1);
    triangles.Add(vertexIndex + 2);
    triangles.Add(vertexIndex + 3);
  }

  public void AddQuadUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
    int vertexIndex = vertices.Count;
    vertices.Add(v1);
    vertices.Add(v2);
    vertices.Add(v3);
    vertices.Add(v4);
    triangles.Add(vertexIndex);
    triangles.Add(vertexIndex + 2);
    triangles.Add(vertexIndex + 1);
    triangles.Add(vertexIndex + 1);
    triangles.Add(vertexIndex + 2);
    triangles.Add(vertexIndex + 3);
  }

  public void AddQuadColor(Color color1, Color color2, Color color3, Color color4) {
    colors.Add(color1);
    colors.Add(color2);
    colors.Add(color3);
    colors.Add(color4);
  }

  public void AddQuadColor(Color color1, Color color2) {
    colors.Add(color1);
    colors.Add(color1);
    colors.Add(color2);
    colors.Add(color2);
  }

  public void AddQuadColor(Color color) {
    colors.Add(color);
    colors.Add(color);
    colors.Add(color);
    colors.Add(color);
  }

  //----------UVS----------------
  public void AddTriangleUV(Vector2 uv1, Vector2 uv2, Vector2 uv3) {
    uvs.Add(uv1);
    uvs.Add(uv2);
    uvs.Add(uv3);
  }

  public void AddQuadUV(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4) {
    uvs.Add(uv1);
    uvs.Add(uv2);
    uvs.Add(uv3);
    uvs.Add(uv4);
  }
  public void AddQuadUV(float uMin, float uMax, float vMin, float vMax) {
    uvs.Add(new Vector2(uMin, vMin));
    uvs.Add(new Vector2(uMax, vMin));
    uvs.Add(new Vector2(uMin, vMax));
    uvs.Add(new Vector2(uMax, vMax));
  }

  public void AddTriangleUV2(Vector2 uv1, Vector2 uv2, Vector2 uv3) {
    uv2s.Add(uv1);
    uv2s.Add(uv2);
    uv2s.Add(uv3);
  }

  public void AddQuadUV2(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4) {
    uv2s.Add(uv1);
    uv2s.Add(uv2);
    uv2s.Add(uv3);
    uv2s.Add(uv4);
  }
  public void AddQuadUV2(float uMin, float uMax, float vMin, float vMax) {
    uv2s.Add(new Vector2(uMin, vMin));
    uv2s.Add(new Vector2(uMax, vMin));
    uv2s.Add(new Vector2(uMin, vMax));
    uv2s.Add(new Vector2(uMax, vMax));
  }

  //------------------TERRAIN---------------------------

  public void AddTriangleTerrainTypes(Vector3 types) {
    terrainTypes.Add(types);
    terrainTypes.Add(types);
    terrainTypes.Add(types);
  }

  public void AddQuadTerrainTypes(Vector3 types) {
    terrainTypes.Add(types);
    terrainTypes.Add(types);
    terrainTypes.Add(types);
    terrainTypes.Add(types);
  }

  public void Clear() {
    hexMesh.Clear();
    vertices = ListPool<Vector3>.Get();
    triangles = ListPool<int>.Get();

    if (useColors) {
      colors = ListPool<Color>.Get();
    }

    if (useUVCordinates) {
      uvs = ListPool<Vector2>.Get();
    }

    if (useUV2Cordinates) {
      uv2s = ListPool<Vector2>.Get();
    }

    if (useTerrainTypes) {
      terrainTypes = ListPool<Vector3>.Get();
    }
  }

  public void Apply() {
    hexMesh.SetVertices(vertices);
    ListPool<Vector3>.Add(vertices);

    if (useColors) {
      hexMesh.SetColors(colors);
      ListPool<Color>.Add(colors);
    }

    if (useUVCordinates) {
      hexMesh.SetUVs(0, uvs);
      ListPool<Vector2>.Add(uvs);
    }

    if (useUV2Cordinates) {
      hexMesh.SetUVs(0, uv2s);
      ListPool<Vector2>.Add(uv2s);
    }

    if (useTerrainTypes) {
      hexMesh.SetUVs(2, terrainTypes);
      ListPool<Vector3>.Add(terrainTypes);
    }

    hexMesh.SetTriangles(triangles, 0);
    ListPool<int>.Add(triangles);
    hexMesh.RecalculateNormals();

    if (useCollider) {
      meshCollider.sharedMesh = hexMesh;
    }
  }

}
