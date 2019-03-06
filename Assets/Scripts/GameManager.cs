using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;

    public float difficulty;
    int curLevelID;

    MapLoader map;
    LevelManager levels;
    CollectibleSpawner collectibleSpawner;
    BarManager bar;
    SpawnManager spawnManager;
    BlockSpawner[] blockSpawners;
    PlayerController playerController;
    CameraController camCon;

    void Awake() {
        instance = this;
        map = FindObjectOfType<MapLoader>();
        levels = FindObjectOfType<LevelManager>();

        collectibleSpawner = FindObjectOfType<CollectibleSpawner>();
        blockSpawners = FindObjectsOfType<BlockSpawner>();
        spawnManager = FindObjectOfType<SpawnManager>();
        playerController = FindObjectOfType<PlayerController>();
        camCon = FindObjectOfType<CameraController>();

        bar = FindObjectOfType<BarManager>();
    }

    void Start() {
        StartGame(curLevelID);
    }

    public void StartGame(int id) {
        spawnManager.Reset();
        LevelManager.LevelData levelData = levels.levels[id];
        map.LoadMap(levelData.map);

        float levelTime = Mathf.Lerp(levelData.easyTime, levelData.hardTime, difficulty);

        collectibleSpawner.Init();
        foreach (BlockSpawner spawner in blockSpawners) {
            spawner.Init(levelTime);
        }

        playerController.Init();

        bar.InitTime(levelTime);
    }

    public void StartBlockSpawning() {
        foreach (BlockSpawner spawner in blockSpawners) {
            spawner.StartSpawning();
        }
    }

    public void GameOver() {
        print("uyape");
    }

    public void GameWin() {
        playerController.Stop();
        StartCoroutine(GameWinSequence());
    }

    IEnumerator GameWinSequence() {
        camCon.ChangePixelation(-1);
        yield return new WaitUntil(() => !camCon.transitioning);
        curLevelID++;
        StartGame(curLevelID);
        camCon.ChangePixelation(1);
    }
}
