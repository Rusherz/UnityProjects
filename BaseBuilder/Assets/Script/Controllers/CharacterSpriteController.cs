using UnityEngine;
using System.Collections.Generic;

public class CharacterSpriteController : MonoBehaviour {

	Dictionary<Character, GameObject> characterGameObjectMap;

	Dictionary<string, Sprite> characterSprites;

	World world {
		get { return WorldController.Instance.world; }
	}

	// Use this for initialization
	void Start () {
		
		LoadSprites();
		characterGameObjectMap = new Dictionary<Character, GameObject>();
		world.RegisterCharacterCreated(OnCharacterCreated);
		foreach (Character c in world.characters) {
			OnCharacterCreated (c);
		}
	}

	void LoadSprites() {
		characterSprites = new Dictionary<string, Sprite>();
		Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Character/Characters/alien_red/");

		foreach(Sprite s in sprites) {
			characterSprites[s.name] = s;
		}
	}

	public void OnCharacterCreated( Character c ) {
		//Debug.Log("OnCharacterCreated");

		GameObject char_go = new GameObject();

		characterGameObjectMap.Add( c, char_go );

		char_go.name = "Character";
		char_go.transform.position = new Vector3( c.X, c.Y, 0);
		char_go.transform.SetParent(this.transform, true);

		SpriteRenderer sr = char_go.AddComponent<SpriteRenderer>();
		sr.sprite = characterSprites["Char1_Right"];
		sr.sortingLayerName = "Character";

		c.RegisterOnChangedCallback( OnCharacterChanged );

	}

	void OnCharacterChanged( Character c ) {

		if(characterGameObjectMap.ContainsKey(c) == false) {
			Debug.LogError("OnCharacterChanged -- trying to change visuals for character not in our map.");
			return;
		}

		GameObject char_go = characterGameObjectMap[c];
		/*if (c.X > char_go.transform.position.x) {
			char_go.GetComponent<SpriteRenderer> ().sprite = characterSprites ["Char1_Right"];
		} else if (c.X < char_go.transform.position.x) {
			char_go.GetComponent<SpriteRenderer> ().sprite = characterSprites ["Char1_Left"];
		} else {

		}*/

		char_go.transform.position = new Vector3( c.X, c.Y, 0);
	}



}
