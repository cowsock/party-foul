using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PartyFoul : MonoBehaviour {
	static public PartyFoul S;

	public Text CREAM; // cash rules everything around me
	public int cashExtracted;
	int inventorySlotsUsed;

	public Text alert;

	public Sprite walletSprite;
	public Sprite legLampSprite;
	public Sprite scotchSprite;

	const int inventorySlots = 4;
	public Image[] inventory = new Image[inventorySlots];

	void Awake(){
		S = this;
	}

	public bool AddToInventory(Item item){
		if (inventorySlotsUsed >= inventorySlots) return false; // bail if unable to put in inv
		if (item.name == "Wallet"){
			inventory[inventorySlotsUsed++].sprite = walletSprite;
		}
		else if (item.name == "LegLamp"){
			inventory[inventorySlotsUsed++].sprite = legLampSprite;
		}
		else if (item.name == "Scotch"){
			inventory[inventorySlotsUsed++].sprite = scotchSprite;
		}
		else 
			return false;
		cashExtracted += item.value;
		// destroy Item from game world (yeah, this is sketchy but w/e)
		Destroy(item.gameObject);
		return true; // if we did anything but the else!
	}

	// Use this for initialization
	void Start () {
		inventorySlotsUsed = 0;
		cashExtracted = 0;
	}
	
	// Update is called once per frame
	void Update () {
		ShowTxt();
	}

	void ShowTxt(){
		CREAM.text = "Funds Extracted: $" + cashExtracted;
	}

	public void StartAlert(){
		alert.enabled = true;
	}
}
