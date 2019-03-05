using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleSpawner : MonoBehaviour {
    SpawnManager spawner;
    BarManager bar;

    void Awake() {
        spawner = FindObjectOfType<SpawnManager>();
        bar = FindObjectOfType<BarManager>();
    }

    public void Init() {
        bar.InitNumCollectibles(spawner.GetRemainingSpawns(3));
        SpawnCollectible();
    }

    public void SpawnCollectible() {
        if (spawner.GetRemainingSpawns(3) > 0) {
            spawner.PlaceBlock(3);
        }
    }
}
