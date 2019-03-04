using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleSpawner : MonoBehaviour {
    SpawnManager spawner;

    void Start() {
        spawner = FindObjectOfType<SpawnManager>();
        SpawnCollectible();
    }

    public void SpawnCollectible() {
        if (spawner.GetRemainingSpawns(3) > 0) {
            spawner.PlaceBlock(3);
        }
    }
}
