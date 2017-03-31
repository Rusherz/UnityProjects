using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour {
	
	public Dictionary<string, Sprite> Sprites;

	public virtual void LoadSprites (){
		Sprites = new Dictionary<string, Sprite> ();
		Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Objects/");
		foreach (Sprite s in sprites) {
			//Debug.Log (s);
			Sprites[s.name] = s;
		}
	}

}
