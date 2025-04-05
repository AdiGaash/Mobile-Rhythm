using System.Collections.Generic;
using UnityEngine;

public static class BeatmapLoader
{
    public static BeatmapWrapper LoadFromJson(string jsonText)
    {
        return JsonUtility.FromJson<BeatmapWrapper>(jsonText);
    }
}