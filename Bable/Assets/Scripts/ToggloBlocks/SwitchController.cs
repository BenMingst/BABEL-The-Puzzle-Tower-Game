using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchController : MonoBehaviour {
	public static SwitchController instance = null;
	public bool isBlueOn;
	
        void Awake() {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy(gameObject);
		}
        }

	public void ToggleSwitch () {
		isBlueOn = !isBlueOn;
	}
}
