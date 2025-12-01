using UnityEngine;

public class MapSettingsComponent : MonoBehaviour
{
    [SerializeField]
    MapSettings settings;

    public MapSettings Settings => settings;

    private void Awake()
    {
        Destroy(gameObject);
    }
}

[System.Serializable]
public class MapSettings
{
    public string Name;

    [TextArea]
    public string Description;

    public string Author;

    [HideInInspector]
    public bool HasPreviewImage;

    [Header("Optional. Recommended size of 1920x1080.")]
    public Texture2D PreviewImage;

    public PlayType PlayType;

    public PlayAngle PlayAngle;

    public string PlayAngleString
    {
        get
        {
            return PlayAngle.ToString().Replace("_", "") + "°";
        }
    }
}

public enum PlayType 
{ 
    Standing, 
    RoomScale 
};

public enum PlayAngle
{
    _180,
    _360
};