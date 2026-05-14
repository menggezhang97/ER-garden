using UnityEngine;

public class PlantGrowth : MonoBehaviour
{
    public GameObject nextStagePrefab;
    public float growDelay = 2f;

    private bool hasBeenWatered = false;

    public void Water()
    {
        if (hasBeenWatered) return;

        hasBeenWatered = true;
        Invoke(nameof(Grow), growDelay);
    }

    private void Grow()
    {
        if (nextStagePrefab != null)
        {
            Instantiate(nextStagePrefab, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }
}