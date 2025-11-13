using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class SoundPlayer : MonoBehaviour
{

    [Serializable]
    struct soundSource
    {
        public string name;
        public AudioClip resource;
    }

    [SerializeField] private List<soundSource> _audioResources;
    private static AudioSource _audioSource;
    private static SoundPlayer _soundPlayerInstance;

    private void Awake()
    {
        _soundPlayerInstance = this;
    }
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private AudioClip GetSoundResource(string sound)
    {
        AudioClip _soundResource = _audioResources.Where(source => source.name == sound.ToLower()).First().resource;
        return _soundResource;
    }

    IEnumerator PlaySoundUntilDone(AudioClip sound)
    {
        _audioSource.PlayOneShot(sound);
        yield return new WaitForSecondsRealtime(sound.length*1000);
    }

    public void PlaySound(string sound)
    {
        StartCoroutine(PlaySoundUntilDone(GetSoundResource(sound)));
    }
}
