using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    [System.Serializable]
    public struct LevelData {
        public Texture2D map;
        public float easyTime;
        public float hardTime;
    }

    public LevelData[] levels;
}
