using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleSpawnEffect : MonoBehaviour {
    public float lifeTime;
    public float maxSize;
    SpriteRenderer spriteRenderer;

    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(LifeSequence());
    }

    IEnumerator LifeSequence() {
        Vector3 startSize = transform.localScale;
        maxSize = Vector2.Distance(transform.position, Camera.main.transform.position) * 3f;
        Vector3 endSize = new Vector3(maxSize, maxSize, 1f);

        float p = 0f;
        while (p < 1f) {
            transform.localScale = Vector3.Lerp(startSize, endSize, p);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.Lerp(0.5f, 0, p));
            yield return new WaitForEndOfFrame();
            p += Time.deltaTime / lifeTime;
        }

        Destroy(gameObject);
    }
}
