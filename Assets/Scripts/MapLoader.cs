using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapTileData {
    public Color32 fileColor;
    public bool walkable;
    public bool usesBitmask;
    public Color tileColor;
    public Color iconColor;
    public Sprite iconSprite;
    public bool isCollectible;
}

public class GameTile {
    public GameObject tile;
    public int blockTypeId;

    public SpriteRenderer GetMainSprite() {
        return tile.transform.Find("Main").GetComponent<SpriteRenderer>();
    }

    public SpriteRenderer GetIconSprite() {
        return tile.transform.Find("Icon").GetComponent<SpriteRenderer>();
    }

    public void UpdateIcon(Sprite sprite) {
        if (tile == null) {
            return;
        }

        tile.transform.Find("Icon").GetComponent<SpriteRenderer>().sprite = sprite;
    }
}

public class BitmaskSprite {
    public Sprite sprite;
    public int angle;

    public BitmaskSprite(Sprite newSprite, int newAngle) {
        sprite = newSprite;
        angle = newAngle;
    }
}

public class MapLoader : MonoBehaviour {
    public Texture2D mapTexture;

    public MapTileData[] publicTileData;
    public GameObject tilePrefab;
    BitmaskSprite[] bitmaskSprites;

    Dictionary<Color32, int> colorIds = new Dictionary<Color32, int>();

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
    SpawnManager blocker;

    void Awake() {
        //load resources
        //mapTexture = Resources.Load (mapName) as Texture2D;
        //backgroundMat = Resources.Load (backgroundMatName) as Material;

        path = FindObjectOfType<PathfindingMap>();
        blocker = FindObjectOfType<SpawnManager>();
        blocker.Init(this);

        bitmaskSprites = LoadBitmaskSprites("Border");

        LoadColorDictionary();
        LoadMap();
    }

    void LoadColorDictionary() {
        for (int i = 0; i < publicTileData.Length; i++) {
            colorIds.Add(publicTileData[i].fileColor, i);
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
                Color32 fullColorPixel = GetFullColor(currentPixel);

                tiles[x, y] = new GameTile();
                if (fullColorPixel.Equals(publicTileData[1].fileColor) && currentPixel.a > 0) {
                    LoadTile(x, y, 1);
                } else {
                    LoadTile(x, y, 0);
                    if (currentPixel.a > 0 && colorIds.ContainsKey(fullColorPixel)) {
                        blocker.AddFreeTile(colorIds[fullColorPixel], new Vector3(x, y, 0), currentPixel.a);
                    }
                }
            }
        }

        path.AddNeighbors();
        blocker.SortSpawnLists();

        UpdateBitmaskTiles();
    }

    void LoadTile(int x, int y, int id) {
        if (id > 0) {
            if (tiles[x, y].tile == null) {
                tiles[x, y].tile = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity, transform);
            }

            tiles[x, y].GetMainSprite().color = publicTileData[id].tileColor;
            tiles[x, y].GetIconSprite().color = publicTileData[id].iconColor;

        } else {
            if (tiles[x, y].tile != null) {
                Destroy(tiles[x, y].tile);
            }

            tiles[x, y].tile = null;
        }

        tiles[x, y].blockTypeId = id;

        bool isWalkable = GetTileData(x, y).walkable;
        path.AddTile(isWalkable, x, y);

        tiles[x, y].UpdateIcon(publicTileData[id].iconSprite);
    }

    Color32 GetFullColor(Color32 original) {
        return new Color32(original.r, original.g, original.b, 255);
    }

    public MapTileData GetTileData(int x, int y) {
        return publicTileData[tiles[x, y].blockTypeId];
    }

    MapTileData GetTileData(GameTile tile) {
        return publicTileData[tile.blockTypeId];
    }

    public GameTile GetTile(int x, int y) {
        return tiles[x, y];
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

                SpriteRenderer srend = tiles[x, y].GetMainSprite();
                srend.sprite = bitmaskSprite.sprite;
                srend.transform.rotation = Quaternion.Euler(0, 0, bitmaskSprite.angle);
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

    public void AddObstacle(int x, int y, int id) {
        LoadTile(x, y, id);

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
        BitmaskSprite[] newSprites = new BitmaskSprite[48];

        //Sprite baseSprite = null;//Resources.Load<Sprite>(prefabName + "/BaseSprite");
        Sprite sideSprite = Resources.Load<Sprite>(prefabName + "/SideSprite");
        Sprite innerCornerSprite = Resources.Load<Sprite>(prefabName + "/InnerCornerSprite");
        Sprite outerCornerSprite = Resources.Load<Sprite>(prefabName + "/InnerCornerSprite");
        Sprite tSplitSprite = Resources.Load<Sprite>(prefabName + "/TSplitSprite");
        Sprite endSprite = Resources.Load<Sprite>(prefabName + "/EndSprite");
        Sprite boxSprite = Resources.Load<Sprite>(prefabName + "/BaseSprite");
        Sprite xSprite = Resources.Load<Sprite>(prefabName + "/IntersectionSprite");

        newSprites[0] = new BitmaskSprite(null, 0);
        newSprites[1] = new BitmaskSprite(endSprite, 180);
        newSprites[2] = new BitmaskSprite(endSprite, 270);
        newSprites[3] = new BitmaskSprite(innerCornerSprite, 90);
        newSprites[4] = new BitmaskSprite(outerCornerSprite, 90);
        newSprites[5] = new BitmaskSprite(endSprite, 90);
        newSprites[6] = new BitmaskSprite(innerCornerSprite, 0);
        newSprites[7] = new BitmaskSprite(outerCornerSprite, 0);
        newSprites[8] = new BitmaskSprite(sideSprite, 0);
        newSprites[9] = new BitmaskSprite(tSplitSprite, 0);
        newSprites[10] = new BitmaskSprite(tSplitSprite, 0);
        newSprites[11] = new BitmaskSprite(tSplitSprite, 0);
        newSprites[12] = new BitmaskSprite(sideSprite, 180);
        newSprites[13] = new BitmaskSprite(endSprite, 0);
        newSprites[14] = new BitmaskSprite(sideSprite, 90);
        newSprites[15] = new BitmaskSprite(innerCornerSprite, 180);
        newSprites[16] = new BitmaskSprite(tSplitSprite, 90);
        newSprites[17] = new BitmaskSprite(tSplitSprite, 90);
        newSprites[18] = new BitmaskSprite(innerCornerSprite, 270);
        newSprites[19] = new BitmaskSprite(tSplitSprite, 270);
        newSprites[20] = new BitmaskSprite(tSplitSprite, 270);
        newSprites[21] = new BitmaskSprite(tSplitSprite, 180);
        newSprites[22] = new BitmaskSprite(xSprite, 0);
        newSprites[23] = new BitmaskSprite(xSprite, 0);
        newSprites[24] = new BitmaskSprite(xSprite, 0);
        newSprites[25] = new BitmaskSprite(tSplitSprite, 180);
        newSprites[26] = new BitmaskSprite(outerCornerSprite, 180);
        newSprites[27] = new BitmaskSprite(tSplitSprite, 90);
        newSprites[28] = new BitmaskSprite(sideSprite, 270);
        newSprites[29] = new BitmaskSprite(tSplitSprite, 180);
        newSprites[30] = new BitmaskSprite(xSprite, 0);
        newSprites[31] = new BitmaskSprite(tSplitSprite, 270);
        newSprites[32] = new BitmaskSprite(xSprite, 0);
        newSprites[33] = new BitmaskSprite(innerCornerSprite, 270);
        newSprites[34] = new BitmaskSprite(outerCornerSprite, 270);
        newSprites[35] = new BitmaskSprite(tSplitSprite, 270);
        newSprites[36] = new BitmaskSprite(sideSprite, 90);
        newSprites[37] = new BitmaskSprite(tSplitSprite, 180);
        newSprites[38] = new BitmaskSprite(xSprite, 0);
        newSprites[39] = new BitmaskSprite(xSprite, 0);
        newSprites[40] = new BitmaskSprite(tSplitSprite, 90);
        newSprites[41] = new BitmaskSprite(innerCornerSprite, 180);
        newSprites[42] = new BitmaskSprite(sideSprite, 0);
        newSprites[43] = new BitmaskSprite(tSplitSprite, 0);
        newSprites[44] = new BitmaskSprite(innerCornerSprite, 0);
        newSprites[45] = new BitmaskSprite(innerCornerSprite, 90);
        newSprites[46] = new BitmaskSprite(null, 0);
        newSprites[47] = new BitmaskSprite(boxSprite, 0);

        return newSprites;
    }
}