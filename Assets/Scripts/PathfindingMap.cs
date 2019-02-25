using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePos {
    public int x;
    public int y;

    public TilePos(int newX, int newY) {
        x = newX;
        y = newY;
    }
}

public class Node : IHeapItem<Node> {
    public bool walkable;
    //TODO: add a dead zone

    public TilePos gridPosition;

    public List<Node> neighbors;

    public int gCost;
    public int hCost;

    public Node parent;

    int heapIndex;

    public Node() {
        neighbors = new List<Node>();
    }

    public int fCost {
        get {
            return gCost + hCost;
        }
    }

    public override string ToString() {
        return string.Format("({0}, {1})", gridPosition.x, gridPosition.y);
    }

    public string NeighborsToString() {
        string value = this.ToString() + " - Neighbors: ";
        foreach (Node neighbor in neighbors) {
            value += (neighbor.ToString() + " ");
        }

        return value;
    }

    public int HeapIndex {
        get { return heapIndex; }
        set { heapIndex = value; }
    }

    public int CompareTo(Node nodeToCompare) {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0) {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }

        return -compare;
    }
}

public class PathfindingMap : MonoBehaviour {
    public static Node[,] pathfindingGrid;

    private static int xSize;
    private static int ySize;

    public void InitializeGrid(int x, int y) {
        pathfindingGrid = new Node[x, y];
        xSize = x;
        ySize = y;
    }

    public void AddTile(bool walkable, int x, int y) {
        Node newNode = new Node();

        newNode.walkable = walkable;

        newNode.gridPosition = new TilePos(x, y);

        pathfindingGrid[x, y] = newNode;
    }

    public void AddNeighbors() {
        for (int x = 0; x < xSize; x++) {
            for (int y = 0; y < ySize; y++) {
                if (pathfindingGrid[x, y].walkable) {
                    AddNeighborsForNode(pathfindingGrid[x, y]);
                }
            }
        }
    }

    void AddNeighborsForNode(Node node) {
        for (int dx = -1; dx <= 1; dx++) {
            for (int dy = -1; dy <= 1; dy++) {
                if ((dx == 0) && (dy == 0))
                    continue;

                int checkX = node.gridPosition.x + dx;
                int checkY = node.gridPosition.y + dy;

                if ((checkX >= 0) && (checkX < xSize) && (checkY >= 0) && (checkY < ySize)) {
                    node.neighbors.Add(pathfindingGrid[checkX, checkY]);
                }
            }
        }
    }

    public static bool CanWalkOnTile(Vector3 worldPos) {
        return CanWalkOnTile(WorldToTilePos(worldPos));
    }

    public static bool CanWalkOnTile(TilePos tilePos) {
        return pathfindingGrid[tilePos.x, tilePos.y].walkable;
    }

    public static int MaxSize {
        get { return (xSize * ySize); }
    }

    public static TilePos WorldToTilePos(Vector3 worldPos) {
        return new TilePos(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
    }

    public static Vector3 TileToWorldPos(TilePos tilePos) {
        return new Vector3(tilePos.x, tilePos.y, 0);
    }
}