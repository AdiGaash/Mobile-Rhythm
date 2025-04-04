[System.Serializable]
public class NoteData {
    public float time;
    public string type; // tap, hold, swipe
    public int lane;
    public float duration; // only for hold notes
}