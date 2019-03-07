using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager instance;

    public float difficulty;

    public GameObject loadingCanvas;
    public GameObject goText;
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
        loadingCanvas.SetActive(false);
        goText.SetActive(false);

        StartTutorial();
    }

    void StartTutorial() {
        spawnManager.Reset();
        LevelManager.LevelData levelData = levels.levels[0];
        map.LoadMap(levelData.map);

        float levelTime = Mathf.Lerp(levelData.easyTime, levelData.hardTime, difficulty);

        foreach (BlockSpawner spawner in blockSpawners) {
            spawner.Init(1, true);
        }

        playerController.Init();
        playerController.Stop();

        bar.InitNumCollectibles(spawnManager.GetRemainingSpawns(3));
        bar.InitTime(levelTime);

        StartCoroutine(TutorialSetupSequence());
    }

    IEnumerator TutorialSetupSequence() {
        yield return new WaitForSeconds(3f);
        playerController.Unfreeze();
        collectibleSpawner.Init();

        StartCoroutine(GoSequence());
    }

    public void StartGame(int id) {
        spawnManager.Reset();
        LevelManager.LevelData levelData = levels.levels[id];
        map.LoadMap(levelData.map);

        float levelTime = Mathf.Lerp(levelData.easyTime, levelData.hardTime, difficulty);

        foreach (BlockSpawner spawner in blockSpawners) {
            spawner.Init(levelTime);
        }

        playerController.Init();

        bar.InitNumCollectibles(spawnManager.GetRemainingSpawns(3));
        bar.InitTime(levelTime);
    }

    public void StartBlockSpawning() {
        foreach (BlockSpawner spawner in blockSpawners) {
            spawner.StartSpawning();
        }
    }

    public void GameOver() {
        playerController.Stop();
        camCon.enabled = false;

        loadingCanvas.SetActive(true);
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
        yield return new WaitUntil(() => !camCon.transitioning);
        playerController.Unfreeze();
        collectibleSpawner.Init();

        StartCoroutine(GoSequence());
    }

    IEnumerator GoSequence() {
        goText.transform.position = playerController.transform.position;
        for (int i = 0; i < 2; i++) {
            goText.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            goText.SetActive(false);
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void Restart() {
        SceneManager.LoadScene(0);
    }
}
