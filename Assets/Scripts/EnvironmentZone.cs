using UnityEngine;

public enum EnvironmentType
{
    Sunny,
    Shady
}

public class EnvironmentZone : MonoBehaviour
{
    public EnvironmentType environmentType = EnvironmentType.Shady;
}