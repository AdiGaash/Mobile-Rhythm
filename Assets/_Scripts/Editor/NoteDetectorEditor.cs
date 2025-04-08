using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class NoteDetectorEditor : EditorWindow
{
    private AudioClip audioClip;
    private string outputPath = "Assets/DetectedNotes.json";
    private float sampleInterval = 0.1f; // How often (in seconds) to analyze

    // New fields for importance score range
    private float minImportanceScore = 0.0f;
    private float maxImportanceScore = 1.0f;

    [MenuItem("Tools/Note Detector")]
    public static void ShowWindow()
    {
        GetWindow<NoteDetectorEditor>("Note Detector");
    }

    void OnGUI()
    {
        GUILayout.Label("Audio Note Detector", EditorStyles.boldLabel);

        // Audio clip input field
        audioClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", audioClip, typeof(AudioClip), false);

        // Interval for analyzing the audio
        sampleInterval = EditorGUILayout.FloatField("Sample Interval (s)", sampleInterval);

        // Output path for the JSON
        outputPath = EditorGUILayout.TextField("Output Path", outputPath);

        // Importance score range inputs
        minImportanceScore = EditorGUILayout.Slider("Min Importance Score", minImportanceScore, 0.0f, 1.0f);
        maxImportanceScore = EditorGUILayout.Slider("Max Importance Score", maxImportanceScore, 0.0f, 1.0f);

        // Button to analyze and export notes
        if (GUILayout.Button("Analyze & Export Notes"))
        {
            if (audioClip != null)
            {
                AnalyzeClip();
            }
            else
            {
                Debug.LogWarning("Please assign an AudioClip.");
            }
        }
    }

    private void AnalyzeClip()
    {
        float[] samples = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(samples, 0);

        int sampleRate = audioClip.frequency;
        int windowSize = 1024;
        float[] window = new float[windowSize];

        List<NoteEvent> notes = new List<NoteEvent>();
        Dictionary<string, int> repeatCounter = new Dictionary<string, int>();

        // Process the audio data
        for (float t = 0; t < audioClip.length; t += sampleInterval)
        {
            int startSample = (int)(t * sampleRate);
            if (startSample + windowSize >= samples.Length) break;

            for (int i = 0; i < windowSize; i++)
                window[i] = samples[startSample + i];

            // Perform FFT to get spectrum (simplified version)
            float[] spectrum = FFT(window);

            int maxIndex = 0;
            float maxValue = 0f;

            for (int i = 0; i < spectrum.Length; i++)
            {
                if (spectrum[i] > maxValue)
                {
                    maxValue = spectrum[i];
                    maxIndex = i;
                }
            }

            // Calculate frequency based on FFT
            float frequency = maxIndex * sampleRate / windowSize;
            string note = FrequencyToNote(frequency);

            // If the note is not empty, calculate the importance score and add it to the list
            if (!string.IsNullOrEmpty(note))
            {
                // Count how many times this note appeared
                if (!repeatCounter.ContainsKey(note))
                    repeatCounter[note] = 1;
                else
                    repeatCounter[note]++;

                float amplitude = maxValue; // crude amplitude
                float durationScore = sampleInterval / 0.5f;
                float amplitudeScore = Mathf.Clamp01(amplitude * 2f);
                float repeatBonus = repeatCounter[note] > 2 ? 0.5f : 0f;

                float importanceScore = amplitudeScore + durationScore + repeatBonus;

                // Filter based on the user-defined importance score range
                if (importanceScore >= minImportanceScore && importanceScore <= maxImportanceScore)
                {
                    // Check if the note is already in the list, if it is, update the end time
                    bool noteExists = false;
                    foreach (var existingNote in notes)
                    {
                        if (existingNote.note == note && existingNote.endTime == 0f)
                        {
                            existingNote.endTime = t;
                            noteExists = true;
                            break;
                        }
                    }

                    // Add the note with start time and importance score
                    notes.Add(new NoteEvent
                    {
                        startTime = t,
                        endTime = 0f, // Initially set end time to 0, we'll update it when the note ends
                        note = note,
                        importanceScore = Mathf.Clamp01(importanceScore)
                    });
                }
            }
        }

        // Set the end time for any remaining notes (those not updated in the loop)
        foreach (var note in notes)
        {
            if (note.endTime == 0f)
            {
                note.endTime = audioClip.length; // Assume it ends at the end of the clip
            }
            // Calculate duration based on start and end times
            note.duration = note.endTime - note.startTime;
        }

        // Convert the notes to JSON format
        string json = JsonHelper.ToJson(notes.ToArray(), true);

        // Save the JSON to the output path
        File.WriteAllText(outputPath, json);
        AssetDatabase.Refresh();

        // Log success message
        Debug.Log("Note detection complete. Output saved to: " + outputPath);
    }

    // FFT implementation using Unity's built-in math (naive version)
    private float[] FFT(float[] data)
    {
        float[] spectrum = new float[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            spectrum[i] = Mathf.Abs(data[i]); // Simplified for readability
        }
        return spectrum;
    }

    // Convert frequency to note (simplified)
    private string FrequencyToNote(float frequency)
    {
        if (frequency <= 0f) return null;

        float A4 = 440f;
        int noteIndex = Mathf.RoundToInt(12 * Mathf.Log(frequency / A4, 2));
        string[] notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
        int octave = (noteIndex + 9) / 12 + 4;
        int nameIndex = (noteIndex + 9) % 12;

        if (nameIndex < 0 || nameIndex >= notes.Length) return null;
        return notes[nameIndex] + octave;
    }

    // Helper class for JSON export
    [System.Serializable]
    public class NoteEvent
    {
        public float startTime;
        public float endTime;
        public string note;
        public float importanceScore;
        public float duration; // Added duration field
    }

    // JSON utility for easy array-to-JSON conversion
    public static class JsonHelper
    {
        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T> { Items = array };
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }
}
