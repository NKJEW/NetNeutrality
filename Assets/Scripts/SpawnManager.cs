using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BlockSpawn {
    public int spawnPriority;
    public Vector3 pos;

    public BlockSpawn(Vector3 spawnPos, int priority) { //lol
        spawnPriority = priority;
        pos = spawnPos;
    }
}

public class SpawnManager : MonoBehaviour {
    List<List<BlockSpawn>> spawns = new List<List<BlockSpawn>>();
    PlayerController player;
    MapLoader map;

    public void Init(MapLoader mapLoader) {
        player = FindObjectOfType<PlayerController>();
        map = mapLoader;

        InitSpawnLists();
	}

    void InitSpawnLists() {
        for (int i = 0; i < map.publicTileData.Length; i++) {
            spawns.Add(new List<BlockSpawn>());
        }
    }

    public void Reset() {
        for (int i = 0; i < spawns.Count; i++) {
            spawns[i].Clear();
        }
    }

	public void AddFreeTile(int id, Vector3 pos, int priority) {
        spawns[id].Add(new BlockSpawn(pos, priority));
    }

    public void SortSpawnLists() {
        for (int i = 0; i < spawns.Count; i++) {
            spawns[i].Sort(SpawnSort);
        }
    }

    public int SpawnSort(BlockSpawn a, BlockSpawn b) {
        return -a.spawnPriority.CompareTo(b.spawnPriority);
    }

    public void PlaceBlock(int id) {
        TilePos tilePos = PathfindingMap.WorldToTilePos(spawns[id][0].pos);
        spawns[id].RemoveAt(0);

        map.AddObstacle(tilePos.x, tilePos.y, id);
    }

    public int GetRemainingSpawns(int id) {
        return spawns[id].Count;
    }
}
