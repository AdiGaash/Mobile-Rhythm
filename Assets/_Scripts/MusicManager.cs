using UnityEngine;

public class MusicManager : SingletonMonoBehaviour<MusicManager>
{
    public AudioSource musicSource;

    public bool initialized = false;
    public float songTime => musicSource.time;

    void Start()
    {
        // Get the AudioClip from the current SongData in LevelManager
        musicSource.clip = LevelManager.Instance.currentSongData.songAudio;
        musicSource.Play(); // Start the song
        initialized = true;
    }
}