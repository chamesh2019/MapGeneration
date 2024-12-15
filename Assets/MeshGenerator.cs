using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public SquareGrid squareGrid;

    public MeshFilter wallMeshFilter;

    List<Vector3> verticies;
    List<int> triangles;

    Dictionary<int, List<Triangle>> trianglesDict = new Dictionary<int, List<Triangle>> ();
    List<List<int>> outlines = new List<List<int>> ();
    HashSet<int> checkedVertices = new HashSet<int> ();
    public void GenerateMesh(int[,] map, float squareSize) {
        outlines.Clear ();
        checkedVertices.Clear ();

        squareGrid = new SquareGrid(map, squareSize);
        verticies = new List<Vector3>();
        triangles = new List<int>();


        for (int x = 0; x < squareGrid.squares.GetLength(0); x++) {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++) {
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh; 

        mesh.vertices = verticies.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        CreateWallMesh();
    }

    void CreateWallMesh() {
        GenerateMeshOutlines();

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();

        Mesh wallMesh = new Mesh();
        float wallHeight = 5;

        foreach (List<int> outline in outlines) {
            for (int i = 0; i < outline.Count - 1; i++) {
                int startIndex = wallVertices.Count;
                wallVertices.Add(verticies[outline[i]]); // left vertex
                wallVertices.Add(verticies[outline[i + 1]]); // right vertex 
                wallVertices.Add(verticies[outline[i]] - Vector3.up * wallHeight); // bottom left vertex
                wallVertices.Add(verticies[outline[i + 1]] - Vector3.up * wallHeight); // bottom right vertex

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
        //wallMesh.RecalculateNormals(); // Recalculate normals for proper lighting

        if (wallMeshFilter != null) {
            wallMeshFilter.mesh = wallMesh; // Assign the generated wall mesh to the wallMeshFilter
        } else {
            Debug.LogError("Wall Mesh Filter is not assigned.");
        }
    }

    void TriangulateSquare(Square square) {
        switch (square.configuration) {
            case 0:
                break;

            // 1 point active
            case 1:
                MeshFromPoints(square.rightMid, square.bottomRight, square.bottomMid);
                break;
            case 2:
                MeshFromPoints(square.bottomMid, square.bottomLeft, square.leftMid);
                break;
            case 4:
                MeshFromPoints(square.topMid, square.topRight, square.rightMid);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.topMid, square.leftMid);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(square.rightMid, square.bottomRight, square.bottomLeft, square.leftMid);
                break;
            case 5:
                MeshFromPoints(square.topMid, square.topRight, square.bottomRight, square.bottomMid);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.topMid, square.bottomMid, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.rightMid, square.leftMid);
                break;
            case 6:
                MeshFromPoints(square.topMid, square.topRight, square.rightMid, square.bottomMid, square.bottomLeft, square.leftMid);
                break;
            case 9 :
                MeshFromPoints(square.topLeft, square.topMid, square.rightMid, square.bottomRight, square.bottomMid, square.leftMid);
                break;

            // 3 point:
            case 7:
                MeshFromPoints(square.topMid, square.topRight, square.bottomRight, square.bottomLeft, square.leftMid);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.topMid, square.rightMid, square.bottomRight, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.rightMid, square.bottomMid, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomMid, square.leftMid);
                break;

            // 4 point:
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                checkedVertices.Add(square.topLeft.vertexIndex);
                checkedVertices.Add(square.topRight.vertexIndex);
                checkedVertices.Add(square.bottomRight.vertexIndex);
                checkedVertices.Add(square.bottomLeft.vertexIndex);
                break;
        }
    }

    void MeshFromPoints(params Node[] points) {
        AssignVerticies(points);

        if (points.Length >= 3) {
            CreateTriangle(points[0], points[1], points[2]);
        }
        if (points.Length >= 4) {
            CreateTriangle(points[0], points[2], points[3]);
        }
        if (points.Length >= 5) {
            CreateTriangle(points[0], points[3], points[4]);
        }
        if (points.Length >= 6) {
            CreateTriangle(points[0], points[4], points[5]);
        }
    }

    void AssignVerticies(Node[] points) {
        for (int i = 0; i < points.Length; i++) {
            if (points[i].vertexIndex == -1) { 
                points[i].vertexIndex = verticies.Count;
                verticies.Add(points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c) {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDict(triangle.vertexIndexA, triangle);
        AddTriangleToDict(triangle.vertexIndexB, triangle);
        AddTriangleToDict(triangle.vertexIndexC, triangle);
    }

    void AddTriangleToDict(int vertexIndexKey, Triangle triangle) {
        if (trianglesDict.ContainsKey(vertexIndexKey)) {
            trianglesDict[vertexIndexKey].Add(triangle);
        } else {
            List<Triangle> list = new List<Triangle>();
            list.Add(triangle);
            trianglesDict.Add(vertexIndexKey, list);
        }
    }

    void GenerateMeshOutlines() {

        for (int vertexIndex = 0; vertexIndex < verticies.Count; vertexIndex++) {
            if (!checkedVertices.Contains(vertexIndex)) {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1) { 
                    checkedVertices.Add(newOutlineVertex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);

                    FollowOutline(newOutlineVertex, outlines.Count -1);

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

    bool IsOutline(int vertexA, int vertexB) {
        List<Triangle> trianglesContainingVertexA = trianglesDict[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++) {
            if (trianglesContainingVertexA[i].Contains(vertexB)) {
                sharedTriangleCount++;
            }
            if (sharedTriangleCount > 1) {
                break;
            }
        }

        return sharedTriangleCount == 1;
    }

    int GetConnectedOutlineVertex(int vertexIndex) {
        List<Triangle> trianglesContainsVertex = trianglesDict[vertexIndex];

        for (int i = 0; i < trianglesContainsVertex.Count; i++) {
            Triangle triangle = trianglesContainsVertex[i];

            for (int j = 0; j < 3; j++) {
                int vertexB = triangle.IndexOf(j);
                if (vertexIndex != vertexB && IsOutline(vertexIndex, vertexB) && !checkedVertices.Contains(vertexB)) {
                    return vertexB;
                }
            }
        }
        return -1;
    }

    public class SquareGrid {
        public Square[,] squares;

        public SquareGrid(int[,] map, float squareSize) {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);

            float mapWidth = squareSize * nodeCountX;
            float mapHeight = squareSize * nodeCountY;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++) {
                for (int y = 0; y < nodeCountY; y++) {
                    Vector3 pos = new(-mapWidth/2f + x * squareSize + squareSize/2, 0, -mapHeight/2f + y * squareSize + squareSize/2);
                    controlNodes[x, y] = new ControlNode(pos, map[x,y]==1, squareSize);
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];

            for (int x = 0; x < nodeCountX-1; x++) {
                for (int y = 0; y < nodeCountY-1; y++) {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x, y], controlNodes[x + 1, y]);
                }
            }
        }
    }

    public class Square {
        public ControlNode topLeft, topRight, bottomLeft, bottomRight;
        public Node topMid, bottomMid, leftMid, rightMid;
        public int configuration;

        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomLeft, ControlNode _bottomRight) {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomLeft = _bottomLeft;
            bottomRight = _bottomRight;

            topMid = _topLeft.right;
            bottomMid = _bottomLeft.right;
            leftMid = _bottomLeft.above;
            rightMid = bottomRight.above ;

            if (topLeft.active)
                configuration += 8;
            if (topRight.active)
                configuration += 4;
            if (bottomLeft.active)
                configuration += 2;
            if (bottomRight.active)
                configuration += 1;
        }
    }

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
            above = new Node(position + Vector3.forward * squareSize/2f);
            right = new Node(position + Vector3.right * squareSize/2f);
        }
    }

    public struct Triangle {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;

        int[] vertices;

        public Triangle(int a, int b, int c) {
            vertexIndexA = a; vertexIndexB = b; vertexIndexC = c;
            vertices = new int[3];
            vertices[0] = vertexIndexA; vertices[1] = vertexIndexB; vertices[2] = vertexIndexC;
        }

        public bool Contains(int vertexT) {
            return vertexT == vertexIndexA || vertexT == vertexIndexB || vertexT == vertexIndexC;
        }

        public int IndexOf(int vertexT) { return vertices[vertexT]; }

    }

}
