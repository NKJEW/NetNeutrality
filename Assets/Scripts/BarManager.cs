using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarManager : MonoBehaviour {
    float maxTime;
    int numCollectibles;

    int curNumCollectibles;
    float startTime;
    float barWidth;

    Transform playBar;
    Transform playHead;
    Transform bufferBar;

    bool isStarted;

    SpawnManager spawner;

    void Awake() {
        spawner = FindObjectOfType<SpawnManager>();

        bufferBar = transform.Find("BufferBar");
        playBar = transform.Find("PlayBar");
        playHead = transform.Find("Playhead");

        barWidth = Screen.width * 0.8f;
    }

    public void InitNumCollectibles(int num) {
        numCollectibles = num;
    }

    public void InitTime(float newTime) {
        maxTime = newTime;
    }

    public void PickupCollectible() {
        if (curNumCollectibles == 0) {
            startTime = Time.time;
            isStarted = true;
        }

        curNumCollectibles++;
        UpdateBufferBar();
    }

    void UpdateBufferBar() {
        bufferBar.transform.localScale = new Vector3(GetBufferFrac(), 1, 1);
    }

    float GetBufferFrac() {
        return (float)curNumCollectibles / numCollectibles;
    }

    float GetPlayFrac() {
        return (Time.time - startTime) / maxTime;
    }

    void LateUpdate() {
        //if (Input.GetKeyDown(KeyCode.K)) {
        //    PickupCollectible();
        //}

        if (isStarted) {
            UpdatePlayBar();
        }
    }

    void UpdatePlayBar() {
        float curFrac = Mathf.Clamp01(GetPlayFrac());
        playBar.transform.localScale = new Vector3(curFrac, 1, 1);
        playHead.transform.localPosition = new Vector3((curFrac - 0.5f) * barWidth, playHead.transform.localPosition.y, 0);

        if (curFrac >= GetBufferFrac()) {
            isStarted = false;
            GameManager.instance.GameOver();
        }
    }
}
