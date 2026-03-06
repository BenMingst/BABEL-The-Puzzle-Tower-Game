using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //NEW

public class PlayerController : MonoBehaviour
{
	[SerializeField] private float BASE_SPEED = 250f;
	private Rigidbody2D rb;

	float currentSpeed;
	//NEW
	[SerializeField] private float JUMP_FORCE = 300f;
	//private bool isGrounded = false;

	// Start is called before the first frame update
	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		currentSpeed = BASE_SPEED;
	}

	
	//NEW attempt 2
	public IEnumerator SpeedChange(float newSpeed, float timeInSecs)
	{
		currentSpeed = newSpeed;
		yield return new WaitForSeconds(timeInSecs);
		currentSpeed = BASE_SPEED;
	}

	// Update is called once per frame
	void Update()
	{
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");

		/*
		if (horizontal < 0)
		{
			this.transform.rotation = new Quaternion(0, -1, 0, 0);
		}
		else
		{
			this.transform.rotation = new Quaternion(0, 0, 0, 0);
		}
		*/

		if (vertical > 0 && Mathf.Approximately(rb.linearVelocity.y, 0))
		{
			rb.AddRelativeForce(Time.deltaTime * new Vector2(0, JUMP_FORCE), ForceMode2D.Impulse);
		}
		rb.linearVelocity = new Vector2(Time.deltaTime * horizontal * currentSpeed, rb.linearVelocity.y);
	}
}