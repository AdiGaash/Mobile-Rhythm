using UnityEngine;
using UnityEngine.Pool;

public class NoteObject : MonoBehaviour
{
    public float timeToHit; // When this note should be hit (in song time)
    public int lane;        // Which lane this note belongs to
    public bool isHit = false;
    public int scoreValue = 1;

    public float moveSpeed = 5f; // For simple downward movement
    private ObjectPool<GameObject> pool;

    public Vector3 startLocalPosition; // Start local position of the note
    public Vector3 endLocalPosition;   // End local position of the note
    public float spawnAheadTime;       // How early the note was spawned

    public void Initialize(ObjectPool<GameObject> pool)
    {
        this.pool = pool;
    }

    void Update()
    {
        float songTime = MusicManager.Instance.songTime;
        float t = Mathf.Clamp01((spawnAheadTime - (timeToHit - songTime)) / spawnAheadTime);
        transform.localPosition = Vector3.Lerp(startLocalPosition, endLocalPosition, t);

        // Optional: return to pool if off-screen or too late
        if (!isHit && timeToHit - songTime < -0.5f)
        {
            Debug.Log("Missed note!");
            pool.Release(gameObject);
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
        pool.Release(gameObject);
    }
}