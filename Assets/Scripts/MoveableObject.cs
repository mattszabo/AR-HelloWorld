using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using GoogleARCore.HelloAR;

public class MoveableObject : MonoBehaviour {

	public Color defaultColour;
	public Color selectedColour;

	private bool isSelected;
	private Vector3 newPosition;
	private TrackedPlane cubePlane;
	private Material material;

	void Start() {
		isSelected = false;
		material = GetComponent<Renderer>().material;
	}

	void Update() {
		if(isSelected) {
			MoveObject();
		}
	}

	private void MoveObject() {
		transform.position = new Vector3(newPosition.x, transform.position.y, newPosition.z);
	}

	void OnTouchDown() {
		Debug.Log("Matt - touch down!");
		material.color = selectedColour;
		isSelected = true;
	}

	void OnTouchStay() {
		Debug.Log("Matt - ...");
	}
	
	void OnTouchMoved(Vector3 Point) {
		Debug.Log("Matt - move " + Point.ToString());
		newPosition = Point;
	}

	void OnTouchUp() {
		Debug.Log("Matt - touch up!");
		material.color = defaultColour;
		isSelected = false;
	}
}
