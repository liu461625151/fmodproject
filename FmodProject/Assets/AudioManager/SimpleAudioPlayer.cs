using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;


public class SimpleAudioPlayer : MonoBehaviour
{
    private string EventName = "event:/Attack/attack01";
    private EventInstance _instance;
    private bool isPause = true;
    public float speed = 1.0f;
    public float volume = 1.0f;
    public bool isMute = false;
    
    private void Awake()
    {
        _instance = CreateInstance(EventName);
    }

    /// <summary>
    /// ≤•∑≈…˘“Ù
    /// </summary>
    public void OnPlaySoundButtonClick()
    {
        //_instance.setCallback(callbackFunc);
        _instance.set3DAttributes(RuntimeUtils.To3DAttributes(new Vector3()));
        _instance.start();
    }

    public void PlayOnShot()
    {
        RuntimeManager.PlayOneShot(EventName);
    }

    /// <summary>
    /// ‘›Õ£…˘“Ù
    /// </summary>
    public void OnStopSoundButtonClick()
    {
        StopSound();
    }

    public void OnSetSpeedButtonClick()
    {
        _instance.setPitch(speed);
        RuntimeManager.StudioSystem.setParameterByName("MasterVolume", volume);
    }

    RESULT callbackFunc(EVENT_CALLBACK_TYPE type, System.IntPtr eventInstance, System.IntPtr parameters)
    {
        if (type == EVENT_CALLBACK_TYPE.STOPPED)
        {
            UnityEngine.Debug.LogError("callbackFunc");
        }
      
        return RESULT.OK;
    }
    public void OnSoundEnd()
    {
        UnityEngine.Debug.LogError("OnSoundEnd");
        _instance.release();
        _instance.clearHandle();
    }

    /// <summary>
    ///  «∑Òæ≤“Ù
    /// </summary>
    public void OnMuteButtonClick()
    {
        isMute = !isMute;
        RuntimeManager.MuteAllEvents(isMute);
    }

    /// <summary>
    /// Õ£÷πµ•∏ˆ…˘“Ù
    /// </summary>
    public void StopSound()
    {
        _instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _instance.release();
        _instance.clearHandle();
    }
    
    /// <summary>
    /// ‘›Õ£…˘“Ù
    /// </summary>
    public void OnPauseSoundButtonClick()
    {
        _instance.setPaused(isPause);
        isPause = !isPause;
    }

    public EventInstance CreateInstance(string eventPath)
    {
        var instance = RuntimeManager.CreateInstance(eventPath);
        return instance;
    }
  
}
