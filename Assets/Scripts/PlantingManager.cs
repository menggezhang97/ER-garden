using UnityEngine;

public class PlantingManager : MonoBehaviour
{
    public GameObject seedPrefab;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("PlantZone"))
                {
                    PlantSpot spot = hit.collider.GetComponent<PlantSpot>();

                    if (spot != null && !spot.isOccupied)
                    {
                        Instantiate(seedPrefab, spot.transform.position, Quaternion.identity);
                        spot.isOccupied = true;
                    }
                }
            }
        }
    }
}