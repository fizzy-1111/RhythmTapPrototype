
using UnityEngine;
using System.IO;
[System.Serializable] public class MiniNote { public int t; public int lane; }
[System.Serializable] public class MiniChart { public int lanes; public int offsetMs; public MiniNote[] notes; }
public class ChartLoader : MonoBehaviour
{
  public string chartFileName = "recorded_minimap.json";
  public MiniChart Chart;
  void Awake()
  {
    var p = Path.Combine(Application.streamingAssetsPath, "Beatmaps", chartFileName);
    Chart = JsonUtility.FromJson<MiniChart>(File.ReadAllText(p));
  }
}
