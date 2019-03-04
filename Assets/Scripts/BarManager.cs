using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarManager : MonoBehaviour {
    public float maxTime;
    public int numCollectibles;

    int curNumCollectibles;
    float startTime;
    float barWidth;

    Transform playBar;
    Transform playHead;
    Transform bufferBar;

    bool isStarted;

    void Awake() {
        bufferBar = transform.Find("BufferBar");
        playBar = transform.Find("PlayBar");
        playHead = transform.Find("Playhead");

        barWidth = Screen.width * 0.8f;
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
        bufferBar.transform.localScale = new Vector3((float)curNumCollectibles/numCollectibles, 1, 1);
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
        float curFrac = (Time.time - startTime) / maxTime;
        playBar.transform.localScale = new Vector3(curFrac, 1, 1);
        playHead.transform.localPosition = new Vector3((curFrac - 0.5f) * barWidth, playHead.transform.localPosition.y, 0);
    }
}
