using UnityEngine;
using UnityEngine.Pool;

public class NoteObject : MonoBehaviour
{
    public float timeToHit; // When this note should be hit (in song time)
    public int lane;        // Which lane this note belongs to
    public bool isHit = false;
    public int scoreValue = 1;

    
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

        
      
        float t = (songTime - (timeToHit - spawnAheadTime)) / spawnAheadTime;
        
        
        t = Mathf.Clamp01(t); // Ensure t is clamped between 0 and 1

        Debug.Log($"songTime: {songTime}, timeToHit: {timeToHit}, spawnAheadTime: {spawnAheadTime}, t: {t}");
        // Update the local position of the note
        transform.localPosition = Vector3.Lerp(startLocalPosition, endLocalPosition, t);
       
        
        // Return to pool if off-screen or too late
        if (!isHit && t >= 1)
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