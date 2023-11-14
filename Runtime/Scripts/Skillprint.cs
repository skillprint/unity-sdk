/*
 * This is a wrapper of the Skillprint Core Class that exposes the public APIs in a singleton
 * pattern. The idea here is to let the user initialize the script once and use the same
 * instance over and over again.
 */
using System.Collections.Generic;
using UnityEngine;


public class Skillprint
{
    private SkillprintCore _skillprintCore;
    private static Skillprint _instance = null;
    private static readonly object _instanceLock = new object();

    public Skillprint()
    {

    }


    public static Skillprint GetInstance()
    {
        if (_instance == null)
        {
            lock(_instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new Skillprint();
                    }
                }
        }
        return _instance;
    }

    public static Skillprint Instance
    {
        get
        {
            return GetInstance();
        }
    }

    public void Init(string gameId, Guid? playerId = null)
    {
        _skillprintCore = new GameObject("SkillprintCore").AddComponent<SkillprintCore>();
        _skillprintCore.GameId = gameId;
        if (playerId is null) {
            playerId = new Guid()
        }
        _skillprintCore._persistPlayerId(playerId);
    }
    

    public void SendEvent(string eventName, IDictionary<string, dynamic> eventParams = null)
    {
        _skillprintCore.SendEventNative(eventName, eventParams);
    }

    
    public void GameSessionStart()
    {
        _skillprintCore.GameSessionStartNative();
    }

    
    public void GameSessionEnd()
    {
        _skillprintCore.GameSessionEndNative();
    }

    
    public void ShowWebViewContent()
    {
        _skillprintCore.ShowWebViewContentNative();
    }
    
    
    public void HideWebViewContent()
    {
        _skillprintCore.HideWebViewContent();
    }

    // Standard Events
    public void LevelStart(IDictionary<string, dynamic> eventParams = null)
    {
        SendEvent(
            "LEVEL_START",
            eventParams
        );
    }

    public void LevelFailed(IDictionary<string, dynamic> eventParams = null)
    {
        SendEvent(
            "LEVEL_FAILED",
            eventParams
        );
    }

    public void LevelComplete(IDictionary<string, dynamic> eventParams = null)
    {
        SendEvent(
            "LEVEL_COMPLETE",
            eventParams
        );
    }

    public void LevelQuit(IDictionary<string, dynamic> eventParams = null)
    {
        SendEvent(
            "LEVEL_QUIT",
            eventParams
        );
    }

    public void LevelRestart(IDictionary<string, dynamic> eventParams = null)
    {
        SendEvent(
            "LEVEL_RESTART",
            eventParams
        );
    }

    public void Hint(IDictionary<string, dynamic> eventParams = null)
    {
        SendEvent(
            "HINT",
            eventParams
        );
    }

    public void GenericPositive(IDictionary<string, dynamic> eventParams = null)
    {
        SendEvent(
            "GENERIC_POSITIVE",
            eventParams
        );
    }

    public void GenericNegative(IDictionary<string, dynamic> eventParams = null)
    {
        SendEvent(
            "GENERIC_NEGATIVE",
            eventParams
        );
    }

}

