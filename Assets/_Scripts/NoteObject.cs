using UnityEngine;
using UnityEngine.Pool;

public class NoteObject : MonoBehaviour
{
    public float timeToHit; // When this note should be hit (in song time)
    public int lane;        // Which lane this note belongs to
    public bool isHit = false;

    public float moveSpeed = 5f; // For simple downward movement
    private ObjectPool<GameObject> pool;

    public void Initialize(ObjectPool<GameObject> pool)
    {
        this.pool = pool;
    }

    void Update()
    {
        // Move down over time (just an example movement)
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);

        // Optional: return to pool if off-screen or too late
        if (!isHit && timeToHit - MusicManager.Instance.songTime < -0.5f)
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