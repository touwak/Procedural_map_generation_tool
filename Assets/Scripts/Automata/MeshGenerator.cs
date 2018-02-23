using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {

  public SquareGrid squareGrid;
  public MeshFilter walls;

  List<Vector3> vertices;
  List<int> triangles;

  Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
  List<List<int>> outlines = new List<List<int>>();
  HashSet<int> checkedVertices = new HashSet<int>();

  public void GenerateMesh(int[,] map, float squareSize) {

    triangleDictionary.Clear();
    outlines.Clear();
    checkedVertices.Clear();

    squareGrid = new SquareGrid(map, squareSize);

    vertices = new List<Vector3>();
    triangles = new List<int>();

    for (int x = 0; x < squareGrid.squares.GetLength(0); x++) {
      for (int y = 0; y < squareGrid.squares.GetLength(1); y++) {
        TriangulateSquare(squareGrid.squares[x, y]);
      }
    }

    Mesh mesh = new Mesh();
    GetComponent<MeshFilter>().mesh = mesh;

    //transform Mesh component with the vertices and triangles created in the program
    mesh.vertices = vertices.ToArray();
    mesh.triangles = triangles.ToArray();
    mesh.RecalculateNormals();

    CreateWallMesh();
  }

  void CreateWallMesh() {

    CalculateMeshOutlines();

    List<Vector3> wallVertices = new List<Vector3>();
    List<int> wallTriangles = new List<int>();
    Mesh wallMesh = new Mesh();
    float wallHeight = 5;

    foreach (List<int> outline in outlines) {
      for (int i = 0; i < outline.Count - 1; i++) {
        int startIndex = wallVertices.Count;
        wallVertices.Add(vertices[outline[i]]); // left
        wallVertices.Add(vertices[outline[i + 1]]); // right
        wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight); // bottom left
        wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * wallHeight); // bottom right

        wallTriangles.Add(startIndex + 0);
        wallTriangles.Add(startIndex + 2);
        wallTriangles.Add(startIndex + 3);

        wallTriangles.Add(startIndex + 3);
        wallTriangles.Add(startIndex + 1);
        wallTriangles.Add(startIndex + 0);
      }
    }
    wallMesh.vertices = wallVertices.ToArray();
    wallMesh.triangles = wallTriangles.ToArray();
    walls.mesh = wallMesh;
  }

  void TriangulateSquare(Square square) {
    switch (square.configuration) {
      case 0:
        break;

      // Number of points in the square: 1
      case 1: // 0001
        MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
        break;
      case 2: // 0010
        MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
        break;
      case 4: // 0100
        MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
        break;
      case 8: // 1000
        MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
        break;

      // Number of points in the square: 2
      case 3: // 0011
        MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
        break;
      case 6: // 0110
        MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
        break;
      case 9: // 1001
        MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
        break;
      case 12: // 1100
        MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
        break;
      case 5: // 0101
        MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
        break;
      case 10: // 1010
        MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
        break;

      // 3 point:
      case 7: // 0111
        MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
        break;
      case 11: // 1011
        MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
        break;
      case 13: // 1101
        MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
        break;
      case 14: // 1110
        MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
        break;

      // 4 point:
      case 15: // 1111
        MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
        checkedVertices.Add(square.topLeft.vertexIndex);
        checkedVertices.Add(square.topRight.vertexIndex);
        checkedVertices.Add(square.bottomRight.vertexIndex);
        checkedVertices.Add(square.bottomLeft.vertexIndex);
        break;
    }

  }

  //params = unknow array number, you can send many params to the function, see above
  void MeshFromPoints(params Node[] points) {
    AssignVertices(points);

    //create all the triangles of the mesh
    if (points.Length >= 3)
      CreateTriangle(points[0], points[1], points[2]);
    if (points.Length >= 4)
      CreateTriangle(points[0], points[2], points[3]);
    if (points.Length >= 5)
      CreateTriangle(points[0], points[3], points[4]);
    if (points.Length >= 6)
      CreateTriangle(points[0], points[4], points[5]);

  }

  void AssignVertices(Node[] points) {
    for (int i = 0; i < points.Length; i++) {
      if (points[i].vertexIndex == -1) {
        points[i].vertexIndex = vertices.Count; //0,1,2...
        vertices.Add(points[i].position);
      }
    }
  }

  void CreateTriangle(Node a, Node b, Node c) {
    triangles.Add(a.vertexIndex);
    triangles.Add(b.vertexIndex);
    triangles.Add(c.vertexIndex);

    Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
    AddTriangleToDictionary(triangle.vertexIndexA, triangle);
    AddTriangleToDictionary(triangle.vertexIndexB, triangle);
    AddTriangleToDictionary(triangle.vertexIndexC, triangle);
  }

  void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle) {
    if (triangleDictionary.ContainsKey(vertexIndexKey)) {
      triangleDictionary[vertexIndexKey].Add(triangle);
    }
    else {
      List<Triangle> triangleList = new List<Triangle> { triangle };
      triangleDictionary.Add(vertexIndexKey, triangleList);
    }
  }

  void CalculateMeshOutlines() {

    for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++) {
      if (!checkedVertices.Contains(vertexIndex)) {
        int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
        if (newOutlineVertex != -1) {
          checkedVertices.Add(vertexIndex);

          List<int> newOutline = new List<int> { vertexIndex};
          outlines.Add(newOutline);
          FollowOutline(newOutlineVertex, outlines.Count - 1);
          outlines[outlines.Count - 1].Add(vertexIndex);
        }
      }
    }
  }

  void FollowOutline(int vertexIndex, int outlineIndex) {
    outlines[outlineIndex].Add(vertexIndex);
    checkedVertices.Add(vertexIndex);
    int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

    if (nextVertexIndex != -1) {
      FollowOutline(nextVertexIndex, outlineIndex);
    }
  }

  int GetConnectedOutlineVertex(int vertexIndex) {
    List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

    for (int i = 0; i < trianglesContainingVertex.Count; i++) {
      Triangle triangle = trianglesContainingVertex[i];

      for (int j = 0; j < 3; j++) {
        int vertexB = triangle[j];
        if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB)) {
          if (IsOutlineEdge(vertexIndex, vertexB)) {
            return vertexB;
          }
        }
      }
    }

    return -1;
  }

  bool IsOutlineEdge(int vertexA, int vertexB) {
    List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
    int sharedTriangleCount = 0;

    for (int i = 0; i < trianglesContainingVertexA.Count; i++) {
      if (trianglesContainingVertexA[i].Contains(vertexB)) {
        sharedTriangleCount++;
        if (sharedTriangleCount > 1) {
          break;
        }
      }
    }
    return sharedTriangleCount == 1;
  }

  //--------------------------------------TRIANGLE-----------------------------------

  struct Triangle {
    public int vertexIndexA;
    public int vertexIndexB;
    public int vertexIndexC;
    int[] vertices;

    public Triangle(int a, int b, int c) {
      vertexIndexA = a;
      vertexIndexB = b;
      vertexIndexC = c;

      vertices = new int[3];
      vertices[0] = a;
      vertices[1] = b;
      vertices[2] = c;
    }

    public int this[int i] {//To access like array
      get {
        return vertices[i];
      }
    }


    public bool Contains(int vertexIndex) {
      return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
    }
  }


 //-------------------------------------SQUARE--------------------------------------

  //each square forms a "quad" 
  public class Square {

    public ControlNode topLeft, topRight, bottomRight, bottomLeft;
    public Node centreTop, centreRight, centreBottom, centreLeft;
    public int configuration;

    public Square(ControlNode _topLeft, ControlNode _topRight, 
      ControlNode _bottomRight, ControlNode _bottomLeft) {

      topLeft = _topLeft;
      topRight = _topRight;
      bottomRight = _bottomRight;
      bottomLeft = _bottomLeft;

      centreTop = topLeft.right;
      centreRight = bottomRight.above;
      centreBottom = bottomLeft.right;
      centreLeft = bottomLeft.above;

      //in binary 0000
      if (topLeft.active)
        configuration += 8;       //1000
      if (topRight.active)
        configuration += 4;       //0100
      if (bottomRight.active)
        configuration += 2;       //0010
      if (bottomLeft.active)
        configuration += 1;       //0001
    }

  }

  public class SquareGrid {
    public Square[,] squares;

    public SquareGrid(int[,] map, float squareSize) {
      int nodeCountX = map.GetLength(0);
      int nodeCountY = map.GetLength(1);
      float mapWidth = nodeCountX * squareSize;
      float mapHeight = nodeCountY * squareSize;

      ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

      for (int x = 0; x < nodeCountX; x++) {
        for (int y = 0; y < nodeCountY; y++) {
          //-mapWidth / 2 = far left | squareSize / 2 = to center around the square 
          Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2,
            0, -mapHeight / 2 + y * squareSize + squareSize / 2);
          
          //1 is active
          controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize);
        }
      }

      squares = new Square[nodeCountX - 1, nodeCountY - 1];
      for (int x = 0; x < nodeCountX - 1; x++) {
        for (int y = 0; y < nodeCountY - 1; y++) {
          squares[x, y] = new Square(controlNodes[x, y + 1],
            controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
        }
      }

    }
  }

  //--------------------------------------NODES-------------------------------------------

  /*
 O--o--O  The corners are control nodes and the inside are nodes
 |     |  The control node control the nodes at the sides
 o     o
 |     |
 O--o--O

   */

  public class Node {
    public Vector3 position;
    public int vertexIndex = -1;

    public Node(Vector3 _pos) {
      position = _pos;
    }
  }

  public class ControlNode : Node {

    public bool active;
    public Node above, right;

    public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos) {
      active = _active;
      above = new Node(position + Vector3.forward * squareSize / 2f);
      right = new Node(position + Vector3.right * squareSize / 2f);
    }

  }
}