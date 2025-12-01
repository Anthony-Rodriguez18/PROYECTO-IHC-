using UnityEngine;

public class StartPosition : MonoBehaviour
{
    public enum PositionType { Warrior, Giant }

    [SerializeField]
    PositionType type;

    public PositionType Type => type;
}