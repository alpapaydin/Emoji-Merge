using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ParticleEffect
{
    public string name;
    public ParticleSystem prefab;
    [Range(0.1f, 5f)]
    public float scale = 1f;
    public bool autoDestroy = true;
}

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }

    [SerializeField]
    private ParticleEffect[] particles;

    private Dictionary<string, ParticleEffect> particleDictionary;
    private Dictionary<string, List<ParticleSystem>> activeParticles;
    private Dictionary<GameObject, List<ParticleSystem>> attachedParticles;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeParticleSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeParticleSystem()
    {
        particleDictionary = new Dictionary<string, ParticleEffect>();
        activeParticles = new Dictionary<string, List<ParticleSystem>>();
        attachedParticles = new Dictionary<GameObject, List<ParticleSystem>>();

        foreach (ParticleEffect particle in particles)
        {
            if (!particleDictionary.ContainsKey(particle.name))
            {
                particleDictionary.Add(particle.name, particle);
                activeParticles.Add(particle.name, new List<ParticleSystem>());
            }
        }
    }

    public ParticleSystem SpawnParticle(string particleName, Vector3 position, bool playOnSpawn = true)
    {
        if (!particleDictionary.TryGetValue(particleName, out ParticleEffect particleEffect))
            return null;

        ParticleSystem newParticle = Instantiate(particleEffect.prefab, position, Quaternion.identity);
        newParticle.transform.SetParent(transform);
        newParticle.transform.localScale = Vector3.one * particleEffect.scale;

        activeParticles[particleName].Add(newParticle);

        if (playOnSpawn)
        {
            newParticle.Play();
        }

        if (particleEffect.autoDestroy)
        {
            float lifetime = newParticle.main.duration;
            Destroy(newParticle.gameObject, lifetime);
            activeParticles[particleName].Remove(newParticle);
        }

        return newParticle;
    }

    public ParticleSystem AttachParticle(string particleName, GameObject target, Vector3 localPosition = default, bool playOnSpawn = true)
    {
        if (!particleDictionary.TryGetValue(particleName, out ParticleEffect particleEffect))
            return null;

        ParticleSystem newParticle = Instantiate(particleEffect.prefab, target.transform);
        newParticle.transform.localPosition = localPosition;
        newParticle.transform.localScale = Vector3.one * particleEffect.scale;

        if (!attachedParticles.ContainsKey(target))
        {
            attachedParticles[target] = new List<ParticleSystem>();
        }
        attachedParticles[target].Add(newParticle);
        activeParticles[particleName].Add(newParticle);

        if (playOnSpawn)
        {
            newParticle.Play();
        }

        return newParticle;
    }

    public void StopParticle(string particleName, bool immediate = false)
    {
        if (!activeParticles.TryGetValue(particleName, out List<ParticleSystem> particles))
            return;

        foreach (ParticleSystem particle in particles.ToArray())
        {
            if (particle != null)
            {
                if (immediate)
                {
                    Destroy(particle.gameObject);
                }
                else
                {
                    particle.Stop();
                    Destroy(particle.gameObject, particle.main.duration);
                }
            }
        }
        
        particles.Clear();
    }

    public void StopAttachedParticles(GameObject target, bool immediate = false)
    {
        if (!attachedParticles.TryGetValue(target, out List<ParticleSystem> particles))
            return;

        foreach (ParticleSystem particle in particles.ToArray())
        {
            if (particle != null)
            {
                string particleName = GetParticleName(particle);
                if (particleName != null && activeParticles.ContainsKey(particleName))
                {
                    activeParticles[particleName].Remove(particle);
                }

                if (immediate)
                {
                    Destroy(particle.gameObject);
                }
                else
                {
                    particle.Stop();
                    Destroy(particle.gameObject, particle.main.duration);
                }
            }
        }

        attachedParticles.Remove(target);
    }

    private string GetParticleName(ParticleSystem particle)
    {
        foreach (var kvp in particleDictionary)
        {
            if (kvp.Value.prefab.name == particle.name.Replace("(Clone)", "").Trim())
            {
                return kvp.Key;
            }
        }
        return null;
    }

    public void PauseParticle(string particleName)
    {
        if (!activeParticles.TryGetValue(particleName, out List<ParticleSystem> particles))
            return;

        foreach (ParticleSystem particle in particles)
        {
            if (particle != null)
            {
                particle.Pause();
            }
        }
    }

    public void ResumeParticle(string particleName)
    {
        if (!activeParticles.TryGetValue(particleName, out List<ParticleSystem> particles))
            return;

        foreach (ParticleSystem particle in particles)
        {
            if (particle != null)
            {
                particle.Play();
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var particleList in activeParticles.Values)
        {
            foreach (var particle in particleList)
            {
                if (particle != null)
                {
                    Destroy(particle.gameObject);
                }
            }
        }
    }
}