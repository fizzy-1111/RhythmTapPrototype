using UnityEngine;
public class GameController : MonoBehaviour
{
    public AudioController audioCtrl;
    public ChartLoader chart;
    public NoteSpawner spawner;
    public JudgmentService judgment;
    public UIController ui;
    public float endBufferMs = 1200f; double lastNoteTimeMs = 0; bool running = false;
    void Awake()
    {
        if (audioCtrl == null) audioCtrl = FindObjectOfType<AudioController>();
        if (chart == null) chart = FindObjectOfType<ChartLoader>();
        if (spawner == null) spawner = FindObjectOfType<NoteSpawner>();
        if (judgment == null) judgment = FindObjectOfType<JudgmentService>();
        if (ui == null) ui = FindObjectOfType<UIController>();
    }
    void Start()
    {
        if (chart == null || chart.Chart == null) return;
        int lanes = chart.Chart.lanes > 0 ? chart.Chart.lanes : 3;
        if (judgment != null) judgment.Initialize(lanes, ui);

        if (spawner != null)
        {
            spawner.audioCtrl = audioCtrl;
            spawner.chart = chart;
            spawner.judgment = judgment;
            int poolSize = chart.Chart.notes != null ? chart.Chart.notes.Length + 8 : 32;
            spawner.Prewarm(poolSize);
        }

        lastNoteTimeMs = GetLastNoteTimeMs() + (chart.Chart != null ? chart.Chart.offsetMs : 0) + (spawner != null ? spawner.initialSpawnDelayMs : 0);
        if (audioCtrl != null)
        {
            double delayMs = spawner != null ? spawner.initialSpawnDelayMs : 0.0;
            audioCtrl.PlayWithDelayMs(delayMs, spawner != null ? spawner.initialSpawnDelayMs : 0.0);
        }
        running = true;
    }
    void Update()
    {
        if (!running || audioCtrl == null) return;
        double now = audioCtrl.SongTimeMsDSP();
        bool timeOver = now >= lastNoteTimeMs + endBufferMs;
        bool spawningDone = spawner == null ? true : spawner.HasFinishedSpawning();
        bool allJudged = judgment == null ? true : judgment.RemainingNotes() == 0;
        if (timeOver && spawningDone && allJudged)
        {
            running = false;
            if (ui != null && judgment != null) ui.ShowResults(judgment.CurrentScore(), judgment.MaxCombo());
        }
    }
    double GetLastNoteTimeMs()
    {
        if (chart == null || chart.Chart == null || chart.Chart.notes == null || chart.Chart.notes.Length == 0) return 0;
        double last = chart.Chart.notes[0].t;
        for (int i = 1; i < chart.Chart.notes.Length; i++) { if (chart.Chart.notes[i].t > last) last = chart.Chart.notes[i].t; }
        return last;
    }
}


