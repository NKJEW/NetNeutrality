using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
    float speed = 0f;
    public float maxSpeed;
    public float acceleration;
    bool isStopped;
    bool alreadyMoved = false;

    // rotation
    public float turnSpeed;
    Quaternion targetRotation;
    Transform sprite;

    // position
    Vector2Int lastTile = Vector2Int.zero;
    float distFromTile = 0f;

    // inputs
    Vector2Int curMovement = Vector2Int.zero;
    Vector2Int quedMovement = Vector2Int.zero;

    MapLoader map;
    CollectibleSpawner collectibleSpawner;
    Animator animator;
    BarManager bar;

    public Text cashText;
    public int cash;

    void Awake() {
        map = FindObjectOfType<MapLoader>();
        bar = FindObjectOfType<BarManager>();
        collectibleSpawner = FindObjectOfType<CollectibleSpawner>();
        animator = GetComponent<Animator>();

        sprite = transform.Find("Sprite");

        sprite.gameObject.SetActive(false);

        UpdateCashText();
    }

    public void Init() {
        lastTile = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        ExecuteMove(false);
        sprite.gameObject.SetActive(true);
        isStopped = false;
        alreadyMoved = false;
    }

    void Update() {
        if (isStopped) {
            return;
        }

        // adjust speed
        if (speed < maxSpeed) {
            speed += acceleration * Time.deltaTime;
            if (speed > maxSpeed) {
                speed = maxSpeed;
            }
        }

        // test for inputs
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
            AttemptMove(Vector2Int.up);
        } else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            AttemptMove(Vector2Int.down);
        } else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            AttemptMove(Vector2Int.right);
        } else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            AttemptMove(Vector2Int.left);
        }


        // apply movement
        if (curMovement == Vector2.zero) {
            return;
        }

        distFromTile += speed * Time.deltaTime;
        if (distFromTile >= 1f) {
            ExecuteMove();
        } else {
            transform.position = transform.position + (new Vector3(curMovement.x, curMovement.y, 0f) * speed * Time.deltaTime);
        }

        // apply rotation
        ApplyRotation();

    }

    void ExecuteMove(bool backedUp = false) { // uses qued movement
        distFromTile = 0f;
        Vector2Int newTile = backedUp ? lastTile : lastTile + curMovement;
        lastTile = newTile;
        transform.position = new Vector3(newTile.x, newTile.y, 0f);

        if (quedMovement != Vector2Int.zero) {
            curMovement = quedMovement;
            quedMovement = Vector2Int.zero;
        }

        UpdateTargetRotation(curMovement);

        // stop player if moving onto invalid tile
        if (!MoveValid(curMovement)) {
            curMovement = Vector2Int.zero;
        }
    }

    void AttemptMove (Vector2Int move)
    {
        if (!alreadyMoved) { // tell game manager that player moved at the start of level
            GameManager.instance.StartBlockSpawning();
            alreadyMoved = true;
        }

        if (!MoveValid(move)) { // test immediate move
            // test future move
            Vector2Int nextTile = lastTile + curMovement;
            if (TileValid(nextTile)) {
                quedMovement = move;
            } else {
                return;
            }
        } else {
            quedMovement = move;
            if (distFromTile < 0.3f) {
                ExecuteMove(true);
            }
        }
    }

    bool TileValid(Vector2Int tile) {
        if (PathfindingMap.CanWalkOnTile(new TilePos(tile.x, tile.y))) {
            return true;
        } else {
            return false;
        }
    }

    bool MoveValid(Vector2Int move) {
        Vector2Int tilePos = lastTile + move;
        if (PathfindingMap.CanWalkOnTile(new TilePos(tilePos.x, tilePos.y))) {
            return true;
        } else {
            return false;
        }
    }

    void UpdateTargetRotation(Vector2Int move) {
        if (move == Vector2Int.up) {
            targetRotation = Quaternion.Euler(0f, 0f, 90f);
        } else if (move == Vector2Int.right) {
            targetRotation = Quaternion.identity;
        } else if (move == Vector2Int.down) {
            targetRotation = Quaternion.Euler(0f, 0f, 270f);
        } else if (move == Vector2Int.left) {
            targetRotation = Quaternion.Euler(0f, 0f, 180f);
        }
    }

    void ApplyRotation() {
        //float diff = Quaternion.Angle(targetRotation, sprite.rotation);
        sprite.rotation = Quaternion.RotateTowards(sprite.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    void LateUpdate() {

    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Terrain")) {
            TilePos tilePos = PathfindingMap.WorldToTilePos(other.transform.position);
            int tileId = map.GetTile(tilePos.x, tilePos.y).blockTypeId;
            if (tileId == 3) { // collectible
                Destroy(other.gameObject);
                collectibleSpawner.SpawnCollectible();
                bar.PickupCollectible();
            } else if (tileId == 2) { // throttle
                Throttle();
            } else if (tileId == 4) { //paywall
                map.RemoveObstacle(tilePos.x, tilePos.y);
                cash -= 1; //temp
                UpdateCashText();
                Destroy(other.gameObject);
            }
        }
    }

    void UpdateCashText() {
        cashText.text = "$" + cash.ToString();
    }

    public void Throttle() {
        speed *= 0.25f;
        animator.SetTrigger("Throttle");
    }

    public void Stop() {
        speed = 0;
        curMovement = Vector2Int.zero;
        quedMovement = Vector2Int.zero;
        sprite.rotation = Quaternion.identity;
        isStopped = true;
    }
}
