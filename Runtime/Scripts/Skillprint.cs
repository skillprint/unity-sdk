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

    public void Init(string GameId)
    {
        _skillprintCore = new GameObject("SkillprintCore").AddComponent<SkillprintCore>();
        _skillprintCore.GameId = GameId;
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
}

