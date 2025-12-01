[System.Serializable]
public class MapIdentifier
{
    /// <summary>
    /// For built-in maps, this will be the scene name.
    /// For customs, it will be the GUID.
    /// </summary>
    public string identifier;

    public bool isCustomMap;
}