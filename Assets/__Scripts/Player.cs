using UnityEngine;
using System.Collections;

public enum Facing_e{
	up,
	down,
	left,
	right
}

public enum MoveState_e{
	idle,
	up,
	down,
	left,
	right
}


public class Player : MonoBehaviour {
	static public Player S;

	public float speed = 5f;

	public Facing_e facing;
	public MoveState_e moveState;


	Animator anim;

	public GameObject leg;

	// inventory (4 slots)
	// slot for big item (in hands)
	// public var for item in hands
	// public var for currently stealing something
	public bool stealing;
	// private variable for checking command to *try* to grab
	public bool itemInHand;
	bool grabbing;

	bool canGrab;

	public float grabDuration;

	void Awake(){
		S = this;
		anim = GetComponent<Animator>();
	}

	// Use this for initialization
	void Start () {
		stealing = false;
		canGrab = false;
		itemInHand = false;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		// first check for grab attempt
		if (grabbing){ // as in player is pressing the button
			if (canGrab){ // as in near enough the item
				StartCoroutine(Steal());
			}

			grabbing = false;
		}

		switch(moveState){
			case MoveState_e.idle:
				anim.SetBool ("WalkLeft", false);
				return;
				break;
			case MoveState_e.up:
				MoveU();
				break;
			case MoveState_e.down:
				MoveD();
				break;
			case MoveState_e.left:
				MoveL();
				break;
			case MoveState_e.right:
				MoveR();
				break;
		}
		// process movement
		// process facing (animation)
	}

	void Update(){
		if (PressingLeft()){
			moveState = MoveState_e.left;
		}
		else if (PressingRight()){
			moveState = MoveState_e.right;
		}
		else if (PressingUp()){
			moveState = MoveState_e.up;
		}
		else if (PressingDown()){
			moveState = MoveState_e.down;
		}
		else if (PressingActionButton()){
			grabbing = true;
		}
		else {
			moveState = MoveState_e.idle;
		}
	}

	// add param for item currently being nabbed
	IEnumerator Steal(){
		stealing = true;
		float startTime = Time.time;
		while (Time.time - startTime > grabDuration){
			yield return null;
		}
		stealing = false;
		Item itemRef = GetClosestItem();
		if (!itemRef.carryByHand) // add item to inventory
			PartyFoul.S.AddToInventory(GetClosestItem());
		else { // right now the only item you can do this with is the leg
			itemInHand = true;
			PartyFoul.S.cashExtracted += itemRef.value;
			
			leg.GetComponent<SpriteRenderer>().enabled = true;
			Destroy(itemRef.gameObject);
		}
		
	}

	Item GetClosestItem(){
		GameObject closest = null;
		GameObject[] objs = GameObject.FindGameObjectsWithTag("Item");
		float nearestDist = float.MaxValue;
		Vector3 delta;
		float dist;
		
		foreach(GameObject go in objs){
			delta = go.transform.position - transform.position;
			dist = delta.magnitude;
			if (dist < nearestDist){
				nearestDist = dist;
				closest = go;
			}
		}
		if (closest == null) return null;
		return closest.GetComponent<Item>();
	}

	void OnTriggerEnter(Collider coll){
		if (coll.tag == "Item"){
			canGrab = true;
		}
	}

	void OnTriggerExit(Collider coll){
		if (coll.tag == "Item"){
			canGrab = false;
		}
	}

	void MoveL(){
		float dt = Time.fixedDeltaTime;
		Vector3 pos = transform.position;
		pos.x -= dt * speed;
		transform.position = pos;
		anim.SetBool ("FaceUp", false);
		anim.SetBool ("FaceDown", false);
		anim.SetBool ("WalkLeft", true);
		anim.SetBool ("FaceLeft", true);
		if (transform.localScale.x < 0){
			FlipX();
		}
		facing = Facing_e.left;
	}

	void MoveR(){
		float dt = Time.fixedDeltaTime;
		Vector3 pos = transform.position;
		pos.x += dt * speed;
		transform.position = pos;
		anim.SetBool ("FaceUp", false);
		anim.SetBool ("FaceDown", false);
		anim.SetBool ("WalkLeft", true);
		anim.SetBool ("FaceLeft", true);
		if (transform.localScale.x > 0){
			FlipX();
		}
		facing = Facing_e.right;
	}

	void MoveU(){
		float dt = Time.fixedDeltaTime;
		Vector3 pos = transform.position;
		pos.y += dt * speed;
		transform.position = pos;
		anim.SetBool ("WalkLeft", false);
		anim.SetBool ("FaceLeft", false);
		anim.SetBool ("FaceUp", true);
		if (facing == Facing_e.down){
			//FlipY();
			anim.SetBool ("FaceDown", false);
		}
		facing = Facing_e.up;
	}

	void MoveD(){
		float dt = Time.fixedDeltaTime;
		Vector3 pos = transform.position;
		pos.y -= dt * speed;
		transform.position = pos;
		anim.SetBool ("WalkLeft", false);
		anim.SetBool ("FaceLeft", false);
		anim.SetBool ("FaceDown", true);
		if (facing == Facing_e.up){
			//FlipY();
			anim.SetBool ("FaceUp", false);

		}
		facing = Facing_e.down;
	}



	public void FlipX(){
		Vector3 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;
	}

	public void FlipY(){
		if (facing == Facing_e.up)
			facing = Facing_e.down;
		else if (facing == Facing_e.down)
			facing = Facing_e.up;
		Vector3 scale = transform.localScale;
		scale.y *= -1;
		transform.localScale = scale;
	}




	bool PressingLeft() {
		return Input.GetKey("a") || Input.GetKey(KeyCode.LeftArrow);
	}
	bool PressingRight() {
		return Input.GetKey("d") || Input.GetKey(KeyCode.RightArrow);
	}
	bool PressingUp(){
		return Input.GetKey ("w") || Input.GetKey (KeyCode.UpArrow);
	}
	bool PressingDown(){
		return Input.GetKey ("s") || Input.GetKey (KeyCode.DownArrow);
	}
	bool PressingA() {
		return Input.GetKeyDown("x") || Input.GetKeyDown(".");
	}
	bool PressingB() {
		return Input.GetKeyDown("z") || Input.GetKeyDown(",");
	}
	bool PressingActionButton(){
		return Input.GetKeyDown(KeyCode.Space);
	}
	bool PressingStart() {
		return Input.GetKeyDown(KeyCode.Return);
	}
	bool PressingSelect() {
		return Input.GetKeyDown(KeyCode.Tab);
	}
}
