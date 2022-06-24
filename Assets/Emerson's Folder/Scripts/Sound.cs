using UnityEngine;
using UnityEngine.Audio;

public enum SoundCode
{
    NONE = -1,
    WALK_SOUND,
    JUMP_SOUND,
    GET_HIT_SOUND,
    THROW_SOUND,
    BALL_EXPLOSION,
    DEAD,
    WIN_SOUND,
    LOSE_SOUND,
    MAIN_MENU,
    INGAME_SOUND
}

[System.Serializable]
public class Sound
{
    public SoundCode code = SoundCode.NONE;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1.0f;
    [Range (.1f, 3f)]
    public float pitch = 1.0f;

    public bool loop;

    [HideInInspector]
    public AudioSource source;
     
}