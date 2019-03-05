using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;

    public float difficulty;

    MapLoader map;
    LevelManager levels;
    CollectibleSpawner collectibleSpawner;
    BarManager bar;
    SpawnManager spawnManager;
    BlockSpawner[] blockSpawners;
    PlayerController playerController;

    void Awake() {
        instance = this;
        map = FindObjectOfType<MapLoader>();
        levels = FindObjectOfType<LevelManager>();

        collectibleSpawner = FindObjectOfType<CollectibleSpawner>();
        blockSpawners = FindObjectsOfType<BlockSpawner>();
        spawnManager = FindObjectOfType<SpawnManager>();
        playerController = FindObjectOfType<PlayerController>();

        bar = FindObjectOfType<BarManager>();
    }

    void Start() {
        StartGame(0);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            StartGame(1);
        }
    }

    public void StartGame(int id) {
        spawnManager.Reset();
        LevelManager.LevelData levelData = levels.levels[id];
        map.LoadMap(levelData.map);

        collectibleSpawner.Init();
        foreach (BlockSpawner spawner in blockSpawners) {
            spawner.Init();
        }

        playerController.Init();

        bar.InitTime(Mathf.Lerp(levelData.easyTime, levelData.hardTime, difficulty));
    }

    public void GameOver() {
        print("uyape");
    }
}
