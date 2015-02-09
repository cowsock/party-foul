using UnityEngine;
using System.Collections;

public class Drink{
	public float potency;

	public Drink (float poten){
		potency = poten;
	}
}


public class Drinkable : MonoBehaviour{
	public int drinksRemaining;
	public float drinkPotency;
	

	public Drink GetDrink(){
		--drinksRemaining;
		return new Drink(drinkPotency);

	}

	void Update(){
		if (drinksRemaining <= 0) {
			Environment.drinkSources.Remove(this);
			Destroy(gameObject);		
		}
	}

}
