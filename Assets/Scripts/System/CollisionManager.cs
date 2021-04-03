using UnityEngine;

namespace MonsterFlow
{
	public class CollisionManager : MonoBehaviour
	{
		public GameObject explosion;
		public int        scoreValue;


        private void Awake()
		{
            // Alert if no gameController script could be found
            if (GameController.instance == null) Debug.Log("Cannot find 'GameController' script");
        }

		private void OnTriggerEnter2D(Collider2D other)
		{
            // Tomato collides with Player, Game Over
            if (other.gameObject.CompareTag("Player"))
            {
                GameController.instance.GameOver();
            }
			// Tomato collides with YellowMonster, score increases
			if (other.gameObject.CompareTag("YellowMonster"))
			{
				GameController.instance.UpdateScore(1);

				// Add VFX for exploding Tomato
				if (this.explosion != null) Instantiate(this.explosion, this.transform.position, this.transform.rotation);
			}

			Destroy(this.gameObject);
		}
    }
}