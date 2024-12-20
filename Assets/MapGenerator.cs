using UnityEngine;

public class MapGenerator : MonoBehaviour {
    public int width = 100;
    public int height = 100;

    [Range(0, 100)]
    public int fullPresent;
    public string seed;
    public bool useRandomSeed;

    [Range(0, 8)]
    public int wallThresholdSize = 4;
    public int subSteps = 4;

    int[,] map;


    void Start() {
        GenerateMap();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            GenerateMap();
        }
    }

    void GenerateMap() {
        map = new int[width, height];
        RandomFillMap();

        for (int i = 0; i < subSteps; i++) {
            SmoothMap();
        }

        int borderSize = 5;
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int i = 0; i < borderedMap.GetLength(0); i++) {
            for (int j = 0; j < borderedMap.GetLength(1); j++) {
                if (i >= borderSize && i < width + borderSize && j >= borderSize && j < height + borderSize) {
                    borderedMap[i, j] = map[i - borderSize, j - borderSize];
                } else {
                    borderedMap[i, j] = 1;
                }
            }
        }

        MeshGenerator meshGenerator = GetComponent<MeshGenerator>();
        meshGenerator.GenerateMesh(borderedMap, 1);
    }

    void RandomFillMap() {
        System.Random r;
        if (useRandomSeed) {
             r = new();
        } else {
            r = new System.Random(seed.GetHashCode());
        }

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (i == 0 || j == 0 || i == width - 1 || j == height - 1) {
                    map[i, j] = 1;
                    continue;
                }
                map[i, j] = (r.Next(0, 100) < fullPresent) ? 1 : 0;
            }
        }
    }

    void SmoothMap() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                int surroundingWallCount = GetSurroundingWallCount(x, y);
                if (surroundingWallCount > wallThresholdSize) {
                    map[x, y] = 1;
                } else if (surroundingWallCount < wallThresholdSize) {
                    map[x, y] = 0;
                }
            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY) {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {
                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height) {
                        if (neighbourX != gridX || neighbourY != gridY) {
                        wallCount += map[neighbourX, neighbourY];
                    }
                } else {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    private void OnDrawGizmos() {
        //if (map != null) {
        //    for (int i = 0; i < width; i++) {
        //        for (int j = 0; j < height; j++) {
        //            Gizmos.color = (map[i, j] == 1) ? Color.black : Color.white;
        //            Vector3 pos = new Vector3(-width / 2 + i + .5f, 0, -height / 2 + j + .5f);
        //            Gizmos.DrawCube(pos, Vector3.one);
        //        }
        //    }
        //}
    }
}
