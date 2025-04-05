using UnityEngine;
using System.Collections.Generic;
public class InputManager : MonoBehaviour
{
    public float perfectWindow = 0.1f; // +/- 0.1s
    public float goodWindow = 0.25f;   // +/- 0.25s
    public Camera mainCamera; // Needed for raycasting from touch
    public GUIManager guiManager; // Reference to the GUIManager

    void Update()
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                CheckHit(touch.position);
            }
        }

        // For testing in editor
        if (Input.GetMouseButtonDown(0))
        {
            CheckHit(Input.mousePosition);
        }
    }

    void CheckHit(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            NoteObject note = hit.collider.GetComponent<NoteObject>();
            if (note != null && !note.isHit)
            {
                float offset = note.TimeOffset();
                if (offset <= perfectWindow)
                {
                    Debug.Log("Perfect!");
                    guiManager.AddScore(note.scoreValue * 2); // Double score for perfect hit
                }
                else if (offset <= goodWindow)
                {
                    Debug.Log("Good!");
                    guiManager.AddScore(note.scoreValue); // Normal score for good hit
                }
                else
                {
                    Debug.Log("Too early/late");
                    return;
                }

                note.Hit(); // Mark and destroy
            }
        }
    }
}