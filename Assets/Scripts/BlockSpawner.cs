﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour {
    public float finishBufferRatio;
    public float spawnRate;
    public int id;

    float nextSpawn;
    bool isInited;

    SpawnManager spawnManager;

	void Awake() {
        spawnManager = FindObjectOfType<SpawnManager>();
	}

    public void Init(float levelTime, bool startAutomatically = false) {
        spawnRate = (levelTime * finishBufferRatio) / spawnManager.GetRemainingSpawns(id);
        isInited = startAutomatically;
        if (startAutomatically) {
            nextSpawn = Time.time;
        }
    }

    public void StartSpawning() {
        if (spawnManager.GetRemainingSpawns(id) == 0) {
            return;
        }

        nextSpawn = Time.time;
        isInited = true;
    }

    void Update() {
        if (isInited && Time.time > nextSpawn) {
            nextSpawn = Time.time + spawnRate;
            if (spawnManager.GetRemainingSpawns(id) == 0) {
                isInited = false;
                return;
            }

            spawnManager.PlaceBlock(id);
        }
    }
}
