/*
    Author: Francesco Podda
    Date: 22/05/2019
*/

using UnityEngine;
using CustomAssets.Utilities;
using Tomba;

[System.Serializable]
public class EventDataManager
{

    [ArrayElementTitle("name")]
    public TombaEvent[] events;


    public void Initialize(TombaEvent[] eventList)
    {
        //string json = ToJson(eventList, true);
        //File.WriteAllText(AssetDatabase.GetAssetPath(eventListFile), json);
        //EditorUtility.SetDirty(eventListFile);

        //eventList = FromJson<Event>(eventListFile.text);
        this.events = eventList;

        string str = CustomAssets.Utilities.Serializer.SerializeToXML<EventDataManager, EventDataManager>(this, false, System.Text.Encoding.ASCII, typeof(Event));

        Debug.Log(str);
    }

    public void OccurrEvent(int id)
    {
        events[id].occurred = true;

        //Increment ap gain


        //Save event file

    }
    public void ClearEvent(int id)
    {
        events[id].cleared = true;

        //Increment ap gain
    

        //Save event file
    }

    public static T[] FromJson<T>(string json)
    {
        JSONWrapper<T> wrapper = UnityEngine.JsonUtility.FromJson<JSONWrapper<T>>(json);
        return wrapper.Events;
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        JSONWrapper<T> wrapper = new JSONWrapper<T>();
        wrapper.Events = array;
        return UnityEngine.JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [System.Serializable]
    private class JSONWrapper<T>
    {
        public T[] Events;
    }
}
