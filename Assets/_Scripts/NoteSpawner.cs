using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class NoteSpawner : MonoBehaviour
{
    public AudioSource musicSource;                  // Audio playing the song
    public TextAsset beatmapFile;                    // JSON file with note data
    public float spawnAheadTime = 2.0f;              // How early to spawn notes (in seconds)
    public GameObject tapNotePrefab;                 // Prefab for "tap" notes
    public GameObject holdNotePrefab;                // Prefab for "hold" notes
    public GameObject swipeNotePrefab;               // Prefab for "swipe" notes
    public Transform[] lanePositions;                // Each lane's position (based on index)

    private List<NoteData> beatmap = new List<NoteData>();
    private int nextNoteIndex = 0;

    private ObjectPool<GameObject> tapNotePool;
    private ObjectPool<GameObject> holdNotePool;
    private ObjectPool<GameObject> swipeNotePool;

    void Start()
    {
        // Parse the beatmap JSON file into a list of notes
        beatmap = BeatmapLoader.LoadFromJson(beatmapFile.text);
        musicSource.Play(); // Start the music

        // Initialize object pools
        tapNotePool = new ObjectPool<GameObject>(() => Instantiate(tapNotePrefab), note => note.SetActive(true), note => note.SetActive(false));
        holdNotePool = new ObjectPool<GameObject>(() => Instantiate(holdNotePrefab), note => note.SetActive(true), note => note.SetActive(false));
        swipeNotePool = new ObjectPool<GameObject>(() => Instantiate(swipeNotePrefab), note => note.SetActive(true), note => note.SetActive(false));
    }

    void Update()
    {
        float songTime = musicSource.time;

        // Spawn all upcoming notes that are within the spawnAheadTime
        while (nextNoteIndex < beatmap.Count &&
               beatmap[nextNoteIndex].time - songTime <= spawnAheadTime)
        {
            SpawnNote(beatmap[nextNoteIndex]);
            nextNoteIndex++;
        }
    }
    void SpawnNote(NoteData note)
    {
        ObjectPool<GameObject> poolToUse = null;

        // Choose the correct pool based on the note type
        switch (note.type)
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

        if (poolToUse == null || note.lane >= lanePositions.Length) return;

        // Get the note from the pool
        GameObject noteObject = poolToUse.Get();

        // Set the parent of the note to the lane's transform
        noteObject.transform.SetParent(lanePositions[note.lane]);

        // Set the local position of the note to (0, 0, 1)
        noteObject.transform.localPosition = new Vector3(0, 1.2f, 6);
        noteObject.transform.localRotation = Quaternion.identity;

        // Initialize the NoteMovement script
        NoteMovement noteMovement = noteObject.GetComponent<NoteMovement>();
        noteMovement.Initialize(poolToUse);
    }
}