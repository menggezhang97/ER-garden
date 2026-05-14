using UnityEngine;

public class PlantGrowth : MonoBehaviour
{
    [Header("Growth Stage Objects")]
    public GameObject seedObject;
    public GameObject seedlingObject;
    public GameObject fullPlantObject;

    [Header("Watering Settings")]
    public int waterNeededPerStage = 2;

    private int currentStage = 0;
    private int waterCount = 0;

    private void Start()
    {
        ShowStage(0);
    }

    public void WaterPlant()
    {
        waterCount++;

        Debug.Log("Plant watered: " + waterCount);

        if (waterCount >= waterNeededPerStage)
        {
            waterCount = 0;
            GrowToNextStage();
        }
    }

    private void GrowToNextStage()
    {
        if (currentStage >= 2)
        {
            Debug.Log("Plant is already fully grown.");
            return;
        }

        currentStage++;
        ShowStage(currentStage);

        Debug.Log("Plant grew to stage: " + currentStage);
    }

    private void ShowStage(int stage)
    {
        seedObject.SetActive(stage == 0);
        seedlingObject.SetActive(stage == 1);
        fullPlantObject.SetActive(stage == 2);
    }
}