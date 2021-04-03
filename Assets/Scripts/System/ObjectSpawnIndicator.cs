using UnityEngine;

public class ObjectSpawnIndicator : MonoBehaviour
{
	private void Start() {}

	public void Setup(float rotation, Color hue = default)
	{
		Transform transformRef;
		(transformRef = this.transform).Rotate(0, 0, rotation);

		float angleInQuarter = rotation % 90f;
	}
}