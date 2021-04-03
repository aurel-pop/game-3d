using UnityEngine;

public class SetInactiveAfterTime : MonoBehaviour
{
    public float maxTime;

    float time;

    void Update()
    {
        // Destroy the object when at maxTime seconds.
        this.time += Time.deltaTime;

        if (this.time > this.maxTime) this.gameObject.SetActive(false);
    }
}
