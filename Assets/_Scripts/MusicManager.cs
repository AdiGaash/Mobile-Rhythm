using UnityEngine;

public class MusicManager : SingletonMonoBehaviour<MusicManager>
{
    public AudioSource musicSource;

    public float songTime => musicSource.time;

    void Start()
    {
        musicSource.Play(); // Start the song
    }
}