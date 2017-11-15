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

	void OnTouchUp() {
		Debug.Log("Matt - touch up!");
		material.color = defaultColour;
	}
}
