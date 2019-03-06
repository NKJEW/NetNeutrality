using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleSpawner : MonoBehaviour {
    public GameObject effect;

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
            Instantiate(effect, spawner.GetNextPos(3), Quaternion.identity);

            spawner.PlaceBlock(3);
        }
    }
}
