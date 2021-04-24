﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField]
    bool disableMusic;

    [SerializeField, Range(0f, 1f)]
    float musicVolume = .5f;

    [SerializeField, Range(0f, 1f)]
    float sfxVolume = .5f;

    [SerializeField]
    AudioClip musicClip;

    [SerializeField]
    int poolSize = 10;

    List<AudioSource> sources;
    AudioSource musicSource;

    private void Start()
    {
        if (musicClip != null)
        {
            musicSource = SpawnAudioSource("MusicAudioSource");
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.clip = musicClip;

            if (!disableMusic)
                musicSource.Play();
        }

        AudioSource src;
        sources = new List<AudioSource>();
        for (int i = 0; i < poolSize; i++)
        {
            src = SpawnAudioSource($"AudioClip_Source_{i}");
            sources.Add(src);
        }
    }

    private void Update()
    {
        musicSource.volume = musicVolume;
        if (disableMusic)
            musicSource.Pause();
        else if (!musicSource.isPlaying)
            musicSource.Play();

        sources.ForEach(s => s.volume = sfxVolume);
    }

    private AudioSource SpawnAudioSource(string sourceName)
    {
        var go = new GameObject(sourceName);
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.volume = sfxVolume;

        return src;
    }

    AudioSource GetAvailableSource()
    {
        return sources.Where(s => !s.isPlaying).FirstOrDefault();
    }

    public AudioSource PlayClip(AudioClip clip)
    {
        var src = GetAvailableSource();
        if (src == null || clip == null)
            return null;

        src.volume = sfxVolume;
        src.clip = clip;
        src.Play();

        return src;
    }
}