using UnityEngine;

public class PlantGrowth : MonoBehaviour
{
    [Header("Growth Stages")]
    public Vector3[] growthStages = new Vector3[]
    {
        new Vector3(0.3f, 0.3f, 0.3f),
        new Vector3(0.6f, 0.6f, 0.6f),
        new Vector3(1.0f, 1.0f, 1.0f)
    };

    [Header("Watering")]
    public int waterNeededPerStage = 2;
    public float smoothSpeed = 3f;

    private int currentStage = 0;
    private int waterCount = 0;
    private Vector3 targetScale;

    private void Start()
    {
        currentStage = 0;
        transform.localScale = growthStages[currentStage];
        targetScale = growthStages[currentStage];
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * smoothSpeed
        );
    }

    public void WaterPlant()
    {
        if (currentStage >= growthStages.Length - 1)
        {
            Debug.Log("Plant is already fully grown.");
            return;
        }

        waterCount++;
        Debug.Log("Plant watered. Count = " + waterCount);

        if (waterCount >= waterNeededPerStage)
        {
            waterCount = 0;
            Grow();
        }
    }

    private void Grow()
    {
        currentStage++;
        targetScale = growthStages[currentStage];
        Debug.Log("Plant grew to stage " + currentStage);
    }
}