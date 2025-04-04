using UnityEngine;
using UnityEngine.Pool;

public class NoteMovement : MonoBehaviour
{
    public float speed = 1.0f; // Speed at which the note moves
    private ObjectPool<GameObject> pool;

    public void Initialize(ObjectPool<GameObject> pool)
    {
        this.pool = pool;
    }

    void Update()
    {
        // Move the note towards the near Z position in local space
        transform.localPosition += Vector3.back * speed * Time.deltaTime;

        // Return the note to the pool when it reaches the near Z position
        if (transform.localPosition.z <= -6)
        {
            pool.Release(gameObject);
        }
    }
}