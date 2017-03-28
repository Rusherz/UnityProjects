using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(TDVisual))]
public class TileMapMouse : MonoBehaviour {

	public Color highlightColor;
	Color normalColor;
	Renderer renderer;
	Collider collider;
	TDVisual tileMap;

	// Use this for initialization
	void Start () {
		tileMap = GetComponent<TDVisual> ();
		renderer = GetComponent<Renderer> ();
		collider = GetComponent<Collider> ();
		normalColor = renderer.material.color;
	}
	
	// Update is called once per frame
	void Update () {
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hitInfo;

		if (collider.Raycast (ray, out hitInfo, Mathf.Infinity)) {
			int x = Mathf.FloorToInt (hitInfo.point.x / tileMap.tileSize);
			int z = Mathf.FloorToInt (hitInfo.point.z / tileMap.tileSize);
		} else {
			renderer.material.color = normalColor;
		}
	}

}
