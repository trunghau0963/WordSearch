
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Serialization<TKey, TValue>
{
    public List<TKey> keys;
    public List<TValue> values;

    public Serialization(Dictionary<TKey, TValue> dictionary)
    {
        keys = new List<TKey>(dictionary.Keys);
        values = new List<TValue>(dictionary.Values);
    }

    public Dictionary<TKey, TValue> ToDictionary()
    {
        Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
        for (int i = 0; i < keys.Count; i++)
        {
            result.Add(keys[i], values[i]);
        }
        return result;
    }
}
public class DictionaryDB
{
    public List<string> keys;
    public List<string> values;

    public DictionaryDB()
    {
        keys = new List<string>();
        values = new List<string>();
    }

    public void Add(string key, string value)
    {
        keys.Add(key);
        values.Add(value);
    }

    public void Remove(string key)
    {
        int index = keys.IndexOf(key);
        keys.RemoveAt(index);
        values.RemoveAt(index);
    }

    public bool ContainsKey(string key)
    {
        return keys.Contains(key);
    }

    public string GetValue(string key)
    {
        int index = keys.IndexOf(key);
        Debug.Log("Index: " + index + " " + key);   
        return values[index];
    }
}