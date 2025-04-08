using UnityEngine;
using UnityEngine.Serialization;

public class LevelManager : SingletonMonoBehaviour<LevelManager>
{
    public SongData currentSongData; // Reference to the current SongData
    public LevelSettings levelSettings; // Assuming you have a LevelSettings class
    public Transform NoteLaneParent; 
    public GameObject lanePrefab;

    void Start()
    {
        // Initialize or load the currentSongData as needed
        CreateNoteslanes();
    }

    public void CreateNoteslanes()
    {
        // Destroy all existing child objects under NotelaneParent
        foreach (Transform child in NoteLaneParent)
        {
            Destroy(child.gameObject);
        }

        // Get the width of the parent object's MeshFilter
        MeshFilter meshFilter = NoteLaneParent.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("NoteLaneParent does not have a MeshFilter component!");
            return;
        }

        float parentWidth = meshFilter.sharedMesh.bounds.size.x * NoteLaneParent.localScale.x;

        // Calculate the width for each lane
        float laneWidth = parentWidth / levelSettings.numOfLanes;

        // Instantiate new lanes and adjust their scale and color
        for (int i = 0; i < levelSettings.numOfLanes; i++)
        {
            GameObject lane = Instantiate(lanePrefab, NoteLaneParent);
            lane.name = $"Lane_{i}";

            // Adjust the scale of the lane to match the calculated width
            Vector3 laneScale = lane.transform.localScale;
            laneScale.x = laneWidth / lanePrefab.GetComponent<MeshFilter>().sharedMesh.bounds.size.x;
            lane.transform.localScale = laneScale;

            // Position the lanes evenly
            float lanePositionX = -parentWidth / 2 + laneWidth / 2 + i * laneWidth;
            lane.transform.localPosition = new Vector3(lanePositionX, 0, 0);

            // Set the color of the lane's material
            Material laneMaterial = lane.GetComponent<Renderer>().material;
            Color laneColor = (i % 2 == 0) ? new Color(0.8f, 0.6f, 0.8f) : new Color(0.6f, 0.8f, 0.6f); // Light purple and light green
            laneMaterial.SetColor("_BaseColor", laneColor); // Assuming the shader uses "_BaseColor" for the BaseMap
        }
    }
}