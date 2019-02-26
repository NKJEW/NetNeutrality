using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public float followSharpness = 0.1f;
    Vector3 offset;
    public Transform target;

    private void Start() {
        offset = transform.position - target.position;
    }
    // Update is called once per frame
    void LateUpdate() {
        float blend = 1f - Mathf.Pow(1f - followSharpness, Time.deltaTime * 30f);

        transform.position = Vector3.Lerp(
               transform.position,
               target.transform.position + offset,
               blend);
    }
}
