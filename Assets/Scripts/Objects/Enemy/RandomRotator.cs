using UnityEngine;

// Move sprites clockwise
namespace MonsterFlow
{
	public class RandomRotator : MonoBehaviour
	{
		public float rotationSpeed;

		private void FixedUpdate() { this.transform.Rotate(Vector3.forward * this.rotationSpeed); }
	}
}