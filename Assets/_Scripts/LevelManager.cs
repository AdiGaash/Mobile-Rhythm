using UnityEngine;

public class LevelManager : SingletonMonoBehaviour<LevelManager>
{
    public SongData currentSongData; // Reference to the current SongData
    public LevelSettings levelSettings; // Assuming you have a LevelSettings class
    
    void Start()
    {
        // Initialize or load the currentSongData as needed
    }
}