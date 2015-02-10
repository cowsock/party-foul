using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Environment : MonoBehaviour {
	static public Environment S;

	static public List<Drinkable> drinkSources;

	// how good is the current song?

	void Awake(){
		S = this;
		drinkSources = new List<Drinkable> ();
	
	}

	// Use this for initialization
	void Start () {
		GameObject[] drinks = GameObject.FindGameObjectsWithTag ("DrinkSource");
		foreach (GameObject go in drinks) {
			Drinkable dr = go.GetComponent<Drinkable>();
			if (dr != null){
				drinkSources.Add (dr);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
