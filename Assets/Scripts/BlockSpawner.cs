using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour {
    public float spawnRate;
    public int id;

    float nextSpawn;

    SpawnManager spawnManager;

	void Start() {
        spawnManager = FindObjectOfType<SpawnManager>();
        nextSpawn = Time.time + spawnRate;

        if (spawnManager.GetRemainingSpawns(id) == 0) {
            enabled = false;
        }
	}

    void Update() {
        if (Time.time > nextSpawn) {
            nextSpawn = Time.time + spawnRate;
            if (spawnManager.GetRemainingSpawns(id) == 0) {
                enabled = false;
                return;
            }

            spawnManager.PlaceBlock(id);
        }
    }
}
