using MonsterFlow.System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScenes : MonoBehaviour
{
    private BackgroundMusicSwitcher backgroundMusicSwitcher;

    private void Start()
    {
        backgroundMusicSwitcher = FindObjectOfType<BackgroundMusicSwitcher>();

        if (backgroundMusicSwitcher == null)
            print("Could not find BackgroundMusicSwitcher script");
    }

    void Update()
    {
        if (Input.touchCount > 0 || Input.anyKey)
        {
            if (backgroundMusicSwitcher != null)
                backgroundMusicSwitcher.SwitchBackgroundMusic();

            SceneManager.LoadScene("Game");
        }
    }
}
