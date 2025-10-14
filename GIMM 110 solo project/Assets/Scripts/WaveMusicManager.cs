using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class WaveMusicController : MonoBehaviour
{
    [Header("Music Tracks")]
    [Tooltip("Track played during active combat waves.")]
    public AudioClip waveMusic;

    [Tooltip("Track played between waves.")]
    public AudioClip calmMusic;

    [Header("Settings")]
    [Range(0f, 3f)] public float fadeDuration = 2f; // seconds
    [Range(0f, 1f)] public float musicVolume = 0.8f;

    private AudioSource waveSource;
    private AudioSource calmSource;

    private Coroutine currentFade;
    private bool isBetweenWaves = false;

    void Awake()
    {
        // Create two AudioSources so both tracks can play simultaneously
        waveSource = gameObject.AddComponent<AudioSource>();
        calmSource = gameObject.AddComponent<AudioSource>();

        waveSource.loop = true;
        calmSource.loop = true;

        waveSource.volume = 0f;
        calmSource.volume = 0f;

        waveSource.playOnAwake = false;
        calmSource.playOnAwake = false;
    }

    void Start()
    {
        // Assign clips if given
        if (waveMusic != null)
            waveSource.clip = waveMusic;

        if (calmMusic != null)
            calmSource.clip = calmMusic;

        // Start both songs muted (so they stay in sync)
        waveSource.Play();
        calmSource.Play();

        // Initialize based on current phase
        if (WaveManagerTMP.IsBetweenWaves)
            SetPhase(true, instant: true);
        else
            SetPhase(false, instant: true);
    }

    void Update()
    {
        // Listen for changes in phase from WaveManagerTMP
        if (WaveManagerTMP.IsBetweenWaves != isBetweenWaves)
        {
            isBetweenWaves = WaveManagerTMP.IsBetweenWaves;
            SetPhase(isBetweenWaves, instant: false);
        }
    }

    void SetPhase(bool betweenWaves, bool instant)
    {
        if (currentFade != null)
            StopCoroutine(currentFade);

        if (instant)
        {
            // Instantly set volumes (used on Start)
            waveSource.volume = betweenWaves ? 0f : musicVolume;
            calmSource.volume = betweenWaves ? musicVolume : 0f;
        }
        else
        {
            // Smooth transition
            currentFade = StartCoroutine(Crossfade(betweenWaves));
        }
    }

    IEnumerator Crossfade(bool toCalm)
    {
        float t = 0f;

        float startWave = waveSource.volume;
        float startCalm = calmSource.volume;

        float targetWave = toCalm ? 0f : musicVolume;
        float targetCalm = toCalm ? musicVolume : 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / fadeDuration);

            waveSource.volume = Mathf.Lerp(startWave, targetWave, lerp);
            calmSource.volume = Mathf.Lerp(startCalm, targetCalm, lerp);

            yield return null;
        }

        waveSource.volume = targetWave;
        calmSource.volume = targetCalm;
        currentFade = null;
    }
}

