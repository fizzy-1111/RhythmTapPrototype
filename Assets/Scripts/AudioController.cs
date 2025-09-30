
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public AudioSource source;
    [Range(-200,200)] public int latencyOffsetMs = 0;
    double dspStart; int sampleRate;

    void Awake(){ sampleRate = AudioSettings.outputSampleRate; }
    public void Play(){ dspStart = AudioSettings.dspTime + 0.1; source.PlayScheduled(dspStart); }
    public void PlayWithDelayMs(double delayMs, double initialSpawnDelayMs)
    {
        double delaySec = delayMs * 0.001 + initialSpawnDelayMs*0.001;
        dspStart = AudioSettings.dspTime + delaySec;
        source.PlayScheduled(dspStart);
    }
    public double SongTimeMsDSP(){ return (AudioSettings.dspTime - dspStart)*1000.0 + latencyOffsetMs; }
    public double SongTimeMsFromSamples(){ return (double)source.timeSamples / sampleRate * 1000.0 + latencyOffsetMs; }
}
