using UnityEngine;
using UnityEngine.Audio;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public Sound[] sounds;

    void Awake()
    {
        //SINGLETON DECLARATION
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(this);


        //INITIALIZATION FOR EACH SOUND
        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
        }
    }

    void Start()
    {
        
    }

    public Sound GetSound(SoundCode _soundCode)
    {
        Sound s = Array.Find(sounds, sound => sound.code == _soundCode);
        return s;
    }

    public void Play(SoundCode _soundCode)
    {
        Sound s = Array.Find(sounds, sound => sound.code == _soundCode);
        if (s == null) return;
        s.source.Play();
    }

    public void Stop(SoundCode _soundCode)
    {
        Sound s = Array.Find(sounds, sound => sound.code == _soundCode);
        if (s == null) return;
        s.source.Stop();
    }

    

}