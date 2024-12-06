
using UnityEngine;
using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;


public class SoundManager<T> : MonoBehaviour where T : Component
{
	protected static T _instance;

	public static T instance
	{
		get
		{
			if (_instance == null)
				_instance = FindObjectOfType<T>();

			return _instance;
		}
	}

	public static bool DoesInstanceExist
	{
		get
		{
			return instance != null;
		}
	}


	public BGMPathDefinition[] BGMPathDefinitions;
	protected Dictionary<BGMType, string> BGMPathDict;
	protected BGMType currentAmbientAudio;
	//һ������ʵ�����ɿ��ơ�
	//ѭ�����ŵĿ��Դ���һ��,Ҫ�ֶ�release, OneShot Ϊtrue�������Զ�release.
	protected EventInstance BGMEvent;

	protected Dictionary<string, EventInstance> loopingSoundEvents = new Dictionary<string, EventInstance>();

	protected List<EventInstance> pausedSoundEvents;
	protected const int minPathLength = 7;
	protected virtual void Awake()
	{
		_instance = GetComponent<T>();

		BGMPathDict = new Dictionary<BGMType, string>();
		foreach (var ambientAudioDefinition in BGMPathDefinitions)
		{
			BGMPathDict.Add(ambientAudioDefinition.ambientAudioType, ambientAudioDefinition.path);
		}
	}

	protected virtual void OnDestroy()
	{
		StopAndReleaseAll();

		if (BGMEvent.isValid())
		{
			BGMEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			BGMEvent.release();
		}
		_instance = null;
	}


	public virtual void StopAndReleaseAll()
	{
		foreach (var sound in loopingSoundEvents.Values)
		{
			if (sound.isValid())
            {
				sound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
				sound.release();
			}
		}

		loopingSoundEvents.Clear();
	}


	public virtual void pauseAll()
	{
		pausedSoundEvents = new List<EventInstance>();

		foreach (var loopingInstance in loopingSoundEvents.Values)
		{
			if (loopingInstance.isValid())
            {
				PLAYBACK_STATE state;
				loopingInstance.getPlaybackState(out state);
				if (state == PLAYBACK_STATE.PLAYING
					|| state == PLAYBACK_STATE.STARTING)
				{
					loopingInstance.setPaused(true);
					pausedSoundEvents.Add(loopingInstance);
				}
			}
		}

		PauseBGM(true);
	}

	public virtual void resumeAll()
	{
		PauseBGM(!MusicEnable());

		if (!SFXEnable() || pausedSoundEvents == null)
			return;

		foreach (var pausedSound in pausedSoundEvents)
		{
			pausedSound.setPaused(false);
		}
	}

	#region BGM

	public virtual void PlayBGM(BGMType type)
	{
		currentAmbientAudio = type;

		if (MusicEnable())
		{
			CheckBGMEventInstance();
		}
	}

	public virtual void PauseBGM(bool pause)
	{
		if (!BGMEvent.isValid())
			CheckBGMEventInstance();

		if (BGMEvent.isValid())
			BGMEvent.setPaused(pause);
	}

	protected void CheckBGMEventInstance()
	{

		if (BGMPathDict.ContainsKey(currentAmbientAudio))
		{
			//ֹ֮ͣǰ��,��ÿ���ͨ�������仯��������
			if (BGMEvent.isValid())
			{
				BGMEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
				BGMEvent.release();
			}
			BGMEvent = RuntimeManager.CreateInstance(BGMPathDict[currentAmbientAudio]);
			BGMEvent.start();
		}
	}

	#endregion

	#region SFX

	/// <summary>
	/// ����һ�Σ����ܸı��κβ���
	/// </summary>
	public virtual void PlaySoundOnce(string sound)
	{
		if (!SFXEnable() || string.IsNullOrEmpty(sound))
			return;

		if (sound.Length < minPathLength)
		{
			Debug.LogWarning("Wrong Sound Path:" + sound);
			return;
		}

		RuntimeManager.PlayOneShot(sound);
	}
 
	// <summary>
	// Ŀ��λ�ò���һ��(λ�ò��䣩
	// </summary>
	//public void PlaySoundOnce(string Event, Vector3 position) {
	//	if (SFXEnable())
	//		RuntimeManager.PlayOneShot(Event, position);
	//}
 
	// <summary>
	// ����һ�Σ�λ����attach��(����attach�ƶ�)
	// </summary>
	//public void PlaySoundOnce(string Event, GameObject attach) {
	//	if (SFXEnable())
	//		RuntimeManager.PlayOneShotAttached(Event, attach);
	//}

	/// <summary>
	/// ѭ������,ҪFmod������OneShotΪfalse
	/// </summary>
	//public virtual EventInstance PlayLoopSounds(string sound)
	//{
	//	if (!SFXEnable() || string.IsNullOrEmpty(sound))
	//		return;

	//	if (sound.Length < minPathLength)
	//	{
	//		Debug.LogWarning("Wrong Sound Path:" + sound);
	//		return null;
	//	}

	//	if (HavaEventInstance(sound))
	//	{
	//		loopingSoundEvents[sound].start();
	//		return loopingSoundEvents[sound];
	//	}

	//	var newInstance = RuntimeManager.CreateInstance(sound);

	//	newInstance.start();
	//	loopingSoundEvents[sound] = newInstance;

	//	return newInstance;
	//}

	/// <summary>
	/// ��ָͣ��ѭ����Ч
	/// </summary>
	public bool PauseSound(string sound, bool pause = true)
	{
		if (HavaEventInstance(sound))
		{
			var result = loopingSoundEvents[sound].setPaused(pause && SFXEnable());
			return result == FMOD.RESULT.OK;
		}

		return false;
	}
	/// <summary>
	/// ָֹͣ��ѭ����Ч�����ͷ�
	/// </summary>
	public bool StopSound(string sound)
	{
		if (HavaEventInstance(sound))
		{
			var result = loopingSoundEvents[sound].stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			loopingSoundEvents[sound].release();
			loopingSoundEvents.Remove(sound);
			return result == FMOD.RESULT.OK;
		}

		return false;
	}

	protected bool HavaEventInstance(string sound)
	{
		return !string.IsNullOrEmpty(sound) && loopingSoundEvents.ContainsKey(sound);
	}
	#endregion

	//void OnApplicationPause(bool didPause) {
	//	if (didPause) {
	//		pauseAll();
	//	}
	//	else {
	//		resumeAll();
	//	}
	//}

	void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			resumeAll();
		}
		else
		{
			pauseAll();
		}
	}

	bool MusicEnable()
    {
		return true;
    }

	bool SFXEnable()
    {
		return true;
    }
}


[Serializable]
public class BGMPathDefinition
{
	public BGMType ambientAudioType;

	[EventRef]
	public string path;
}

public enum BGMType
{
	Mainmenu = 0,
	IngameRunning = 20,
}
