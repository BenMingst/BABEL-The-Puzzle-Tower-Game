using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour {

	private SpriteRenderer renderer;
	public Sprite blueSprite;
	public Sprite yellowSprite;
	private bool isBlueOn;
	public SwitchController switchController;
	private bool setBlue;
	private bool setYellow;

	void Start () {
		renderer = GetComponent<SpriteRenderer> ();
		setBlue = false;
		setYellow = false;
	}
	
	void Update () {
		isBlueOn = SwitchController.instance.isBlueOn;

		if (!setBlue && isBlueOn) {
            renderer.sprite = blueSprite;
            setBlue = true;
            setYellow = false;
		} if (!setYellow && !isBlueOn) {
            renderer.sprite = yellowSprite;
            setBlue = false;
            setYellow = true;
		}
	}

	public void TakeHit()
	{
		switchController.ToggleSwitch();
		// play switch sound
		if (SoundManager.instance != null)
			SoundManager.instance.PlayWorldClip(SoundManager.instance.switchSound, transform, 1f);
	}
}
