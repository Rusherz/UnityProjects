using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World  {

	Tile[,] tiles;
	int width;
	int height;

	public World(int width = 100, int height = 100){
		this.width = width;
		this.height = height;

		tiles = new Tile[width, height];
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				tiles [x, y] = new Tile (this, x, y);
			}
		}

		Debug.Log ("World Generated with  " + (width * height) + " tiles.");
	}

	public Tile getTileAt(int x, int y){
		if (tiles [x, y] == null) {
			tiles [x, y] = new Tile (this, x, y);
		}

		return tiles [x, y];
	}

}
