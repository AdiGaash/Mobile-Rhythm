using UnityEngine;
using UnityEngine.Serialization;

public class LevelManager : SingletonMonoBehaviour<LevelManager>
{
    public LevelSettings levelSettings;
    public SongData currentSongData;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Example usage of levelSettings
        Debug.Log("Spawn Ahead Time: " + levelSettings.spawnAheadTime);
    }

    // Update is called once per frame
    void Update()
    {

    }
}