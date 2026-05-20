using UnityEngine;

public class GardenLamp : MonoBehaviour
{
    [SerializeField] private Light lampLight;
    [SerializeField] private DayNightCycle dayNightCycle;

    [Header("Night Settings")]
    [SerializeField] private float nightThreshold = 0.25f;

    private void Update()
    {
        if (lampLight == null || dayNightCycle == null)
            return;

        bool isNight =
            dayNightCycle.CurrentTimeOfDay < nightThreshold ||
            dayNightCycle.CurrentTimeOfDay > 0.75f;

        lampLight.enabled = isNight;
    }
}