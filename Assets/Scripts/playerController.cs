using UnityEngine;

public class playerController : MonoBehaviour
{
	public gameManager gm;
	public float velocity = 1;
	public Rigidbody2D rb;

	public scoreManager scoreManager;

	// Start is called before the first frame update
	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	private void Update()
	{
		//if clic gauche
		if (Input.GetMouseButtonDown(0))
		{
			//jump
			rb.velocity = Vector2.up * velocity;
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.collider.tag == "death")
		{
			gm.GameOver(scoreManager.score);
		}
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		if (collider.tag == "addScoreHitBox")
		{
			scoreManager.score += 1;
		}
	}
}