using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(MeshFilter))]
[RequireComponent (typeof(MeshRenderer))]
[RequireComponent (typeof(MeshCollider))]
public class TGMap : MonoBehaviour {

	MeshFilter meshFilter;
	MeshRenderer meshRenderer;
	MeshCollider meshCollider;

	public int size_x = 100;
	public int size_z = 50;
	public float tileSize = 1.0f;

	public Texture2D terrainTiles;
	public int tileRes;

	// Use this for initialization
	void Start () {
		meshFilter = GetComponent<MeshFilter> ();
		meshRenderer = GetComponent<MeshRenderer> ();
		meshCollider = GetComponent<MeshCollider> ();
		BuildMesh ();
	}

	Color[][] ChopUpTiles(){
		int numRows = terrainTiles.height / tileRes;
		int numTiles = terrainTiles.width / tileRes;
		Color[][] tiles = new Color[numTiles * numRows][];
		for (int y = 0; y < numRows; y++) {
			for (int x = 0; x < numTiles; x++) {
				tiles[y * numRows + x] = terrainTiles.GetPixels (x * tileRes, y * tileRes, tileRes, tileRes);
			}
		}
		return tiles;
	}

	public void BuildTexture(){
        int textWidth = size_x * tileRes;
		int textHeight = size_z * tileRes;
		Texture2D texture = new Texture2D (textWidth, textHeight);

		Color[][] tiles = ChopUpTiles ();

		for (int y = 0; y < size_z; y++) {
			for (int x = 0; x < size_x; x++) {
				Color[] p = tiles[Random.Range(0, 4)];
				texture.SetPixels (x * tileRes, y * tileRes, tileRes, tileRes, p);
			}
		}
		texture.Apply ();
		texture.filterMode = FilterMode.Point;
		meshRenderer.sharedMaterial.mainTexture = texture;
	}

	public void BuildMesh(){

		int numTiles = size_x * size_z;

		int vertSize_x = size_x + 1;
		int vertSize_z = size_z + 1;
		int numOfVertizies = vertSize_x * vertSize_z;

		int numOfTriangles = numTiles * 2;
		int[] triangles = new int[numOfTriangles * 3];

		Vector3[] verticies = new Vector3[numOfVertizies];
		Vector3[] normals = new Vector3[numOfVertizies];
		Vector2[] uv = new Vector2[numOfVertizies];

		int x, z;
		for (z = 0; z < vertSize_z; z++) {
			for (x = 0; x < vertSize_x; x++) {
				verticies [z * vertSize_x + x] = new Vector3 (x * tileSize, 0, -(z * tileSize));
				normals [z * vertSize_x + x] = Vector3.up;
				uv [z * vertSize_x + x] = new Vector3 ((float)x / size_x, 0, -((float)z / size_z));
			}
		}

		for (z = 0; z < size_z; z++) {
			for (x = 0; x < size_x; x++) {
				int squareIndex = z * size_x + x;
				int triIndex = squareIndex * 6;

				triangles [triIndex + 0] = z * vertSize_x + x;
				triangles [triIndex + 1] = z * vertSize_x + x + vertSize_x + 1;
				triangles [triIndex + 2] = z * vertSize_x + x + vertSize_x;

				triangles [triIndex + 3] = z * vertSize_x + x;
				triangles [triIndex + 4] = z * vertSize_x + x + 1;
				triangles [triIndex + 5] = z * vertSize_x + x + vertSize_x + 1;

			}
		}

		Mesh mesh = new Mesh ();
		mesh.vertices = verticies;
		mesh.triangles = triangles;
		mesh.normals = normals;
		mesh.uv = uv;

		meshCollider.sharedMesh = mesh;
		meshFilter.mesh = mesh;

		BuildTexture ();

	}

	// Update is called once per frame
	void Update () {
		
	}

}