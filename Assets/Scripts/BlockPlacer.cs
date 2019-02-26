using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPlacer : MonoBehaviour {
    public float blockPlaceRate;
    public GameObject block;

    List<Vector3> freeTiles = new List<Vector3>();
    PlayerController player;
    MapLoader map;

    float nextBlockPlacement;

	void Start() {
        player = FindObjectOfType<PlayerController>();
        map = FindObjectOfType<MapLoader>();
	}

	public void AddFreeTile(Vector3 pos) {
        freeTiles.Add(pos);
    }

    Vector3 GetRandomPos() {
        Vector3 randomPos = freeTiles[Random.Range(0, freeTiles.Count)];
        for (int i = 0; i < 10; i++) { //set limit to prevent infinite loop
            if (Vector3.Distance(player.transform.position, randomPos) < 2) {
                randomPos = freeTiles[Random.Range(0, freeTiles.Count)];
            } else {
                break;
            }
        }
        return randomPos;
    }

    void Update() {
        if (Time.time > nextBlockPlacement) {
            nextBlockPlacement = Time.time + blockPlaceRate;
            PlaceBlock();
        }
    }

    void PlaceBlock() {
        Vector3 pos = GetRandomPos();

        GameObject newObstacle = Instantiate(block, pos, Quaternion.identity);
        freeTiles.Remove(pos);
        TilePos tilePos = PathfindingMap.WorldToTilePos(pos);
        PathfindingMap.UpdateTile(false, tilePos.x, tilePos.y);
        map.AddObstacle(newObstacle, tilePos.x, tilePos.y);
    }
}
