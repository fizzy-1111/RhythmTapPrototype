
using UnityEngine;
using System.Collections.Generic;
public enum JudgeGrade { None, Good, Perfect, Miss }
public class JudgmentService : MonoBehaviour
{
  public AudioController audioCtrl;
  public UIController ui;
  public float perfectMs = 30f, goodMs = 80f;


  public int scorePerPerfect = 100, scorePerGood = 50;
  public KeyCode[] laneKeys = new KeyCode[3] { KeyCode.A, KeyCode.S, KeyCode.D };

  Queue<RhythmNote>[] laneQueues; int laneCount = 3; int score = 0, combo = 0, maxCombo = 0;

  public void Initialize(int lanes, UIController uiController)
  {
    laneCount = Mathf.Max(1, lanes); ui = uiController;
    laneQueues = new Queue<RhythmNote>[laneCount];

    for (int i = 0; i < laneCount; i++) laneQueues[i] = new Queue<RhythmNote>();
    score = 0; combo = 0; maxCombo = 0;

    if (ui != null) { ui.UpdateScore(score); ui.UpdateCombo(combo); }
    EnsureLaneKeysSize(laneCount);
  }

  void EnsureLaneKeysSize(int lanes)
  {
    if (laneKeys == null || laneKeys.Length < lanes)
    {
      var keys = new List<KeyCode>();
      keys.AddRange(new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K, KeyCode.L });
      laneKeys = new KeyCode[lanes];
      for (int i = 0; i < lanes; i++) { laneKeys[i] = i < keys.Count ? keys[i] : KeyCode.None; }
    }
  }
  public void RegisterSpawn(RhythmNote note)
  {
    int lane = Mathf.Clamp(note.LaneIndex, 0, laneCount - 1); laneQueues[lane].Enqueue(note);
  }

  public void OnDespawn(RhythmNote note) { }
  public float PerfectWindowMs() { return perfectMs; }
  public float GoodWindowMs() { return goodMs; }

  void Update()
  {
    if (laneQueues == null || audioCtrl == null) return;
    double nowMs = audioCtrl.SongTimeMsDSP();
    for (int lane = 0; lane < laneCount; lane++)
    {
      AutoMissPastNotes(lane, nowMs);
      UpdateLaneWindowBand(lane, nowMs);
      if (laneKeys != null && lane < laneKeys.Length && laneKeys[lane] != KeyCode.None && Input.GetKeyDown(laneKeys[lane]))
      {
        TryJudgeHit(lane, nowMs);
      }
    }
  }
  void UpdateLaneWindowBand(int lane, double nowMs)
  {
    if (ui == null) return;
    var q = laneQueues[lane];
    if (q.Count == 0)
    {
      ui.SetLaneWindowColor(lane, ui.bandMissColor); return;
    }
    
    var head = q.Peek();
    double delta = nowMs - head.TargetTimeMs;
    double abs = System.Math.Abs(delta);

    if (abs <= perfectMs)
    {
      ui.SetLaneWindowColor(lane, ui.bandPerfectColor);
    }
    else if (abs <= goodMs)
    {
      ui.SetLaneWindowColor(lane, delta < 0 ? ui.bandGoodEarlyColor : ui.bandGoodLateColor);
    }
    else
    {
      ui.SetLaneWindowColor(lane, ui.bandMissColor);
    }
  }
  void AutoMissPastNotes(int lane, double nowMs)
  {
    var q = laneQueues[lane];
    while (q.Count > 0)
    {
      var head = q.Peek();
      double lateMs = nowMs - head.TargetTimeMs;
      if (lateMs > goodMs)
      {
        q.Dequeue(); combo = 0;
        if (ui != null) { ui.UpdateCombo(combo); ui.ShowLaneFeedback(lane, "Miss", Color.gray); }
        head.Despawn();
      }
      else break;
    }
  }
  void TryJudgeHit(int lane, double nowMs)
  {

    var q = laneQueues[lane];
    if (q.Count == 0) return;

    var note = q.Peek();
    double delta = nowMs - note.TargetTimeMs;
    double abs = System.Math.Abs(delta);

    if (abs <= perfectMs)
    {
      q.Dequeue(); score += scorePerPerfect; combo++; if (combo > maxCombo) maxCombo = combo;
      if (ui != null)
      {
        ui.UpdateScore(score);
        ui.UpdateCombo(combo);
        ui.ShowLaneFeedback(lane, delta < 0 ? "Early" : "Perfect", delta < 0 ? new Color(1f, 0.9f, 0.3f) : Color.yellow);
      }
      note.Despawn();
    }
    else if (abs <= goodMs)
    {
      q.Dequeue(); score += scorePerGood; combo++; if (combo > maxCombo) maxCombo = combo;
      if (ui != null)
      {
        ui.UpdateScore(score);
        ui.UpdateCombo(combo);
        ui.ShowLaneFeedback(lane, delta < 0 ? "Early" : "Late", delta < 0 ? Color.cyan : Color.magenta);
      }
      note.Despawn();
    }
  }
  public int CurrentScore() { return score; }
  public int CurrentCombo() { return combo; }
  public int MaxCombo() { return maxCombo; }
  public int RemainingNotes()
  {
    if (laneQueues == null) return 0;
    int n = 0; for (int i = 0; i < laneQueues.Length; i++) n += laneQueues[i].Count;
    return n;
  }
}
