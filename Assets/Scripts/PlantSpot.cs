using UnityEngine;

public class PlantSpot : MonoBehaviour
{
    public bool hasPlant = false;
    public Transform plantSpawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (hasPlant) return;

        SeedItem seed = other.GetComponent<SeedItem>();

        if (seed != null)
        {
            PlantSeed(seed);
        }
    }

    private void PlantSeed(SeedItem seed)
    {
        hasPlant = true;

        Instantiate(seed.plantGrowSpotPrefab, plantSpawnPoint.position, plantSpawnPoint.rotation);

        Destroy(seed.gameObject);

        Debug.Log("Seed planted: " + seed.seedType);
    }
}