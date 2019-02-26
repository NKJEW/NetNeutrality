using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapTileData {
    public Color32 color;
    public string prefabName;

    public bool terrain;
    public bool enemy;
    public bool walkable;
    public bool usesBitmask;

    [HideInInspector]
    public GameObject prefab;
}

public class GameTile {
    public GameObject tile;
    public bool usesBitmask;
    public bool walkable;
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
    Dictionary<Color32, MapTileData> tileData = new Dictionary<Color32, MapTileData>();
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

        LoadTilesIntoDictionary();
        LoadMap();
    }

    void LoadTilesIntoDictionary() {
        foreach (MapTileData mapTile in publicTileData) {
            //if (!mapTile.enemy)
            //{
                mapTile.prefab = Resources.Load (mapTile.prefabName) as GameObject;
            //}
            //else
            //{
            //    if (enemySpawner == null)
            //    {
            //        enemySpawner = Resources.Load ("EnemySpawner") as GameObject;
            //    }
            //    mapTile.prefab = enemySpawner;
            //}

            tileData.Add(mapTile.color, mapTile);
        }
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
                    tiles[x, y].tile = null;
                    tiles[x, y].walkable = true;
                    tiles[x, y].usesBitmask = false;

                    path.AddTile(true, x, y);
                } else {
                    GameObject newTile = CreateVisibleTileAtPosition(currentPixel, x, y);

                    if (tileData[currentPixel].terrain) {
                        tiles[x, y].tile = newTile;
                        tiles[x, y].walkable = tileData[currentPixel].walkable;
                        tiles[x, y].usesBitmask = tileData[currentPixel].usesBitmask;

                        path.AddTile(tileData[currentPixel].walkable, x, y);
                    } else {
                        tiles[x, y].tile = null;
                        tiles[x, y].walkable = true;
                        tiles[x, y].usesBitmask = false;

                        path.AddTile(true, x, y);
                    }

                    /*if (tileData [currentPixel].enemy)
                    {
                        newTile.GetComponent<EnemySpawn> ().enemyName = tileData [currentPixel].prefabName;
                    }*/
                }

                if (tiles[x, y].walkable) {
                    blocker.AddFreeTile(new Vector3(x, y, 0));
                }
            }
        }

        path.AddNeighbors();

        UpdateBitmaskTiles();
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
            if (tiles[x, y].usesBitmask) {
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
            return currentTile.walkable == neighborTile.walkable;;
        } else {
            return true;
        }
    }

    public void AddObstacle(GameObject obstacle, int x, int y) {
        tiles[x, y].tile = obstacle;
        tiles[x, y].walkable = false;
        tiles[x, y].usesBitmask = true;

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
        tiles[x, y].tile.GetComponent<SpriteRenderer>().color = Color.red;
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

    GameObject CreateVisibleTileAtPosition(Color32 color, int x, int y) {
        GameObject tile = null;

        if (tileData.ContainsKey(color)) {
            tile = tileData[color].prefab;
        } else {
            Debug.LogError("No prefab exists for tile at (" + x + ", " + y + ") with color" + color);
        }

        if (tile != null) {
            return (GameObject)Instantiate(tile, new Vector3(x, y, 0), Quaternion.identity, transform);
        } else {
            return null;
        }
    }

    BitmaskSprite[] LoadBitmaskSprites(string prefabName) {
        BitmaskSprite[] bitmaskSprites = new BitmaskSprite[48];

        Sprite baseSprite = null;//Resources.Load<Sprite>(prefabName + "/BaseSprite");
        Sprite sideSprite = Resources.Load<Sprite>(prefabName + "/SideSprite");
        Sprite innerCornerSprite = Resources.Load<Sprite>(prefabName + "/InnerCornerSprite");
        Sprite outerCornerSprite = Resources.Load<Sprite>(prefabName + "/OuterCornerSprite");
        Sprite tSplitSprite = Resources.Load<Sprite>(prefabName + "/TSplitSprite");
        Sprite endSprite = Resources.Load<Sprite>(prefabName + "/EndSprite");

        bitmaskSprites[0] = new BitmaskSprite(baseSprite, 0);
        bitmaskSprites[1] = new BitmaskSprite(endSprite, 180);
        bitmaskSprites[2] = new BitmaskSprite(endSprite, 270);
        bitmaskSprites[3] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[4] = new BitmaskSprite(outerCornerSprite, 270);
        bitmaskSprites[5] = new BitmaskSprite(endSprite, 90);
        bitmaskSprites[6] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[7] = new BitmaskSprite(outerCornerSprite, 180);
        bitmaskSprites[8] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[9] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[10] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[11] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[12] = new BitmaskSprite(sideSprite, 180);
        bitmaskSprites[13] = new BitmaskSprite(endSprite, 0);
        bitmaskSprites[14] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[15] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[16] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[17] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[18] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[19] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[20] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[21] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[22] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[23] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[24] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[25] = new BitmaskSprite(tSplitSprite, 180);
        bitmaskSprites[26] = new BitmaskSprite(outerCornerSprite, 0);
        bitmaskSprites[27] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[28] = new BitmaskSprite(sideSprite, 270);
        bitmaskSprites[29] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[30] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[31] = new BitmaskSprite(tSplitSprite, 270);
        bitmaskSprites[32] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[33] = new BitmaskSprite(innerCornerSprite, 270);
        bitmaskSprites[34] = new BitmaskSprite(outerCornerSprite, 90);
        bitmaskSprites[35] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[36] = new BitmaskSprite(sideSprite, 90);
        bitmaskSprites[37] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[38] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[39] = new BitmaskSprite(null, 0);//TODO
        bitmaskSprites[40] = new BitmaskSprite(tSplitSprite, 90);
        bitmaskSprites[41] = new BitmaskSprite(innerCornerSprite, 180);
        bitmaskSprites[42] = new BitmaskSprite(sideSprite, 0);
        bitmaskSprites[43] = new BitmaskSprite(tSplitSprite, 0);
        bitmaskSprites[44] = new BitmaskSprite(innerCornerSprite, 0);
        bitmaskSprites[45] = new BitmaskSprite(innerCornerSprite, 90);
        bitmaskSprites[46] = new BitmaskSprite(baseSprite, 0);
        bitmaskSprites[47] = new BitmaskSprite(null, 0);//TODO

        return bitmaskSprites;
    }
}