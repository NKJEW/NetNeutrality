using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour {
    public float spawnRate;
    public int id;

    float nextSpawn;
    bool isInited;

    SpawnManager spawnManager;

	void Awake() {
        spawnManager = FindObjectOfType<SpawnManager>();
	}

    public void Init() {
        if (spawnManager.GetRemainingSpawns(id) == 0) {
            enabled = false;
            return;
        }


        isInited = true;
    }

    void Update() {
        if (isInited && Time.time > nextSpawn) {
            nextSpawn = Time.time + spawnRate;
            if (spawnManager.GetRemainingSpawns(id) == 0) {
                enabled = false;
                return;
            }

            spawnManager.PlaceBlock(id);
        }
    }
}
