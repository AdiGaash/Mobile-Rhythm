using UnityEngine;

[CreateAssetMenu(fileName = "SongData", menuName = "ScriptableObjects/SongData", order = 1)]
public class SongData : ScriptableObject
{
    public string songName;
    public AudioClip songAudio;
    public Sprite songImage;
    public TextAsset beatmapWrapperJson;
}