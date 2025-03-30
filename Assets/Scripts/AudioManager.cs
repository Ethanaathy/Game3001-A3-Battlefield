using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public GameObject AudioPrefab;
    public static AudioManager Instance;

    public AudioSource backgroundMusic;
    public AudioClip musicClip; // Background music

    // New audio clips for win and lose sounds
    public AudioSource winSound;
    public AudioSource loseSound;
    public AudioSource StageChsngeSound;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Add AudioSource for background music if not set
            if (backgroundMusic == null)
            {
                backgroundMusic = gameObject.AddComponent<AudioSource>();
                backgroundMusic.clip = musicClip;
                backgroundMusic.loop = true;
                backgroundMusic.playOnAwake = true;
                backgroundMusic.volume = 0.5f;
                backgroundMusic.Play();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Play3D(AudioClip clip, Vector3 position)
    {
        GameObject audioGameObject = Instantiate(AudioPrefab, position, Quaternion.identity);
        AudioSource source = audioGameObject.GetComponent<AudioSource>();

        source.clip = clip;
        source.Play();

        Destroy(audioGameObject, clip.length);
    }

    public void StopMusic()
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.Stop();
        }
    }

    // New methods to play win and lose sounds
    public void PlayWinSound()
    {
        if (winSound != null)
        {
            winSound.Play();
        }
    }

    public void PlayLoseSound()
    {
        if (loseSound != null)
        {
            loseSound.Play();
        }
    }

    public void PlayStageChangeSound()
    {
        if (StageChsngeSound != null)
        {
            StageChsngeSound.Play();
        }
    }
}