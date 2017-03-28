using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : MonoBehaviour {

    TDVisual map;
    public Vector2 playerSpawn;
    public GameObject player;

    // Use this for initialization
    void Start () {
        if (playerSpawn != null) {
            map = GameObject.FindObjectOfType<TDVisual>();
            map.playerSpawn = playerSpawn;
            map.BuildMesh();
        } else {
            Debug.Log("Set player spawn");
        }
        TDTile tile = map.GetTileAt((int)playerSpawn.x, (int)playerSpawn.y);
        Instantiate(player, new Vector3(tile.x, 0, tile.y), Quaternion.identity);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
