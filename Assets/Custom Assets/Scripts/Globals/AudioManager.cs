using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : SerializableMonoBehaviour
{

	public static AudioManager instance;

	public AudioMixerGroup mixerGroup;

    public Sound[] themes;

    public Sound[] sounds;

    public override string FileExtension { get => ".smam"; protected set => base.FileExtension = value; }
    public override string FileExtensionName { get => "SMAM"; protected set => base.FileExtensionName = value; }

    public void UpdateSceneTheme(int sceneIndex)
    {
        StopAllSounds();
        if (sceneIndex >= 0)
        {
            if (sceneIndex <= themes.Length - 1)
            {
                if (themes[sceneIndex] != null)
                {
                    this.Play(themes[sceneIndex]);
                }
            }
        }
    }

    void Awake()
	{
		if (instance != null)
		{
			Destroy(gameObject);
		}
		else
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}

        foreach (Sound s in themes)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.loop = s.loop;

            s.source.outputAudioMixerGroup = mixerGroup;
        }

        foreach (Sound s in sounds)
		{
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;
			s.source.loop = s.loop;

			s.source.outputAudioMixerGroup = mixerGroup;
		}
    }

    public void Play(Sound sound)
    {
        if (sound == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        sound.source.volume = sound.volume * (1f + UnityEngine.Random.Range(-sound.volumeVariance / 2f, sound.volumeVariance / 2f));
        sound.source.pitch = sound.pitch * (1f + UnityEngine.Random.Range(-sound.pitchVariance / 2f, sound.pitchVariance / 2f));

        sound.source.Play();
    }

    public void Play(string sound)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);

        Play(s);
	}

    public AudioSource Pause(Sound sound)
    {
        if (sound == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return null;
        }
        sound.source.Stop();
        return sound.source;
    }
    public AudioSource Pause(string sound)
    {
        Sound s = Array.Find(sounds, item => item.name == sound);
        return Pause(s);
    }
    public void Stop(string sound)
    {
        ResetTime(Pause(sound));
    }

    public void StopAllSounds()
    {
        foreach (Sound s in themes)
        {
            if(s.source.isPlaying)
            {
                ResetTime(Pause(s));
            }
        }
        foreach (Sound s in sounds)
        {
            if (s.source.isPlaying)
            {
                ResetTime(Pause(s));
            }
        }
    }

    private void ResetTime(AudioSource s)
    {
        s.time = 0;
    }
    
    public void SoundLoop(string sound, bool flag)
    {
        Sound s = Array.Find(sounds, item => item.name == sound);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.loop = flag;
        s.source.loop = s.loop;
    }

    public override void Save(string path)
    {
        throw new NotImplementedException();
    }

    public override void Load()
    {
        throw new NotImplementedException();
    }
}
