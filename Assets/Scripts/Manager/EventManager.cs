using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private Dictionary<string, Action<Dictionary<string, object>>> eventDictory;

    public void Init()
    {
        if(eventDictory == null)
        {
            eventDictory = new Dictionary<string, Action<Dictionary<string, object>>>();
        }
    }

    public void StartListening(string eventName, Action<Dictionary<string, object>> listener)
    {
        Action<Dictionary<string, object>> thisEvent;

        if(eventDictory.TryGetValue(eventName, out thisEvent))
        {
            thisEvent += listener;
            eventDictory[eventName] = thisEvent;
        }
        else
        {
            thisEvent += listener;
            eventDictory.Add(eventName, listener);
        }
    }

    public void StopListening(string eventName, Action<Dictionary<string, object>> listener)
    {
        Action<Dictionary<string, object>> thisEvent;
        if(eventDictory.TryGetValue(eventName, out thisEvent))
        {
            thisEvent -= listener;
            eventDictory[eventName] = thisEvent;
        }
    }

    public void InvokeEvent(string eventName, Dictionary<string, object> message)
    {
        Action<Dictionary<string, object>> thisEvent;
        if(eventDictory.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke(message);
            Debug.Log(string.Format("Invoke event {0}, message {1}", eventName, message.ToString()));
        }    
    }

}
