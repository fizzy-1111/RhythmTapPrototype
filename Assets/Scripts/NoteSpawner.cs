
using UnityEngine;
using System.Collections.Generic;
public class NoteSpawner : MonoBehaviour
{
  public GameObject notePrefab; 
  public Transform[] laneParents; 
  
  public float spawnLeadTimeMs = 1500f; 
  public float laneHeight = 6f;
  public float initialSpawnDelayMs = 0f;

  public AudioController audioCtrl; 
  public ChartLoader chart; 
  public JudgmentService judgment;

  readonly Stack<RhythmNote> pool = new Stack<RhythmNote>(); 
  readonly List<RhythmNote> live = new List<RhythmNote>(); 
  int nextIndex = 0;

  public void Prewarm(int count)
  {
    for (int i = 0; i < count; i++)
    {
      var go = Instantiate(notePrefab, transform);
      go.SetActive(false);

      var note = go.GetComponent<RhythmNote>();
      if (note.visual == null) note.visual = note.transform;

      pool.Push(note);
    }
  }
  RhythmNote GetFromPool()
  {
    if (pool.Count > 0) return pool.Pop();
    var go = Instantiate(notePrefab, transform);
    go.SetActive(false);
    return go.GetComponent<RhythmNote>();
  }
  public void Despawn(RhythmNote note)
  {
    if (live.Contains(note)) live.Remove(note);
    note.transform.SetParent(transform, false);
    note.gameObject.SetActive(false);
    pool.Push(note);
    if (judgment != null) judgment.OnDespawn(note);
  }
  void Update()
  {
    if (chart == null || chart.Chart == null || audioCtrl == null) return;

    double ms = audioCtrl.SongTimeMsDSP();

    while (nextIndex < chart.Chart.notes.Length)
    {
      var n = chart.Chart.notes[nextIndex];
      double targetMs = n.t + chart.Chart.offsetMs + initialSpawnDelayMs;
      if (targetMs - ms <= spawnLeadTimeMs)
      {
        var lane = Mathf.Clamp(n.lane, 0, laneParents.Length - 1);

        var note = GetFromPool(); 
        note.transform.SetParent(laneParents[lane], false); 
        note.transform.localPosition = Vector3.zero;
        note.Activate(this, judgment, audioCtrl, lane, targetMs, ms, spawnLeadTimeMs, laneHeight);

        live.Add(note); if (judgment != null) judgment.RegisterSpawn(note); nextIndex++;
      }
      else break;
    }
  }
  public bool HasFinishedSpawning() { return chart != null && chart.Chart != null && nextIndex >= chart.Chart.notes.Length; }
  public int ActiveCount() { return live.Count; }
}
