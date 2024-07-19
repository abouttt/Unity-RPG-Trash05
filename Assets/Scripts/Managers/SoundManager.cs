using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class SoundManager : SingletonBehaviour<SoundManager>
{
    private readonly List<AudioSource> _audioSources = new();

    protected override void Init()
    {
        base.Init();

        foreach (var typeName in Enum.GetNames(typeof(SoundType)))
        {
            var go = new GameObject(typeName);
            var audioSource = go.AddComponent<AudioSource>();
            _audioSources.Add(audioSource);
            go.transform.SetParent(transform);
        }

        _audioSources[(int)SoundType.BGM].loop = true;
    }

    private SoundManager() { }

    protected override void Dispose()
    {
        base.Dispose();

        Clear();
    }

    public void Play(SoundType audioType, string key)
    {
        ResourceManager.Instance.LoadAsync<AudioClip>(key, audioClip => Play(audioType, audioClip));
    }

    public void Play(SoundType soundType, AudioClip audioClip)
    {
        if (audioClip == null)
        {
            return;
        }

        var audioSource = _audioSources[(int)soundType];

        if (soundType == SoundType.BGM)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else
        {
            audioSource.PlayOneShot(audioClip);
        }
    }

    public void Stop(SoundType soundType)
    {
        _audioSources[(int)soundType].Stop();
    }

    public void ChangeVolume(SoundType soundType, float volume)
    {
        _audioSources[(int)soundType].volume = volume;
    }

    public void Clear()
    {
        foreach (var audioSource in _audioSources)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }
}
