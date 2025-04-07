using UnityEngine;
using System.Collections.Generic;

public class AudioBeatDetector : MonoBehaviour
{
    public AudioClip audioClip;
    public float sampleWindow = 0.02f; // Duration of each chunk in seconds
    public float volumeThreshold = 0.1f; // Minimum amplitude to be considered a peak

    public List<float> detectedTimes = new List<float>();

    void Start()
    {
        DetectPeaks();
    }

    void DetectPeaks()
    {
        if (audioClip == null)
        {
            Debug.LogWarning("No audio clip assigned!");
            return;
        }

        int sampleRate = audioClip.frequency;
        int channels = audioClip.channels;
        int totalSamples = audioClip.samples;
        float[] samples = new float[totalSamples * channels];
        audioClip.GetData(samples, 0);

        int windowSize = Mathf.FloorToInt(sampleRate * sampleWindow);
        int numWindows = totalSamples / windowSize;

        for (int i = 0; i < numWindows; i++)
        {
            float sum = 0f;

            for (int j = 0; j < windowSize; j++)
            {
                int index = (i * windowSize + j) * channels;

                // Average over all channels
                for (int c = 0; c < channels; c++)
                {
                    sum += Mathf.Abs(samples[index + c]);
                }
            }

            float avg = sum / (windowSize * channels);
            if (avg >= volumeThreshold)
            {
                float time = i * sampleWindow;
                detectedTimes.Add(time);
            }
        }

        Debug.Log($"Detected {detectedTimes.Count} peaks in audio.");
    }
}