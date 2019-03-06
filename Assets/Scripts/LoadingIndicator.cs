using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingIndicator : MonoBehaviour {
    public int numPieces;
    public float speed;

    Transform center;
    GameObject original;
    GameObject[] pieces;

    public bool isBuffering;
    int curDisabledPiece;
    float nextSwitch;

    void Awake() {
        center = transform.Find("Center");
        original = center.GetChild(0).gameObject;
        GenerateLoadingIndicator();
    }

    void GenerateLoadingIndicator() {
        pieces = new GameObject[numPieces];
        pieces[0] = original;
        float radius = original.transform.localPosition.y * transform.localScale.y;

        for (int i = 1; i < numPieces; i++) {
            float angle = 2 * Mathf.PI * i / numPieces;
            pieces[i] = Instantiate(original, new Vector3(radius * Mathf.Cos(angle + (Mathf.PI / 2)), radius * Mathf.Sin(angle + (Mathf.PI / 2)), 0) + center.transform.position, Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg), center);
        }
    }

    void Update() {
        if (isBuffering) {
            if (Time.time > nextSwitch) {
                nextSwitch = Time.time + (1f / speed);
                pieces[curDisabledPiece].SetActive(true);
                curDisabledPiece = (curDisabledPiece + numPieces - 1) % numPieces;
                pieces[curDisabledPiece].SetActive(false);
            }
        }
    }
}
