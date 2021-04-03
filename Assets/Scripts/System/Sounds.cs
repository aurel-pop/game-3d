using UnityEngine;

public class Sounds : MonoBehaviour
{
    public AudioClip[] soundClips;

    private AudioSource source;
    public float lowPitchRange;
    public float highPitchRange;


    private void Start()
    {
        this.source = GetComponent<AudioSource>();
    }

    public void PlaySound(int i)
    {
        this.source.pitch = Random.Range(lowPitchRange, highPitchRange);
        this.source.PlayOneShot(soundClips[i]);
    }
}
