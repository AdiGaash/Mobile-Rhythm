using System.Collections.Generic;
using UnityEngine;

public static class BeatmapLoader
{
    public static List<NoteData> LoadFromJson(string jsonText)
    {
        BeatmapWrapper wrapper = JsonUtility.FromJson<BeatmapWrapper>(jsonText);
        return wrapper.notes;
    }
}