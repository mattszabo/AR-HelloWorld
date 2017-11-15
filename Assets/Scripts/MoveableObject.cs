using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableObject : MonoBehaviour {

	public Color defaultColour;
	public Color selectedColour;

	private Material material;

	void Start() {
		material = GetComponent<Renderer>().material;
	}

	void OnTouchDown() {
		Debug.Log("Matt - touch down!");
		material.color = selectedColour;
	}

	void OnTouchStay() {
		Debug.Log("Matt - ...");
		material.color = selectedColour;
	}
	
	void OnTouchMoved(Vector3 Point) {
		Debug.Log("Matt - move " + Point.ToString());
		material.color = selectedColour;
	}

	void OnTouchUp() {
		Debug.Log("Matt - touch up!");
		material.color = defaultColour;
	}
}
