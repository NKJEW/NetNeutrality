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
