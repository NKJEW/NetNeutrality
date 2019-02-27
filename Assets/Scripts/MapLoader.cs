using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapTileData {
    public bool walkable;
    public bool usesBitmask;
    public Color tileColor;
}

public class GameTile {
    public GameObject tile;
    public int blockTypeId;
}

public class BitmaskSprite {
    public Sprite sprite;
    public float angle;

    public BitmaskSprite(Sprite newSprite, float newAngle) {
        sprite = newSprite;
        angle = newAngle;
    }
}

public class MapLoader : MonoBehaviour {
    public Texture2D mapTexture;

    public MapTileData[] publicTileData;
    public GameObject tilePrefab;
    BitmaskSprite[] bitmaskSprites;

    GameTile[,] tiles;

    //private string backgroundMatName = "Background_Mat";
    //private Material backgroundMat;

    int width;
    int height;

    //private GameObject enemySpawner;

    enum CardinalDirection {
        NorthWest = 0,
        North = 1,
        NorthEast = 2,
        West = 3,
        East = 4,
        SouthWest = 5,
        South = 6,
        SouthEast = 7
    }

    Dictionary<int, int> bitmaskValueToIndex = new Dictionary<int, int>() {
        {0, 47},
        {2, 1},
        {8, 2},
        {10, 3},
        {11, 4},
        {16, 5},
        {18, 6},
        {22, 7},
        {24, 8},
        {26, 9},
        {27, 10},
        {30, 11},
        {31, 12},
        {64, 13},
        {66, 14},
        {72, 15},
        {74, 16},
        {75, 17},
        {80, 18},
        {82, 19},
        {86, 20},
        {88, 21},
        {90, 22},
        {91, 23},
        {94, 24},
        {95, 25},
        {104, 26},
        {106, 27},
        {107, 28},
        {120, 29},
        {122, 30},
        {123, 31},
        {126, 32},
        {127, 33},
        {208, 34},
        {210, 35},
        {214, 36},
        {216, 37},
        {218, 38},
        {219, 39},
        {222, 40},
        {223, 41},
        {248, 42},
        {250, 43},
        {251, 44},
        {254, 45},
        {255, 46}
    };

    PathfindingMap path;
    BlockPlacer blocker;

    void Awake() {
        //load resources
        //mapTexture = Resources.Load (mapName) as Texture2D;
        //backgroundMat = Resources.Load (backgroundMatName) as Material;

        path = FindObjectOfType<PathfindingMap>();
        blocker = FindObjectOfType<BlockPlacer>();

        bitmaskSprites = LoadBitmaskSprites("Border");

        LoadMap();
    }

    void EmptyMap() {
        //loop through all children and delete them
        while (transform.childCount > 0) {
            GameObject child = transform.GetChild(0).gameObject; //get the first child in the list
            child.transform.SetParent(null); //remove from the list
            Destroy(child);
        }
    }

    void LoadMap() {
        EmptyMap();

        //for now load all the tiles
        Color32[] pixels = mapTexture.GetPixels32();
        width = mapTexture.width;
        height = mapTexture.height;

        tiles = new GameTile[width, height];

        //CreateBackground ();
        //TODO:
        //when these maps are eventually loaded separately, we'll need to know how big the total world is first
        //then update the map with offsets, but for now assume this one map is the entire map
        path.InitializeGrid(width, height);

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Color32 currentPixel = pixels[(y * width) + x];
                tiles[x, y] = new GameTile();
                if (currentPixel.a == 0) {
                    LoadTile(x, y, 0);
                } else {
                    LoadTile(x, y, 1);
                }
            }
        }

        path.AddNeighbors();

        UpdateBitmaskTiles();
    }

    void LoadTile(int x, int y, int id) {
        if (id > 0) {
            if (tiles[x, y].tile == null) {
                tiles[x, y].tile = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity, transform);
            }

            tiles[x, y].tile.GetComponent<SpriteRenderer>().color = publicTileData[id].tileColor;

        } else {
            if (tiles[x, y].tile != null) {
                Destroy(tiles[x, y].tile);
            }

            tiles[x, y].tile = null;
        }

        tiles[x, y].blockTypeId = id;

        bool isWalkable = GetTileData(x, y).walkable;
        path.AddTile(isWalkable, x, y);
        if (isWalkable) {
            blocker.AddFreeTile(new Vector3(x, y, 0));
        }
    }

    MapTileData GetTileData(int x, int y) {
        return publicTileData[tiles[x, y].blockTypeId];
    }

    MapTileData GetTileData(GameTile tile) {
        return publicTileData[tile.blockTypeId];
    }

    void UpdateBitmaskTiles() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                UpdateTileBitmask(x, y);
            }
        }
    }

    void UpdateTileBitmask(int x, int y) {
        if (tiles[x, y].tile != null) {
            if (GetTileData(x, y).usesBitmask) {
                int bitmaskValue = GetBitmaskValue(x, y);
                int spriteIndex = bitmaskValueToIndex[bitmaskValue];

                //print ("Value for (" + x + ", " + y + ") is " + bitmaskValue);

                BitmaskSprite bitmaskSprite = bitmaskSprites[spriteIndex];

                tiles[x, y].tile.GetComponentInChildren<SpriteRenderer>().sprite = bitmaskSprite.sprite;
                tiles[x, y].tile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, bitmaskSprite.angle));

                //TODO: calculate the collider accordingly
            }
        }
    }

    int GetBitmaskValue(int x, int y) {
        List<CardinalDirection> validDirections = new List<CardinalDirection>();

        //check for west
        if (isValidNeighbor(tiles[x, y], new TilePos(x - 1, y)))
            validDirections.Add(CardinalDirection.West);
        //check for east
        if (isValidNeighbor(tiles[x, y], new TilePos(x + 1, y)))
            validDirections.Add(CardinalDirection.East);
        //check for south
        if (isValidNeighbor(tiles[x, y], new TilePos(x, y - 1)))
            validDirections.Add(CardinalDirection.South);
        //check for north
        if (isValidNeighbor(tiles[x, y], new TilePos(x, y + 1)))
            validDirections.Add(CardinalDirection.North);

        if (validDirections.Contains(CardinalDirection.North) && validDirections.Contains(CardinalDirection.East)) {
            //check for northeast
            if (isValidNeighbor(tiles[x, y], new TilePos(x + 1, y + 1)))
                validDirections.Add(CardinalDirection.NorthEast);
        }
        if (validDirections.Contains(CardinalDirection.North) && validDirections.Contains(CardinalDirection.West)) {
            //check for northwest
            if (isValidNeighbor(tiles[x, y], new TilePos(x - 1, y + 1)))
                validDirections.Add(CardinalDirection.NorthWest);
        }
        if (validDirections.Contains(CardinalDirection.South) && validDirections.Contains(CardinalDirection.East)) {
            //check for southeast
            if (isValidNeighbor(tiles[x, y], new TilePos(x + 1, y - 1)))
                validDirections.Add(CardinalDirection.SouthEast);
        }
        if (validDirections.Contains(CardinalDirection.South) && validDirections.Contains(CardinalDirection.West)) {
            //check for southwest
            if (isValidNeighbor(tiles[x, y], new TilePos(x - 1, y - 1)))
                validDirections.Add(CardinalDirection.SouthWest);
        }

        int value = 0;
        foreach (CardinalDirection direction in validDirections) {
            value += (int)Mathf.Pow(2, (int)direction);
        }

        return value;
    }

    bool isValidNeighbor(GameTile currentTile, TilePos newTilePos) {
        if ((newTilePos.x >= 0) && (newTilePos.x <= (width - 1)) && (newTilePos.y >= 0) && (newTilePos.y <= (height - 1))) {
            GameTile neighborTile = tiles[newTilePos.x, newTilePos.y];
            return GetTileData(currentTile).walkable == GetTileData(neighborTile).walkable;;
        } else {
            return true;
        }
    }

    public void AddObstacle(int x, int y) {
        LoadTile(x, y, 2);

        for (int dx = -1; dx <= 1; dx++) {
            for (int dy = -1; dy <= 1; dy++) {
                if ((dx == 0) && (dy == 0))
                    continue;

                int checkX = x + dx;
                int checkY = y + dy;

                if ((checkX >= 0) && (checkX < width) && (checkY >= 0) && (checkY < height)) {
                    UpdateTileBitmask(checkX, checkY);
                }
            }
        }

        UpdateTileBitmask(x, y);
    }

    /*void CreateBackground ()
    {
        GameObject backgroundObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        backgroundObj.name = "Background";
        backgroundObj.transform.position = new Vector3 ((width / 2) - 0.5f, (height / 2) - 0.5f, 0);
        backgroundObj.transform.localScale = new Vector3 (width, height, 1);

        backgroundObj.transform.parent = this.transform;

        Renderer rend = backgroundObj.GetComponentInChildren <Renderer> ();
        rend.material = backgroundMat;
        rend.material.mainTextureScale = new Vector2 (width, height);
    }*/

    BitmaskSprite[] LoadBitmaskSprites(string prefabName) {
        BitmaskSprite[] bitmaskSprites = new BitmaskSprite[48];

        //Sprite baseSprite = null;//Resources.Load<Sprite>(prefabName + "/BaseSprite");
        Sprite sideSprite = Resources.Load<Sprite>(prefabName + "/SideSprite");
        Sprite innerCornerSprite = Resources.Load<Sprite>(prefabName + "/InnerCornerSprite");
        Sprite outerCornerSprite = Resources.Load<Sprite>(prefabName + "/InnerCornerSprite");
        Sprite tSplitSprite = Resources.Load<Sprite>(prefabName + "/TSplitSprite");
        Sprite endSprite = Resources.Load<Sprite>(prefabName + "/EndSprite");
        Sprite boxSprite = Resources.Load<Sprite>(prefabName + "/BaseSprite");
        Sprite xSprite = Resources.Load<Sprite>(prefabName + "/IntersectionSprite");

        bitmaskSprites[0] = new BitmaskSprite(null, 0);
        bitmaskSprites[1] = new BitmaskSprite(endSprite, 180);
        bitmaskSprites[2] = new BitmaskSprite(endSprite, 270);
        bitmaskSprites[3] = new BitmaskSprite(innerCornerSprite, 90);
        bitmaskSprites[4] = new BitmaskSprite(outerCornerSprite, 90);
        bitmaskSprites[5] = new BitmaskSprite(endSprite, 90);
        bitmaskSprites[6] = new BitmaskSprite(innerCornerSprite, 0);
        bitmaskSprites[7] = new BitmaskSprite(outerCornerSprite, 0);
        bitmaskSprites[8] = new BitmaskSprite(sideSprite, 0);
        bitmaskSprites[9] = new BitmaskSprite(tSplitSprite, 0);
        bitmaskSprites[10] = new BitmaskSprite(tSplitSprite, 0);
        bitmaskSprites[11] = new BitmaskSprite(tSplitSprite, 0);
        bitmaskSprites[12] = new BitmaskSprite(sideSprite, 180);
        bitmaskSprites[13] = new BitmaskSprite(endSprite, 0);
        bitmaskSprites[14] = new BitmaskSprite(sideSprite, 90);
        bitmaskSprites[15] = new BitmaskSprite(innerCornerSprite, 180);
        bitmaskSprites[16] = new BitmaskSprite(tSplitSprite, 90);
        bitmaskSprites[17] = new BitmaskSprite(tSplitSprite, 90);
        bitmaskSprites[18] = new BitmaskSprite(innerCornerSprite, 270);
        bitmaskSprites[19] = new BitmaskSprite(tSplitSprite, 270);
        bitmaskSprites[20] = new BitmaskSprite(tSplitSprite, 270);
        bitmaskSprites[21] = new BitmaskSprite(tSplitSprite, 180);
        bitmaskSprites[22] = new BitmaskSprite(xSprite, 0);
        bitmaskSprites[23] = new BitmaskSprite(xSprite, 0);
        bitmaskSprites[24] = new BitmaskSprite(xSprite, 0);
        bitmaskSprites[25] = new BitmaskSprite(tSplitSprite, 180);
        bitmaskSprites[26] = new BitmaskSprite(outerCornerSprite, 180);
        bitmaskSprites[27] = new BitmaskSprite(tSplitSprite, 90);
        bitmaskSprites[28] = new BitmaskSprite(sideSprite, 270);
        bitmaskSprites[29] = new BitmaskSprite(tSplitSprite, 180);
        bitmaskSprites[30] = new BitmaskSprite(xSprite, 0);
        bitmaskSprites[31] = new BitmaskSprite(tSplitSprite, 270);
        bitmaskSprites[32] = new BitmaskSprite(xSprite, 0);
        bitmaskSprites[33] = new BitmaskSprite(innerCornerSprite, 270);
        bitmaskSprites[34] = new BitmaskSprite(outerCornerSprite, 270);
        bitmaskSprites[35] = new BitmaskSprite(tSplitSprite, 270);
        bitmaskSprites[36] = new BitmaskSprite(sideSprite, 90);
        bitmaskSprites[37] = new BitmaskSprite(tSplitSprite, 180);
        bitmaskSprites[38] = new BitmaskSprite(xSprite, 0);
        bitmaskSprites[39] = new BitmaskSprite(xSprite, 0);
        bitmaskSprites[40] = new BitmaskSprite(tSplitSprite, 90);
        bitmaskSprites[41] = new BitmaskSprite(innerCornerSprite, 180);
        bitmaskSprites[42] = new BitmaskSprite(sideSprite, 0);
        bitmaskSprites[43] = new BitmaskSprite(tSplitSprite, 0);
        bitmaskSprites[44] = new BitmaskSprite(innerCornerSprite, 0);
        bitmaskSprites[45] = new BitmaskSprite(innerCornerSprite, 90);
        bitmaskSprites[46] = new BitmaskSprite(null, 0);
        bitmaskSprites[47] = new BitmaskSprite(boxSprite, 0);

        return bitmaskSprites;
    }
}