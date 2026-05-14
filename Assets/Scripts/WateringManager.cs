using UnityEngine;

public class WateringManager : MonoBehaviour
{
    public bool wateringMode = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            wateringMode = !wateringMode;
            Debug.Log("Watering mode: " + wateringMode);
        }

        if (wateringMode && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            foreach (RaycastHit hit in hits)
            {
                PlantGrowth plant = hit.collider.GetComponent<PlantGrowth>();

                if (plant != null)
                {
                    plant.Water();
                    Debug.Log("Watered: " + hit.collider.name);
                    return;
                }
            }

            Debug.Log("Clicked, but no plant/seed was hit.");
        }
    }
}