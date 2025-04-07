using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class AudioBeatDetectorWindow : EditorWindow
{
    private AudioClip audioClip;
    private float volumeThreshold = 0.1f;
    private List<float> detectedTimes = new List<float>();
    private string fileName = "PeakTimes";
    private string subFolder = "DetectedPeaks";
    private int peakCount = 0;

    [MenuItem("Tools/Audio Beat Detector")]
    public static void ShowWindow()
    {
        GetWindow<AudioBeatDetectorWindow>("Audio Beat Detector");
    }

    private void OnGUI()
    {
        GUILayout.Label("Audio Beat Detector", EditorStyles.boldLabel);

        audioClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", audioClip, typeof(AudioClip), false);
        volumeThreshold = EditorGUILayout.FloatField("Volume Threshold", volumeThreshold);
        fileName = EditorGUILayout.TextField("File Name", fileName);
        subFolder = EditorGUILayout.TextField("Subfolder", subFolder);

        if (GUILayout.Button("Run"))
        {
            DetectPeaks();
        }

        if (peakCount > 0)
        {
            EditorGUILayout.LabelField($"Detected Peaks: {peakCount}");
        }

        if (detectedTimes.Count > 0)
        {
            if (GUILayout.Button("Save to JSON"))
            {
                SaveToJson();
            }

            EditorGUILayout.LabelField("Detected Peaks:");
            foreach (float time in detectedTimes)
            {
                EditorGUILayout.LabelField(time.ToString("F2") + " seconds");
            }
        }
    }

    private void DetectPeaks()
    {
        detectedTimes.Clear();
        peakCount = 0;

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

        int windowSize = Mathf.FloorToInt(sampleRate * 0.02f); // 0.02f is the sample window duration
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
                float time = i * 0.02f; // 0.02f is the sample window duration
                detectedTimes.Add(time);
                peakCount++;
            }
        }

        Debug.Log($"Detected {peakCount} peaks in audio.");
    }

    private void SaveToJson()
    {
        string folderPath = Path.Combine("Assets", subFolder);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string json = JsonUtility.ToJson(new PeakTimesWrapper { times = detectedTimes }, true);
        string fullPath = Path.Combine(folderPath, fileName + ".json");

        File.WriteAllText(fullPath, json);
        Debug.Log("Peak times saved to " + fullPath);
    }

    [System.Serializable]
    private class PeakTimesWrapper
    {
        public List<float> times;
    }
}