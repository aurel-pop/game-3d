using UnityEngine;

// Make the player character blink for a random amount of time, after a random amount of time
namespace MonsterFlow
{
	public class PlayerBlinkingAnimation : MonoBehaviour
	{
		private Animator anim;
		private float    blinkingTimer;
		private float    randomBlinkingRange;
		private float    randomRange;
		private float    timer;

		private void Start()
		{
			this.anim = GetComponent<Animator>();

			this.randomRange = Random.Range(2.5f, 4f);

			//print(randomRange);
			this.randomBlinkingRange = Random.Range(0.19f, 0.25f);

			//print(randomBlinkingRange);
		}

		private void Update()
		{
			// Using timer for the amount of seconds that has passed
			this.timer += Time.deltaTime;

			//Debug.Log(timer);

			if (this.timer > this.randomRange)
			{
				this.anim.SetBool("IsClosingEyes", true);
				this.timer = 0;

				this.randomRange = Random.Range(2.5f, 4f);

				//print(randomRange);
			}

			if (this.anim.GetCurrentAnimatorStateInfo(0).IsName("Player_Blinking"))
			{
				this.blinkingTimer += Time.deltaTime;

				if (this.blinkingTimer > this.randomBlinkingRange)
				{
					this.anim.SetBool("IsClosingEyes", false);
					this.blinkingTimer = 0;
					this.timer         = 0;

					this.randomBlinkingRange = Random.Range(0.19f, 0.25f);

					//print(randomBlinkingRange);
				}
			}

			//timer = 0;
		}
	}
}