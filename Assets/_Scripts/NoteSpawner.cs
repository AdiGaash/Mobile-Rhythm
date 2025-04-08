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
    public Vector3 noteStartLocalPosition = new Vector3(0, 1.2f, 1); // Start local position of the note
    public Vector3 noteEndLocalPosition = new Vector3(0, -1.2f, 1);  // End local position of the note

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
    }

    void Update()
    {
        if (MusicManager.Instance.initialized)
        {
            float songTime = MusicManager.Instance.musicSource.time;
            // Spawn all upcoming notes that are within the spawnAheadTime
            while (nextNoteIndex < beatmap.Count &&
                   beatmap[nextNoteIndex].time - songTime <= LevelManager.Instance.levelSettings.spawnAheadTime)
            {
                // Skip notes that have already passed their time
                if (beatmap[nextNoteIndex].time < songTime)
                {
                    nextNoteIndex++;
                    continue;
                }

                SpawnNote(beatmap[nextNoteIndex]);
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
    }
}