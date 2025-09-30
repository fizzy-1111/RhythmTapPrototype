
using UnityEngine;
using System.Collections.Generic;
public class RhythmNote : MonoBehaviour
{
  public Transform visual; 
  public double TargetTimeMs; 
  public double SpawnMs; 
  public float LaneHeight = 6f; 
  public float LeadMs = 1500f; 
  public int LaneIndex = 0;

  AudioController audioCtrl; 
  NoteSpawner spawner; 
  JudgmentService judge;

  public List<Sprite> musicSprites;
  public SpriteRenderer musicSpriteRenderer;
  
  public void Activate(NoteSpawner owner, JudgmentService judgeService, AudioController audioCtrl, int lane, double targetTimeMs, double spawnMs, float leadMs, float laneHeight)
  {
    spawner = owner; 
    judge = judgeService; 
    this.audioCtrl = audioCtrl;

    LaneIndex = lane; 
    TargetTimeMs = targetTimeMs; 
    SpawnMs = spawnMs; 
    LeadMs = leadMs; 
    LaneHeight = laneHeight;

    if (visual == null) visual = transform;

    if (musicSpriteRenderer != null && musicSprites != null && musicSprites.Count > 0)
    {
      int idx = UnityEngine.Random.Range(0, musicSprites.Count);
      musicSpriteRenderer.sprite = musicSprites[idx];
    }

    gameObject.SetActive(true);
  }
  void Update()
  {
    if (audioCtrl == null) return;
    double ms = audioCtrl.SongTimeMsDSP(); 
    double tUntil = TargetTimeMs - ms;
    float p = 1f - Mathf.Clamp01((float)((LeadMs - tUntil) / LeadMs));
    float y = Mathf.Lerp(0f, LaneHeight, p); if (visual != null) visual.localPosition = new Vector3(0f, y, 0f);
  }
  public void Despawn()
  {
    if (spawner != null) spawner.Despawn(this); else gameObject.SetActive(false);
  }
}
