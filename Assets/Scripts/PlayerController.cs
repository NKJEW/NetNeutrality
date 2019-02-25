using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float speed;

    Vector2Int lastTile = Vector2Int.zero;
    float distFromTile = 0f;

    Vector2Int curMovement = Vector2Int.zero;
    Vector2Int quedMovement = Vector2Int.zero;

	void Start () {
		
	}
	
	void Update () {
        // test for inputs
        if (Input.GetKeyDown(KeyCode.W)) {
            AttemptMove(Vector2Int.up);
        } else if (Input.GetKeyDown(KeyCode.S)) {
            AttemptMove(Vector2Int.down);
        } else if (Input.GetKeyDown(KeyCode.D)) {
            AttemptMove(Vector2Int.right);
        } else if (Input.GetKeyDown(KeyCode.A)) {
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

	}

    void ExecuteMove (bool backedUp = false) { // uses qued movement
        distFromTile = 0f;
        Vector2Int newTile = backedUp ? lastTile : lastTile + curMovement;
        lastTile = newTile;
        transform.position = new Vector3(newTile.x, newTile.y, 0f);

        if (quedMovement != Vector2Int.zero) {
            curMovement = quedMovement;
            quedMovement = Vector2Int.zero;
        }

        // stop player if moving onto invalid tile
        if (!MoveValid(curMovement)) {
            curMovement = Vector2Int.zero;
        }
    }

    void AttemptMove (Vector2Int move)
    {
        if (!MoveValid(move)) {
            return;
        }

        quedMovement = move;
        if (distFromTile < 0.3f) {
            ExecuteMove(true);
        }
    }

    bool MoveValid (Vector2Int move) {
        Vector2Int tilePos = lastTile + move;
        if (PathfindingMap.CanWalkOnTile(new TilePos(tilePos.x, tilePos.y))) {
            return true;
        } else {
            return false;
        }
    }
}
