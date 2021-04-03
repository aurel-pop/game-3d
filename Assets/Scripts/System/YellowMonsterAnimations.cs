using UnityEngine;

namespace MonsterFlow
{
	/// <summary>
	///    Triggers the animation on the yellow monster.
	/// </summary>
	public class YellowMonsterAnimations : MonoBehaviour
	{
		private static readonly int isSleeping = Animator.StringToHash("IsSleeping");
		/// <summary> Playback speed of the animator. </summary>
		public float animationSpeed;
		private Animator animator;
		public AudioClip[] eatingSounds;

		private AudioSource source;
		public float lowPitchRange;
		public float highPitchRange;
        private int lastSound;

		private void Start()
		{
			this.animator       = GetComponent<Animator>();
			this.animator.speed = this.animationSpeed;

			this.source = GetComponent<AudioSource>();
		}

		// Trigger sleeping animation after colliding with enemies
		private void OnTriggerEnter2D(Collider2D other)
		{
			if (other.CompareTag("Tomato"))
			{
				this.animator.SetTrigger(isSleeping);

				this.source.pitch = Random.Range(lowPitchRange, highPitchRange);
				int i = Random.Range(0, eatingSounds.Length - 1);

                while (this.lastSound == i)
                    i = Random.Range(0, eatingSounds.Length - 1);

				this.source.PlayOneShot(eatingSounds[i]);
                this.lastSound = i;
			}
		}
	}
}