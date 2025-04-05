using UnityEngine;
using TMPro;

public class GUIManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText; // Reference to the TextMeshPro object displaying the score
    private int score = 0;

    // Method to add score and update the score display
    public void AddScore(int value)
    {
        score += value;
        UpdateScoreDisplay();
    }

    // Method to update the score display
    private void UpdateScoreDisplay()
    {
        scoreText.text = "Score: " + score;
    }
}