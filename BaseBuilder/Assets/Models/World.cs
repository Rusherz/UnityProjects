using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World  {

	Tile[,] tiles;

	Dictionary<string, Furniture> InstalledObjectProto;

	Action<Furniture> InstalledObjectCreated;

	int width;
	public int Width {
		get {
			return width;
		}
	}

	int height;
	public int Height {
		get {
			return height;
		}
	}

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

		CreateInstalledObjectProtos ();
	}

	void CreateInstalledObjectProtos(){
		InstalledObjectProto = new Dictionary<string, Furniture> ();
		InstalledObjectProto.Add ("Wall", Furniture.CreateProto ("Wall", 0, 1, 1, true));

	}

	public void RandomTiles(){
		Debug.Log ("Randomizing tiles");
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (UnityEngine.Random.Range (0, 2) == 0) {
					tiles [x, y].Type = Tile.TileType.Empty;
				}else{
					tiles [x, y].Type = Tile.TileType.Floor;
				}
			}
		}
	}

	public Tile getTileAt(int x, int y){
		if (tiles [x, y] == null) {
			tiles [x, y] = new Tile (this, x, y);
		}

		return tiles [x, y];
	}

	public void PlaceInstalledObject(string ObjectType, Tile t){

		if (!InstalledObjectProto.ContainsKey (ObjectType)) {
			Debug.LogError ("Not an installed object proto");
			return;
		}

		Furniture obj = Furniture.PlaceObject (InstalledObjectProto [ObjectType], t);

		if (obj == null) {
			return;
		}
		if (InstalledObjectCreated != null) {
			InstalledObjectCreated (obj);
		}
	}

	public void RegisterInstalledObjectCreated(Action<Furniture> callFunc){
		InstalledObjectCreated += callFunc;
	}

	public void UnregisterInstalledObjectCreated(Action<Furniture> callFunc){
		InstalledObjectCreated -= callFunc;
	}

}