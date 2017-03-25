using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

public class WorldController : MonoBehaviour {

	public static WorldController Instance { get; protected set; }
	public World world { get; protected set; }

	static bool loadWorld = false;
	public bool loadedWorld = loadWorld;

	void OnEnable () {
		
		if(Instance != null) {
			Debug.LogError("There should never be two world controllers.");
		}
		Instance = this;

		if (loadWorld) {
			loadWorld = false;
			CreateLoadWorld ();
		} else {
			CreateNewWorld ();
		}
	}

	void Update(){
		if (world != null) {
			world.Update (Time.deltaTime);
		}
	}

	public Tile GetTileAtWorldCoord(Vector3 coord) {
		int x = Mathf.RoundToInt(coord.x);
		int y = Mathf.RoundToInt(coord.y);

		return world.GetTileAt(x, y);
	}

	public void OnNewWorlClick(){
		SceneManager.LoadScene (SceneManager.GetActiveScene().name);
	}

	public void OnSaveWorldClick(){

		XmlSerializer serializer = new XmlSerializer( typeof(World) );
		TextWriter writer = new StringWriter();
		serializer.Serialize(writer, world);
		writer.Close();
		PlayerPrefs.SetString("SaveGame00", writer.ToString());

	}

	public void OnLoadWorldClick(){
		loadWorld = true;
		SceneManager.LoadScene (SceneManager.GetActiveScene().name);
	}

	void CreateNewWorld(){
		world = new World(100, 100);
		Camera.main.transform.position = new Vector3( world.Width/2, world.Height/2, Camera.main.transform.position.z );
	}

	void CreateLoadWorld(){
		// Create a world from our save file data.
		XmlSerializer serializer = new XmlSerializer( typeof(World) );
		TextReader reader = new StringReader( PlayerPrefs.GetString("SaveGame00") );
		world = (World)serializer.Deserialize(reader);
		reader.Close();
		// Center the Camera
		Camera.main.transform.position = new Vector3( world.Width/2, world.Height/2, Camera.main.transform.position.z );

	}

}