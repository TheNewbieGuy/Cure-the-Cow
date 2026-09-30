using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [System.Serializable]
    public class SoundEntry
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float defaultVolume = 1f;
    }

    [Header("Sound Library")]
    [SerializeField] private List<SoundEntry> soundLibrary;

    [Header("Background Music")]
    [Tooltip("Name of the sound entry to play automatically as looping background music on start.")]
    [SerializeField] private string backgroundMusicName;
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 1f;

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 8;

    private Dictionary<string, SoundEntry> _lookup;
    private readonly List<AudioSource> _sources = new List<AudioSource>();
    private AudioSource _musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Build the lookup dictionary once[cite: 6]
        _lookup = new Dictionary<string, SoundEntry>();
        foreach (var entry in soundLibrary)
        {
            if (entry.clip == null) continue;
            if (_lookup.ContainsKey(entry.name))
            {
                Debug.LogWarning($"SFXManager: duplicate sound name '{entry.name}', skipping duplicate.");
                continue;
            }
            _lookup.Add(entry.name, entry);
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            _sources.Add(CreateSource());
        }

        // Setup a dedicated music source channel
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;
    }

    private void Start()
    {
        // Automatically play background music on start if specified
        if (!string.IsNullOrEmpty(backgroundMusicName))
        {
            PlayMusic(backgroundMusicName, musicVolume);
        }
    }

    private AudioSource CreateSource()
    {
        var src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        return src;
    }

    /// <summary>Play a sound by name, as registered in the Sound Library[cite: 6].</summary>
    public void PlaySFX(string soundName, float volumeMultiplier = 1f, float pitch = 1f)
    {
        if (!_lookup.TryGetValue(soundName, out SoundEntry entry))
        {
            Debug.LogWarning($"SFXManager: no sound registered with name '{soundName}'.");
            return;
        }

        AudioSource source = GetAvailableSource();
        source.pitch = pitch;
        source.PlayOneShot(entry.clip, entry.defaultVolume * volumeMultiplier);
    }

    /// <summary>Play a clip directly, bypassing the library[cite: 6].</summary>
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        AudioSource source = GetAvailableSource();
        source.pitch = pitch;
        source.PlayOneShot(clip, volume);
    }

    /// <summary>Play a looping background music track by name.</summary>
    public void PlayMusic(string soundName, float volumeMultiplier = 1f)
    {
        if (!_lookup.TryGetValue(soundName, out SoundEntry entry))
        {
            Debug.LogWarning($"SFXManager: no music track registered with name '{soundName}'.");
            return;
        }

        _musicSource.clip = entry.clip;
        _musicSource.volume = entry.defaultVolume * volumeMultiplier;
        _musicSource.Play();
    }

    /// <summary>Stop the background music.</summary>
    public void StopMusic()
    {
        if (_musicSource != null && _musicSource.isPlaying)
        {
            _musicSource.Stop();
        }
    }

    private AudioSource GetAvailableSource()
    {
        foreach (var s in _sources)
        {
            if (!s.isPlaying) return s;
        }

        var newSource = CreateSource();
        _sources.Add(newSource);
        return newSource;
    }
}