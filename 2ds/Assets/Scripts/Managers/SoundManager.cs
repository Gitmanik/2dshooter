using Mirror;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    private AudioSource source;

    private void OnValidate()
    {
        source = GetComponent<AudioSource>();
    }
    void Awake()
    {
        Instance = this;
    }

    public static void Play(AudioClip clip)
    {
        Instance._Play(clip);
    }

    public void _Play(AudioClip clip)
    {
        source.clip = clip;
        source.Play();
    }
}