using UnityEngine;

public class BackgroundMusicSwitcher : MonoBehaviour
{
    public AudioClip BackgroundMusicGame;
    private AudioSource source;
    public float volumeDownStep;
    public float volumeUpStep;
    public float volumeCrossover;
    public float gameMusicVolume;
    private bool turnVolumeDown;
    private bool turnVolumeUp = true;

    void Start()
    {
        source = GetComponent<AudioSource>();

        source.volume = 0.5f;

        if (BackgroundMusicGame == null)
            print("Could not find BackgroundMusicGame");
    }

    void Update()
    {
        if (turnVolumeUp)
        {
            source.volume += volumeUpStep * Time.deltaTime;

            if (source.volume >= gameMusicVolume)
            {
                turnVolumeUp = false;
            }
        }

        if (turnVolumeDown)
        {
            source.volume -= volumeDownStep * Time.deltaTime;

            if (source.volume < volumeCrossover)
            {
                turnVolumeDown = false;

                // Switch BackgroundMusic
                source.clip = BackgroundMusicGame;
                source.Play();

                turnVolumeUp = true;
            }
        }
    }

    public void SwitchBackgroundMusic()
    {
        turnVolumeDown = true;
    }
}
