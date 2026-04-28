using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleBlocks : MonoBehaviour
{

	public bool isBlue;
	private bool isBlueOn;
	private Collider2D collider;
	private SpriteRenderer renderer;
	public Sprite onSprite;
	public Sprite offSprite;
	// private Color semiVisible;
	private bool setBlueOn;
	private bool setYellowOn;

	// Use this for initialization
	void Start()
	{
		collider = GetComponent<Collider2D>();
		renderer = GetComponent<SpriteRenderer>();
		// semiVisible = new Color(1, 1, 1, 0.5f);
		setBlueOn = false;
		setYellowOn = false;
	}

	// Update is called once per frame
	void Update()
	{
		isBlueOn = SwitchController.instance.isBlueOn;

		if (!setBlueOn && !isBlueOn)
		{
			setBlueOn = true;
			setYellowOn = false;

			bool changed = false;

			if (isBlue)
			{
				collider.enabled = false;
				renderer.sprite = offSprite;
				changed = true;
			}
			else
			{
				collider.enabled = true;
				renderer.sprite = onSprite;
				changed = true;
			}
		}
		if (!setYellowOn && isBlueOn)
		{
			setBlueOn = false;
			setYellowOn = true;

			bool changed = false;

			if (isBlue)
			{
				collider.enabled = true;
				renderer.sprite = onSprite;
				changed = true;
			}
			else
			{
				collider.enabled = false;
				renderer.sprite = offSprite;
				changed = true;
			}
		}
	}
}