using UnityEngine;

public class NoteObject : MonoBehaviour
{
    public float timeToHit; // When this note should be hit (in song time)
    public int lane;        // Which lane this note belongs to
    public bool isHit = false;

    public float moveSpeed = 5f; // For simple downward movement

    void Update()
    {
        // Move down over time (just an example movement)
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);

        // Optional: destroy if off-screen or too late
        if (!isHit && timeToHit - MusicManager.Instance.songTime < -0.5f)
        {
            Debug.Log("Missed note!");
            Destroy(gameObject);
        }
    }

    public float TimeOffset()
    {
        return Mathf.Abs(MusicManager.Instance.songTime - timeToHit);
    }

    public void Hit()
    {
        isHit = true;
        Debug.Log("Note hit!");
        Destroy(gameObject);
    }
}