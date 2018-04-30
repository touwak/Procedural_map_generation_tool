using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {

  public SquareGrid squareGrid;
  public MeshFilter walls;
  public MeshFilter cave;
  public bool is2D;
  public Camera camera2D;
  public Camera camera3D;

  MeshCollider wallCollider;
  List<Vector3> vertices;
  List<int> triangles;

  Dictionary<int, List<Triangle>> triangleDictionary = 
    new Dictionary<int, List<Triangle>>();
  List<List<int>> outlines = new List<List<int>>();
  HashSet<int> checkedVertices = new HashSet<int>();

  /// <summary>
  /// Generate the map mesh
  /// </summary>
  /// <param name="map"> An array with the </param>
  /// <param name="squareSize"> Size of the squares that form the mesh </param>
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
    cave.mesh = mesh;

    //transform Mesh component with the vertices and triangles created in the program
    mesh.vertices = vertices.ToArray();
    mesh.triangles = triangles.ToArray();
    mesh.RecalculateNormals();

    if (is2D) {
      camera3D.gameObject.SetActive(false);
      camera2D.gameObject.SetActive(true);
      cave.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
    }
    else {
      camera2D.gameObject.SetActive(false);
      camera3D.gameObject.SetActive(true);
      cave.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    //-------Texture---------
    int tileAmount = 10;
    Vector2[] uvs = new Vector2[vertices.Count];
    for (int i = 0; i < vertices.Count; i++) {
      float percentX = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, 
        map.GetLength(0) / 2 * squareSize, vertices[i].x) * tileAmount;
      float percentY = Mathf.InverseLerp(-map.GetLength(1) / 2 * squareSize, 
        map.GetLength(1) / 2 * squareSize, vertices[i].z) * tileAmount;

      uvs[i] = new Vector2(percentX, percentY);
    }
    mesh.uv = uvs;

    //-2D-
    if (is2D) {
      if (wallCollider != null) {
        Destroy(wallCollider);
        walls.mesh.Clear();
      }     
      GenerateMesh2DColliders();
    }
    else {
      CreateWallMesh();
    }
  }


  /// <summary>
  /// Generate the inside wall in 3D mode
  /// </summary>
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

        //two triangles of every quad of the wall
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

    // 3D colliders for the wall
    if(wallCollider != null) {
      Destroy(wallCollider);
    }

    wallCollider = walls.gameObject.AddComponent<MeshCollider>();
    wallCollider.sharedMesh = wallMesh;
  }


  /// <summary>
  /// Given a certain number of points generate a mesh
  /// </summary>
  /// <param name="points"> points to generate the mesh. 
  /// params = unknow array number, you can send many params to the function, see above 
  /// </param>
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

  /// <summary>
  /// Assign an index for the vertices
  /// </summary>
  /// <param name="points"> Array of points </param>
  void AssignVertices(Node[] points) {
    for (int i = 0; i < points.Length; i++) {
      if (points[i].vertexIndex == -1) {
        points[i].vertexIndex = vertices.Count; //index 0, 4...
        vertices.Add(points[i].position); // the vertexIndex will be used in this list
      }
    }
  }

  //------------------------------------OUTLINE-------------------------------------
  /// <summary>
  /// Calculate the outline of the inside part of the map
  /// </summary>
  void CalculateMeshOutlines() {
    for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++) {
      if (!checkedVertices.Contains(vertexIndex)) {
        int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
        if (newOutlineVertex != -1) {
          checkedVertices.Add(vertexIndex);

          List<int> newOutline = new List<int> { vertexIndex };
          outlines.Add(newOutline);
          FollowOutline(newOutlineVertex, outlines.Count - 1);
          outlines[outlines.Count - 1].Add(vertexIndex);
        }
      }
    }
  }

  /// <summary>
  /// follow the vertices line recursively and adding them to outlines list
  /// </summary>
  /// <param name="vertexIndex"> new outline vertex </param>
  /// <param name="outlineIndex"> outline index </param>
  void FollowOutline(int vertexIndex, int outlineIndex) {
    outlines[outlineIndex].Add(vertexIndex);
    checkedVertices.Add(vertexIndex);
    int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

    if (nextVertexIndex != -1) {
      FollowOutline(nextVertexIndex, outlineIndex);
    }
  }

  /// <summary>
  /// Given a vertex, follow its edge line
  /// </summary>
  /// <param name="vertexIndex"> Index of the vertex to be checked </param>
  /// <returns> return the vertex that connects the two triangles </returns>
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

  /// <summary>
  /// Given two vertex return if they are in the outline
  /// </summary>
  /// <param name="vertexA"> First vertex to be compared </param>
  /// <param name="vertexB"> Second vertex to be compared </param>
  /// <returns> True if is outline or false if are not</returns>
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

    /// <summary>
    /// Check if this triangle contains a given vertex
    /// </summary>
    /// <param name="vertexIndex"> Vertex to check </param>
    /// <returns> True if the triangle contains the vertex </returns>
    public bool Contains(int vertexIndex) {
      return vertexIndex == vertexIndexA || 
        vertexIndex == vertexIndexB || 
        vertexIndex == vertexIndexC;
    }
  }

  /// <summary>
  /// Create a triangle given three nodes
  /// </summary>
  /// <param name="a"> First node </param>
  /// <param name="b"> Second node </param>
  /// <param name="c"> Third node </param>
  void CreateTriangle(Node a, Node b, Node c) {
    triangles.Add(a.vertexIndex);
    triangles.Add(b.vertexIndex);
    triangles.Add(c.vertexIndex);

    Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
    AddTriangleToDictionary(triangle.vertexIndexA, triangle);
    AddTriangleToDictionary(triangle.vertexIndexB, triangle);
    AddTriangleToDictionary(triangle.vertexIndexC, triangle);
  }

  /// <summary>
  /// Add triangles into the dictionary that will be use to create the mesh
  /// </summary>
  /// <param name="vertexIndexKey"> Index of the dictionary </param>
  /// <param name="triangle"> Triangle to add </param>
  void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle) {
    if (triangleDictionary.ContainsKey(vertexIndexKey)) {
      triangleDictionary[vertexIndexKey].Add(triangle);
    }
    else {
      List<Triangle> triangleList = new List<Triangle> { triangle };
      triangleDictionary.Add(vertexIndexKey, triangleList);
    }
  }


  //-------------------------------------SQUARE--------------------------------------

  /*
O--o--O  The corners are control nodes and the inside are nodes
|     |  The control node control the above and rigth nodes 
o     o
|     |
O--o--O

*/

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

  /// <summary>
  /// Depending on the configuration of the square genrate a different mesh
  /// </summary>
  /// <param name="square"> Square to transform </param>
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


  //--------------------------------------NODES-------------------------------------------

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


  /// <summary>
  /// Generate the outline colliders for the 2D map
  /// </summary>
  void GenerateMesh2DColliders() {

    EdgeCollider2D[] currentColliders = gameObject.GetComponents<EdgeCollider2D>();
    for (int i = 0; i < currentColliders.Length; i++) {
      Destroy(currentColliders[i]);
    }


     CalculateMeshOutlines();

    foreach(List<int> outline in outlines) {
      EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
      Vector2[] edgePoints = new Vector2[outline.Count];

      for(int i = 0; i < outline.Count; i++) {
        edgePoints[i] = new Vector2(vertices[outline[i]].x , vertices[outline[i]].z);
      }

      edgeCollider.points = edgePoints;
    }
  }
}