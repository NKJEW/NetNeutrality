using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarManager : MonoBehaviour {
    float maxTime;
    int numCollectibles;

    int curNumCollectibles;
    float startTime;
    float barWidth;

    Transform playBar;
    Transform playHead;
    Image bufferBar;
    Image rightEnd;

    bool isStarted;

    SpawnManager spawner;
    CameraController cam;

    void Awake() {
        spawner = FindObjectOfType<SpawnManager>();
        cam = FindObjectOfType<CameraController>();

        bufferBar = transform.Find("BufferBar").GetComponent<Image>();
        playBar = transform.Find("PlayBar");
        playHead = transform.Find("Playhead");
        rightEnd = transform.Find("RightEnd").GetComponent<Image>();

        barWidth = Screen.width * 0.8f;
    }

    public void InitNumCollectibles(int num) {
        numCollectibles = num;
        curNumCollectibles = 0;
        UpdateBufferBar();
    }

    public void InitTime(float newTime) {
        maxTime = newTime;
        isStarted = false;
        UpdatePlayBar(0);
    }

    public void PickupCollectible() {
        if (!isStarted) {
            startTime = Time.time - ((float)curNumCollectibles / numCollectibles) * maxTime;
            isStarted = true;
            cam.ChangePixelation(1);
        }

        curNumCollectibles++;
        UpdateBufferBar();
    }

    void UpdateBufferBar() {
        bufferBar.transform.localScale = new Vector3(GetBufferFrac(), 1, 1);
        if (curNumCollectibles == numCollectibles) {
            rightEnd.color = bufferBar.color;
        }
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
            float curFrac = Mathf.Clamp01(GetPlayFrac());
            UpdatePlayBar(curFrac);
        }
    }

    void UpdatePlayBar(float frac) {
        playBar.transform.localScale = new Vector3(frac, 1, 1);
        playHead.transform.localPosition = new Vector3((frac - 0.5f) * barWidth, playHead.transform.localPosition.y, 0);

        if (frac >= GetBufferFrac() && curNumCollectibles > 0) {
            isStarted = false;
            cam.ChangePixelation(-1);
        }
    }
}
