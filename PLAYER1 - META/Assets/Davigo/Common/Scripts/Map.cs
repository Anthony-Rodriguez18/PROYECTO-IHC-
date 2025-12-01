using UnityEngine;

[System.Serializable]
public abstract class Map
{
    [SerializeField]
    protected MapSettings mapSettings;

    public MapSettings MapSettings => mapSettings;

    public abstract MapIdentifier MapIdentifier { get; }
}