using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour
{
    public bool dontDestroyOnLoad;

    public float destroyAfterSeconds;
    float time;

    void Awake()
    {
        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }

    void Update()
    {
        // Destroy the object when at x seconds.
        this.time += Time.deltaTime;

        if (destroyAfterSeconds > 0 && this.time > this.destroyAfterSeconds)
        {
            Destroy(this.gameObject);
        }
    }
}
