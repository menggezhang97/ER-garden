using UnityEngine;

public class WaterDrop : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        GrowablePlant plant = other.GetComponentInParent<GrowablePlant>();

        if (plant != null)
        {
            plant.Water(Time.deltaTime);
        }
    }
}