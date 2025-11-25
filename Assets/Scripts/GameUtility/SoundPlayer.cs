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
    // ---------------------------------------------------------
    //  SERIALIZABLE SOUND STRUCTURE
    // ---------------------------------------------------------

    /// <summary>
    /// Struct representing a single named audio resource.
    /// </summary>
    [Serializable]
    struct soundSource
    {
        public string name;       // Name used to reference the sound
        public AudioClip resource; // AudioClip asset
    }

    // ---------------------------------------------------------
    //  FIELDS
    // ---------------------------------------------------------

    /// <summary>
    /// List of audio resources assignable in the Inspector.
    /// </summary>
    [SerializeField] private List<soundSource> _audioResources;

    /// <summary>
    /// AudioSource component used to play sounds.
    /// </summary>
    private static AudioSource _audioSource;

    /// <summary>
    /// Singleton instance of this SoundPlayer.
    /// </summary>
    private static SoundPlayer _soundPlayerInstance;


    // ---------------------------------------------------------
    //  INITIALIZATION
    // ---------------------------------------------------------

    /// <summary>
    /// Assigns singleton instance on Awake.
    /// </summary>
    private void Awake()
    {
        _soundPlayerInstance = this;
    }

    /// <summary>
    /// Fetches the AudioSource component on Start.
    /// </summary>
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }


    // ---------------------------------------------------------
    //  SOUND RESOURCE MANAGEMENT
    // ---------------------------------------------------------

    /// <summary>
    /// Retrieves the AudioClip resource associated with the given sound name.
    /// </summary>
    /// <param name="sound">The name of the sound to fetch.</param>
    /// <returns>The AudioClip corresponding to the name.</returns>
    private AudioClip GetSoundResource(string sound)
    {
        // Match by name (converted to lowercase)
        AudioClip _soundResource = _audioResources
            .Where(source => source.name == sound.ToLower())
            .First()
            .resource;

        return _soundResource;
    }


    // ---------------------------------------------------------
    //  SOUND PLAYBACK
    // ---------------------------------------------------------

    /// <summary>
    /// Plays a sound asynchronously until it finishes.
    /// </summary>
    /// <param name="sound">The AudioClip to play.</param>
    /// <returns>IEnumerator for coroutine.</returns>
    IEnumerator PlaySoundUntilDone(AudioClip sound)
    {
        _audioSource.PlayOneShot(sound);

        // Wait for the sound's length
        yield return new WaitForSecondsRealtime(sound.length * 1000);
    }

    /// <summary>
    /// Public method to play a sound by name.
    /// Starts a coroutine to play it asynchronously.
    /// </summary>
    /// <param name="sound">The name of the sound to play.</param>
    public void PlaySound(string sound)
    {
        StartCoroutine(PlaySoundUntilDone(GetSoundResource(sound)));
    }
}
