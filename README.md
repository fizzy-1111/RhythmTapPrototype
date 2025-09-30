

### Timekeeping
- Using Unity’s audio time (DSP) as the one true clock.
- A note spawns when its hit time (plus offset/delay) is within the spawn lead time.
- Each note moves with simple math from time-left-to-hit; no physics, no drift.
- Judging uses the same clock, so spawn, motion, and scoring always match the music.

### Performance
- Notes are pooled. Pre-creating them and reuse them to avoid spikes from Instantiate/Destroy.
- Only active notes update each frame. Each does small math and sets its position once.
- Caching all needed references (lanes, services, renderers). No new allocations in Update.
- UI lane feedback uses DOTween. Stopping old animations before starting new ones to prevent leaks.
- On spawn, each note picks a random sprite from a list and sets it on a cached SpriteRenderer.

### If I had more time
- Audio: schedule more precisely, tune audio buffer settings, and smooth tiny jitters under heavy load.
- Pooling: one shared pool system for notes, VFX/SFX, and tween objects with fixed sizes.
- Content: use Addressables, sprite atlases, and easy theme/skin swapping at runtime.
- Latency: in-game calibration tools, per-device profiles, and live offset controls.
- Scale: use Unity Jobs/ECS for very large note counts and add offscreen culling.
- Quality: add tests for timing/judgment, replays/ghosts to tune difficulty.
