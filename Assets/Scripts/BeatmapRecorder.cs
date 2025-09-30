using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class BeatmapRecorder : MonoBehaviour
{
    [SerializeField] AudioController audioCtrl;
    [SerializeField] string outputFileName = "recorded_minimap.json";
    [SerializeField] int lanes = 3;
    [SerializeField] KeyCode recordKey = KeyCode.Space;
    [SerializeField] bool recordMouseButton = false;
    [SerializeField] int exportOffsetMs = 0;
    [SerializeField] int minGapMs = 90;
    [SerializeField] bool isRecording = false;

    readonly List<MiniNote> recorded = new List<MiniNote>();
    int lastMs = 0;

    void Awake()
    {
        if (audioCtrl == null) audioCtrl = FindObjectOfType<AudioController>();
        lanes = Mathf.Max(1, lanes);
    }

    void Update()
    {
        if (!isRecording) return;
        bool pressed = Input.GetKeyDown(recordKey) || (recordMouseButton && Input.GetMouseButtonDown(0));
        if (!pressed) return;


        double tMsD = audioCtrl != null ? audioCtrl.SongTimeMsDSP() : Time.time * 1000.0;
        int tMs = Mathf.RoundToInt((float)tMsD);

        if (tMs < 0) return;
        Debug.Log("gap " + (tMs - lastMs) );
        if (tMs - lastMs < minGapMs) return;

        int lane = Random.Range(0, lanes);
        recorded.Add(new MiniNote { t = tMs, lane = lane });
        Debug.Log("recording note: " + tMs + " " + lane);
        lastMs = tMs;
    }

    [ContextMenu("Start Recording")]
    public void StartRecording()
    {
        isRecording = true; 
        audioCtrl.PlayWithDelayMs(0, 0);
        Debug.Log("BeatmapRecorder: recording started.");
    }
    [ContextMenu("Stop Recording")]
    public void StopRecording()
    {
        isRecording = false; 
        audioCtrl.source.Stop();
        Debug.Log("BeatmapRecorder: recording stopped.");
    }
    [ContextMenu("Toggle Recording")]
    public void ToggleRecording()
    {
        isRecording = !isRecording; Debug.Log($"BeatmapRecorder: recording {(isRecording ? "started" : "stopped")}.");
    }

    [ContextMenu("Save Recorded Beatmap JSON")]
    public void SaveRecorded()
    {
        if (recorded.Count == 0)
        {
            Debug.LogWarning("BeatmapRecorder: nothing to save.");
            return;
        }
        recorded.Sort((a, b) => a.t.CompareTo(b.t));
        var chart = new MiniChart { lanes = lanes, offsetMs = exportOffsetMs, notes = recorded.ToArray() };
        var json = JsonUtility.ToJson(chart, true);
        var dir = Path.Combine(Application.streamingAssetsPath, "Beatmaps");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, string.IsNullOrEmpty(outputFileName) ? "recorded_minimap.json" : outputFileName);
        File.WriteAllText(path, json);
        Debug.Log($"BeatmapRecorder: saved {recorded.Count} notes to {path}");
    }

    [ContextMenu("Clear Recorded Notes")]
    public void ClearRecorded()
    {
        recorded.Clear();
        lastMs = int.MinValue;
        Debug.Log("BeatmapRecorder: cleared recorded notes.");
    }
}


