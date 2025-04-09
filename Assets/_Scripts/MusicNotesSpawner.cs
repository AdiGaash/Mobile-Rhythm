using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


public class MusicNotesSpawner : MonoBehaviour
{
    public GameObject tapNotePrefab;                 // Prefab for "tap" notes
    public GameObject holdNotePrefab;                // Prefab for "hold" notes
    
    public Vector3 noteStartLocalPosition;
    public Vector3 noteEndLocalPosition;

    private List<MusicNote> musicNotes = new List<MusicNote>();
    private int nextNoteIndex = 0;
    private Transform[] lanePositions;

    private ObjectPool<GameObject> tapNotePool;
    private ObjectPool<GameObject> holdNotePool;
    private ObjectPool<GameObject> swipeNotePool;

    private int numOfLanes;
    private HashSet<string> uniqueNotes;
    
    private Dictionary<string, int> noteToLaneMap;


    void InitObjectPools()
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
    }
    private void Awake()
    {
        InitObjectPools();
        InitLanes();
    }

    private void InitLanes()
    {
        // Get the number of lanes from LevelSettings
        numOfLanes = LevelManager.Instance.levelSettings.numOfLanes;

        // Get the NoteLaneParent from LevelManager
        Transform noteLaneParent = LevelManager.Instance.NoteLaneParent;

        // Initialize lanePositions based on the children of NoteLaneParent
        lanePositions = new Transform[numOfLanes];
        for (int i = 0; i < numOfLanes; i++)
        {
            lanePositions[i] = noteLaneParent.GetChild(i);
            Debug.Log("lane name: " +lanePositions[i].gameObject.name);
        }
        
    }

    void Start()
    {
        InitMusicNotes();
    }

    
    //TODO: need to take care of difficulty factor in LevelSettings!
    void InitMusicNotes()
    {
        // Parse the JSON file into a list of music notes
        MusicNotesWrapper notesWrapper = 
            JsonUtility.FromJson<MusicNotesWrapper>(LevelManager.Instance.currentSongData.beatmapWrapperJson.text);
        
        // Check if the data was successfully parsed
        if (notesWrapper == null || notesWrapper.Items == null || notesWrapper.Items.Count == 0)
        {
            Debug.LogError("Failed to load music notes from JSON file or the file is empty.");
            return;
        }
        
        musicNotes = notesWrapper.Items;

        // Sort the notes by startTime
        musicNotes.Sort((note1, note2) => note1.startTime.CompareTo(note2.startTime));
        
        
        // Collect all unique MusicNote.note values
        uniqueNotes = new HashSet<string>();
        foreach (var note in musicNotes)
        {
            if (!string.IsNullOrEmpty(note.note))
            {
                uniqueNotes.Add(note.note);
            }
        }

        // Initialize the noteToLaneMap
        noteToLaneMap = new Dictionary<string, int>();
        List<string> uniqueNotesList = new List<string>(uniqueNotes);
        for (int i = 0; i < uniqueNotesList.Count; i++)
        {
            noteToLaneMap[uniqueNotesList[i]] = i % numOfLanes;
        }

        // Log the unique notes
        Debug.Log("Unique MusicNote.note values: " + string.Join(", ", uniqueNotes));
        
        // Debug output of noteToLaneMap
        foreach (var entry in noteToLaneMap)
        {
            Debug.Log($"Note: {entry.Key}, Lane: {entry.Value}");
        }
    }

    void Update()
    {
        if (!MusicManager.Instance.initialized) return;

        float songTime = MusicManager.Instance.musicSource.time;

        // Spawn notes based on the current song time
        while (nextNoteIndex < musicNotes.Count)
        {
            float noteSpawnTime = musicNotes[nextNoteIndex].startTime;

            if (noteSpawnTime <= songTime && noteSpawnTime > songTime - Time.deltaTime)
            {
                // Spawn the note
                SpawnMusicNote(musicNotes[nextNoteIndex]);
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
                nextNoteIndex++;
            }
        }
    }

    int ChooseLaneIndexForMusicNote(string noteName)
    {
        // Return the lane index for the given noteName
        if (noteToLaneMap.TryGetValue(noteName, out int laneIndex))
        {
            return laneIndex;
        }

        // Default to lane 0 if the noteName is not found
        return 0;
    }
    
    
    void SpawnMusicNote(MusicNote musicNote)
    {
        ObjectPool<GameObject> poolToUse = null;

        // Choose the correct pool based on the note type
        if(musicNote.duration < 3)   
            poolToUse = tapNotePool;
        else
            poolToUse = holdNotePool;

        if (poolToUse == null) return;
        
        GameObject noteObject = poolToUse.Get();
        
        // Determine the lane index (you can customize this logic)
        int laneIndex = ChooseLaneIndexForMusicNote(musicNote.note);

        noteObject.transform.parent = lanePositions[laneIndex];
        // Set the local position of the note to the start position
        noteObject.transform.localPosition = noteStartLocalPosition;

        // Set the note's properties
        NoteObject noteObjectScript = noteObject.GetComponent<NoteObject>();
        
        noteObjectScript.timeToHit = musicNote.startTime;
        
        noteObjectScript.scoreValue = CalcMusicNoteScore(musicNote.importanceScore);
        
        noteObjectScript.startLocalPosition = noteStartLocalPosition;
        noteObjectScript.endLocalPosition = noteEndLocalPosition;

        Debug.Log($"Spawning music note: {musicNote.note} at lane {laneIndex}");
    }

    
    // temp way to calc score... need more work
    int CalcMusicNoteScore(float importanceScore)
    {
        return (int)importanceScore * 2;
    }
}