using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    public bool loop = false;

    [HideInInspector]
    public AudioSource source;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField]
    private Sound[] sounds;

    [SerializeField]
    private int poolSize = 10;

    private Queue<AudioSource> audioSourcePool;
    private List<AudioSource> activeAudioSources;
    private AudioSource currentBGM;

    [SerializeField]
    private float masterVolume = 1f;

    [SerializeField]
    private float sfxVolume = 1f;

    [SerializeField]
    private float uiVolume = 1f;

    private Dictionary<string, Sound> soundDictionary;
    private Dictionary<string, float> soundCooldowns = new Dictionary<string, float>();
    private const float DEFAULT_COOLDOWN = 0.05f;

    private Dictionary<string, Coroutine> activeFadeOuts = new Dictionary<string, Coroutine>();
    private Dictionary<string, Coroutine> activeDelayedSounds = new Dictionary<string, Coroutine>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeSoundSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSoundSystem()
    {
        soundDictionary = new Dictionary<string, Sound>();
        audioSourcePool = new Queue<AudioSource>();
        activeAudioSources = new List<AudioSource>();

        GameObject audioSourceContainer = new GameObject("AudioSources");
        audioSourceContainer.transform.SetParent(transform);

        foreach (Sound sound in sounds)
        {
            if (!soundDictionary.ContainsKey(sound.name))
            {
                soundDictionary.Add(sound.name, sound);
            }
        }

        for (int i = 0; i < poolSize; i++)
        {
            AudioSource source = audioSourceContainer.AddComponent<AudioSource>();
            ConfigureAudioSource(source);
            audioSourcePool.Enqueue(source);
        }
    }

    private void ConfigureAudioSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.priority = 128;
        source.rolloffMode = AudioRolloffMode.Linear;
    }

    public void PlaySound(string soundName, bool isUI = false)
    {
        if (!soundDictionary.TryGetValue(soundName, out Sound sound)) return;

        if (soundCooldowns.TryGetValue(soundName, out float lastPlayTime))
        {
            if (Time.time - lastPlayTime < DEFAULT_COOLDOWN)
            {
                return;
            }
        }

        AudioSource audioSource = GetAudioSource();
        if (audioSource == null) return;

        soundCooldowns[soundName] = Time.time;

        audioSource.clip = sound.clip;
        audioSource.volume = sound.volume * (isUI ? uiVolume : sfxVolume) * masterVolume;
        audioSource.pitch = sound.pitch;
        audioSource.loop = sound.loop;
        audioSource.Play();

        if (!sound.loop)
        {
            StartCoroutine(ReleaseWhenFinished(audioSource));
        }
    }

    public void TryPlaySound(string soundName, bool isUI = false)
    {
        if (activeFadeOuts.TryGetValue(soundName, out Coroutine fadeCoroutine))
        {
            StopCoroutine(fadeCoroutine);
            activeFadeOuts.Remove(soundName);
            
            if (soundDictionary.TryGetValue(soundName, out Sound sound))
            {
                foreach (AudioSource source in activeAudioSources)
                {
                    if (source.clip == sound.clip)
                    {
                        source.volume = sound.volume * (isUI ? uiVolume : sfxVolume) * masterVolume;
                    }
                }
            }
            return;
        }

        if (IsPlaying(soundName)) return;
        PlaySound(soundName, isUI);
    }

    private bool IsPlaying(string soundName)
    {
        if (!soundDictionary.TryGetValue(soundName, out Sound sound)) return false;

        foreach (AudioSource source in activeAudioSources)
        {
            if (source.clip == sound.clip && source.isPlaying)
            {
                return true;
            }
        }
        return false;
    }

    public void PlayBGM(string soundName)
    {
        if (currentBGM != null && currentBGM.isPlaying)
        {
            ReleaseAudioSource(currentBGM);
            currentBGM = null;
        }
        if (!soundDictionary.TryGetValue(soundName, out Sound soundData)) return;

        PlaySound(soundName);
        foreach (AudioSource audioSource in activeAudioSources)
        {
            if (audioSource.clip == soundData.clip)
            {
                currentBGM = audioSource;
                break;
            }
        }
    }

    public void PlayRandomBGM()
    {
        string randomBGM = UnityEngine.Random.value < 0.5f ? "bgm1" : "bgm2";
        PlayBGM(randomBGM);
    }

    public void StopSound(string soundName)
    {
        if (!soundDictionary.TryGetValue(soundName, out Sound sound)) return;

        for (int i = activeAudioSources.Count - 1; i >= 0; i--)
        {
            AudioSource source = activeAudioSources[i];
            if (source.clip == sound.clip)
            {
                ReleaseAudioSource(source);
                if (source == currentBGM)
                {
                    currentBGM = null;
                }
            }
        }
    }

    public void FadeOutSound(string soundName, float duration = 0.5f)
    {
        if (!soundDictionary.TryGetValue(soundName, out Sound sound)) return;

        if (activeFadeOuts.TryGetValue(soundName, out Coroutine existingFade))
        {
            StopCoroutine(existingFade);
            activeFadeOuts.Remove(soundName);
        }

        foreach (AudioSource source in activeAudioSources)
        {
            if (source.clip == sound.clip && source.isPlaying)
            {
                Coroutine fadeCoroutine = StartCoroutine(FadeOut(source, duration, soundName));
                activeFadeOuts[soundName] = fadeCoroutine;
            }
        }
    }

    private IEnumerator FadeOut(AudioSource audioSource, float duration, string soundName)
    {
        float startVolume = audioSource.volume;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, timer / duration);
            yield return null;
        }

        activeFadeOuts.Remove(soundName);
        ReleaseAudioSource(audioSource);
    }

    public void PlaySoundDelayed(string soundName, int repeatCount = 1, float delayTime = 1f, float volumeMultiplier = 1f, bool isUI = false)
    {
        if (!soundDictionary.TryGetValue(soundName, out Sound sound)) return;

        if (activeDelayedSounds.TryGetValue(soundName, out Coroutine existing) && existing != null)
        {
            StopCoroutine(existing);
            activeDelayedSounds.Remove(soundName);
        }

        Coroutine delayedCoroutine = StartCoroutine(PlaySoundWithDelay(sound, repeatCount, delayTime, volumeMultiplier, isUI));
        activeDelayedSounds[soundName] = delayedCoroutine;
    }

    private IEnumerator PlaySoundWithDelay(Sound sound, int repeatCount, float delayTime, float volumeMultiplier, bool isUI)
    {
        for (int i = 0; i < repeatCount; i++)
        {
            AudioSource audioSource = GetAudioSource();
            if (audioSource == null) yield break;

            audioSource.clip = sound.clip;
            audioSource.volume = sound.volume * (isUI ? uiVolume : sfxVolume) * masterVolume * volumeMultiplier;
            audioSource.pitch = sound.pitch;
            audioSource.loop = false;
            audioSource.Play();

            StartCoroutine(ReleaseWhenFinished(audioSource));

            if (i < repeatCount - 1)
                yield return new WaitForSeconds(delayTime);
        }

        activeDelayedSounds.Remove(sound.name);
    }

    public void StopDelayedSound(string soundName)
    {
        if (activeDelayedSounds.TryGetValue(soundName, out Coroutine delayedCoroutine))
        {
            StopCoroutine(delayedCoroutine);
            activeDelayedSounds.Remove(soundName);
        }
    }

    private AudioSource GetAudioSource()
    {
        if (audioSourcePool.Count == 0)
        {
            for (int i = activeAudioSources.Count - 1; i >= 0; i--)
            {
                if (!activeAudioSources[i].isPlaying)
                {
                    AudioSource reclaimedSource = activeAudioSources[i];
                    activeAudioSources.RemoveAt(i);
                    return reclaimedSource;
                }
            }
            return null;
        }

        AudioSource availableSource = audioSourcePool.Dequeue();
        activeAudioSources.Add(availableSource);
        return availableSource;
    }

    private void ReleaseAudioSource(AudioSource source)
    {
        source.Stop();
        source.clip = null;
        activeAudioSources.Remove(source);
        audioSourcePool.Enqueue(source);
    }

    private IEnumerator ReleaseWhenFinished(AudioSource audioSource)
    {
        yield return new WaitUntil(() => !audioSource.isPlaying);
        ReleaseAudioSource(audioSource);
    }
}