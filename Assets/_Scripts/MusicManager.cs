using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioSource musicSource;

    public float songTime => musicSource.time;

    void Awake()
    {
        // Make sure there's only one MusicManager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        musicSource.Play(); // Start the song
    }
}