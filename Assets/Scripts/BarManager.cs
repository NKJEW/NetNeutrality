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
    RectTransform playHead;
    Image bufferBar;
    Image rightEnd;

    Color backColor;

    bool isRunning;

    SpawnManager spawner;
    CameraController cam;

    void Awake() {
        spawner = FindObjectOfType<SpawnManager>();
        cam = FindObjectOfType<CameraController>();

        bufferBar = transform.Find("BufferBar").GetComponent<Image>();
        playBar = transform.Find("PlayBar");
        playHead = transform.Find("Playhead").GetComponent<RectTransform>();
        rightEnd = transform.Find("RightEnd").GetComponent<Image>();

        backColor = rightEnd.color;

        barWidth = GetComponentInParent<CanvasScaler>().referenceResolution.x * 0.8f;
    }

    public void InitNumCollectibles(int num) {
        numCollectibles = num;
        curNumCollectibles = 0;
        rightEnd.color = backColor;
        UpdateBufferBar();
    }

    public void InitTime(float newTime) {
        maxTime = newTime;
        isRunning = false;
        UpdatePlayBar(0);
    }

    public void PickupCollectible() {
        if (!isRunning) {
            startTime = Time.time - ((float)curNumCollectibles / numCollectibles) * maxTime;
            isRunning = true;
            cam.ChangePixelation(1);
        }

        curNumCollectibles++;
        UpdateBufferBar();
    }

    void UpdateBufferBar() {
        bufferBar.transform.localScale = new Vector3(GetBufferFrac(), 1, 1);
        if (curNumCollectibles == numCollectibles) {
            rightEnd.color = bufferBar.color;
            isRunning = false;
            GameManager.instance.GameWin();
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

        if (isRunning) {
            float curFrac = Mathf.Clamp01(GetPlayFrac());
            UpdatePlayBar(curFrac);
        }
    }

    void UpdatePlayBar(float frac) {
        playBar.transform.localScale = new Vector3(frac, 1, 1);
        playHead.anchoredPosition = new Vector3(frac * barWidth, playHead.anchoredPosition.y, 0);

        if (frac >= GetBufferFrac() && curNumCollectibles > 0) {
            isRunning = false;
            cam.ChangePixelation(-1);
        }
    }
}
