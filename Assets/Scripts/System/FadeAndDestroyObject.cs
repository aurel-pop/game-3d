using TMPro;
using UnityEngine;

namespace MonsterFlow
{
	/// <summary>
	///    Creates a configurable fading effect on an object
	///    with a SpriteRenderer (-> can be on child objects).
	/// </summary>
	public class FadeAndDestroyObject : MonoBehaviour
	{
		/// <summary> Time in seconds, before the fading starts. </summary>
		public float waitBeforeFade;
		/// <summary> Duration of the fading. </summary>
		public float fadeTime;

		/// <summary> Amount the object moves within the fadeTime. </summary>
		public Vector2 fadeMovement;
		/// <summary> Amount the object rotates within the fadeTime. </summary>
		public float fadeRotation;
		/// <summary> Amount the object scales within the fadeTime. </summary>
		public Vector3 fadeScaling;

		private SpriteRenderer _renderer;
		private TextMeshPro    _textMesh;
		private float          percentageDone;

		private void Start()
		{
			this._renderer = GetComponentInChildren<SpriteRenderer>();
			this._textMesh = GetComponentInChildren<TextMeshPro>();

			// Destroy script component, if neither SpriteRenderer nor TextMeshPro is found.
			if (this._renderer is null && this._textMesh is null) Destroy(this.gameObject);

			if (this.waitBeforeFade > 0) this.percentageDone -= this.waitBeforeFade / this.fadeTime;
		}

		private void Update()
		{
			// Calculate the current frame time's percentage of the fadeTime.
			float timePercentage = Time.deltaTime / this.fadeTime;

			if (this.percentageDone >= 0f)
			{
				// Change transparency, position, rotation and scaling according to percentage.
				if (this._renderer) this._renderer.color -= new Color(0, 0, 0, timePercentage);
				if (this._textMesh) this._textMesh.color -= new Color(0, 0, 0, timePercentage);
				Transform transformRef                   = this.transform;
				transformRef.Translate(this.fadeMovement    * timePercentage);
				transformRef.Rotate(0, 0, this.fadeRotation * timePercentage);
				transformRef.localScale += this.fadeScaling * timePercentage;
			}

			// Save the amount that is done and destroy the object when at 100 %.
			this.percentageDone += timePercentage;
			if (this.percentageDone >= 1f) Destroy(this.gameObject);
		}
	}
}