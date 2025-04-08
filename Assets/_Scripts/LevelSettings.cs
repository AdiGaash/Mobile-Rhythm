using UnityEngine;

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}

[CreateAssetMenu(fileName = "LevelSettings", menuName = "ScriptableObjects/LevelSettings", order = 1)]
public class LevelSettings : ScriptableObject
{
    public float spawnAheadTime = 2.0f;
    public Difficulty difficulty;
    public int numOfLanes = 4; // Default number of lanes
}