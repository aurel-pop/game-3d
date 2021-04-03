using UnityEngine;

namespace MonsterFlow
{
	/// <summary>
	///    Move an object towards the origin.
	/// </summary>
	public class MoveToOrigin : MonoBehaviour
	{
		/// <summary> Base movement speed of the object. </summary>
		public float speed;

		// Movement vector towards the center position.
		private Vector3 moveDirection;

		private void Start()
		{
			// Calculate direction to center
			this.moveDirection = (Vector3.zero - this.transform.position).normalized;
		}

		private void Update()
		{
			// Move enemy towards center position
			this.transform.Translate(this.moveDirection *
											 (this.speed * GameController.instance.CurrentSpeedMultiplier * Time.deltaTime));
		}
	}
}