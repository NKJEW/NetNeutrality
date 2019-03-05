using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public float followSharpness = 0.1f;
    Vector3 offset;
    public Transform target;

    [Header("Pixelation")]
    public float minDensity;
    public float maxDensity;
    public float transitionTime;
    public int direction = 1;
    public float timer = 0f;
    public float curPixelation = 1f;
    bool transitioning = false;
    PixelateImageEffect pixelationEffect;
    public AnimationCurve pixelationCurve;

    private void Start() {
        offset = transform.position - target.position;
        pixelationEffect = GetComponent<PixelateImageEffect>();
        SetPixelation(1f);
    }
    // Update is called once per frame
    void LateUpdate() {
        float blend = 1f - Mathf.Pow(1f - followSharpness, Time.deltaTime * 30f);

        transform.position = Vector3.Lerp(
               transform.position,
               target.transform.position + offset,
               blend);
    }

    public void ChangePixelation (int direction) {
        this.direction = direction;
        if (transitioning) {
            timer = transitionTime - timer;
        } else {
            timer = 0f;
        }

        transitioning = true;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.I)) {
            ChangePixelation(1);
        }
        if (Input.GetKeyDown(KeyCode.O)) {
            ChangePixelation(-1);
        }


        if (transitioning) {
            timer += Time.deltaTime;
            if (timer < transitionTime) {
                curPixelation += direction * (Time.deltaTime / transitionTime);
                SetPixelation(curPixelation);


            } else { // complete
                int targetPixelation = 1;
                if (direction == -1) {
                    targetPixelation = 0;
                }
                curPixelation = targetPixelation;

                SetPixelation(curPixelation);
                transitioning = false;
            }
        }
    }

    void SetPixelation (float pixelation) {
        float adjustedPixelation = pixelationCurve.Evaluate(Mathf.Clamp01(pixelation));
        pixelationEffect.pixelDensity = Mathf.RoundToInt(Mathf.Lerp (minDensity, maxDensity, adjustedPixelation));
    }
}
