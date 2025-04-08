using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class NoteSpawner : MonoBehaviour
{
    public TextAsset beatmapFile;                    // JSON file with note data
    public GameObject tapNotePrefab;                 // Prefab for "tap" notes
    public GameObject holdNotePrefab;                // Prefab for "hold" notes
    public GameObject swipeNotePrefab;               // Prefab for "swipe" notes
    public Transform[] lanePositions;                // Each lane's position (based on index)
    public Vector3 noteStartLocalPosition = new Vector3(0, 1, 1.2f); // Start local position of the note
    public Vector3 noteEndLocalPosition = new Vector3(0, 1, -1.2f);  // End local position of the note

    private List<GameNoteData> beatmap = new List<GameNoteData>();
    private int nextNoteIndex = 0;

    private ObjectPool<GameObject> tapNotePool;
    private ObjectPool<GameObject> holdNotePool;
    private ObjectPool<GameObject> swipeNotePool;

    private void Awake()
    {
        // Initialize object pools
        tapNotePool = new ObjectPool<GameObject>(() => {
            GameObject note = Instantiate(tapNotePrefab);
            note.GetComponent<NoteObject>().Initialize(tapNotePool);
            return note;
        }, note => note.SetActive(true), note => {
            note.SetActive(false);
            note.transform.SetParent(this.transform);
        });

        holdNotePool = new ObjectPool<GameObject>(() => {
            GameObject note = Instantiate(holdNotePrefab);
            note.GetComponent<NoteObject>().Initialize(holdNotePool);
            return note;
        }, note => note.SetActive(true), note => {
            note.SetActive(false);
            note.transform.SetParent(this.transform);
        });

        swipeNotePool = new ObjectPool<GameObject>(() => {
            GameObject note = Instantiate(swipeNotePrefab);
            note.GetComponent<NoteObject>().Initialize(swipeNotePool);
            return note;
        }, note => note.SetActive(true), note => {
            note.SetActive(false);
            note.transform.SetParent(this.transform);
        });
    }

    void Start()
    {
        beatmapFile = LevelManager.Instance.currentSongData.beatmapWrapperJson;
        // Parse the beatmap JSON file into a BeatmapWrapper
        BeatmapWrapper beatmapWrapper = BeatmapLoader.LoadFromJson(beatmapFile.text);

        // Get the difficulty level from the LevelManager
        Difficulty difficulty = LevelManager.Instance.levelSettings.difficulty;

        // Populate the beatmap list based on the difficulty level
        switch (difficulty)
        {
            case Difficulty.Easy:
                beatmap.AddRange(beatmapWrapper.easyNotes);
                break;
            case Difficulty.Medium:
                beatmap.AddRange(beatmapWrapper.easyNotes);
                beatmap.AddRange(beatmapWrapper.mediumNotes);
                break;
            case Difficulty.Hard:
                beatmap.AddRange(beatmapWrapper.easyNotes);
                beatmap.AddRange(beatmapWrapper.mediumNotes);
                beatmap.AddRange(beatmapWrapper.hardNotes);
                break;
        }
        
        // Sort the beatmap by note time
        beatmap.Sort((note1, note2) => note1.time.CompareTo(note2.time));
    }

    void Update()
    {
        if (!MusicManager.Instance.initialized) return;

        float songTime = MusicManager.Instance.musicSource.time;

        // Spawn only the note that might be late to spawn on the next frame
        while (nextNoteIndex < beatmap.Count)
        {
            float noteSpawnTime = beatmap[nextNoteIndex].time - LevelManager.Instance.levelSettings.spawnAheadTime;

            if (noteSpawnTime <= songTime && noteSpawnTime > songTime - Time.deltaTime)
            {
                // Spawn the note
                Debug.Log("Spawning late note");
                SpawnNote(beatmap[nextNoteIndex]);
                nextNoteIndex++;
            }
            else if (noteSpawnTime > songTime)
            {
                // Stop processing further notes if the next note is not yet ready to spawn
                break;
            }
            else
            {
                // Skip notes that are already too late
                Debug.Log("Skipping missed note");
                nextNoteIndex++;
            }
        }
    }

    void SpawnNote(GameNoteData gameNote)
    {
        ObjectPool<GameObject> poolToUse = null;

        // Choose the correct pool based on the note type
        switch (gameNote.type)
        {
            case "tap":
                poolToUse = tapNotePool;
                break;
            case "hold":
                poolToUse = holdNotePool;
                break;
            case "swipe":
                poolToUse = swipeNotePool;
                break;
        }

        if (poolToUse == null || gameNote.lane >= lanePositions.Length) return;

        // Get the note from the pool
        GameObject noteObject = poolToUse.Get();

        // Set the parent of the note to the lane's transform
        noteObject.transform.SetParent(lanePositions[gameNote.lane]);

        // Set the local position of the note to the start position
        noteObject.transform.localPosition = noteStartLocalPosition;

        // Set the note's properties
        NoteObject noteObjectScript = noteObject.GetComponent<NoteObject>();
        noteObjectScript.timeToHit = gameNote.time;
        noteObjectScript.lane = gameNote.lane;
        noteObjectScript.startLocalPosition = noteStartLocalPosition;
        noteObjectScript.endLocalPosition = noteEndLocalPosition;
        noteObjectScript.spawnAheadTime = LevelManager.Instance.levelSettings.spawnAheadTime;
        
        Debug.Log("spawning note!");
    }
}