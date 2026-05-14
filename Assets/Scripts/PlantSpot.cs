using UnityEngine;

public class PlantSpot : MonoBehaviour
{
    public bool isOccupied = false;
    public EnvironmentType environmentType = EnvironmentType.Sunny;

    private void OnTriggerEnter(Collider other)
    {
        EnvironmentZone zone = other.GetComponent<EnvironmentZone>();

        if (zone != null)
        {
            environmentType = zone.environmentType;
            Debug.Log(name + " is now in " + environmentType);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        EnvironmentZone zone = other.GetComponent<EnvironmentZone>();

        if (zone != null)
        {
            environmentType = EnvironmentType.Sunny;
            Debug.Log(name + " is now in Sunny");
        }
    }
}