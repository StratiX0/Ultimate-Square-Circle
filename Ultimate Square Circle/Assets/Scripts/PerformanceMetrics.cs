using UnityEngine;

public class PerformanceMetrics : MonoBehaviour
{
    public static PerformanceMetrics instance;
    public int successCount = 0;
    public int failureCount = 0;
    public float averageTimePerLevel = 0;

    private void Awake()
    {
        instance = this;
    }
    
    public void UpdateMetrics(bool success, float timeTaken)
    {
        if (success) successCount++;
        else failureCount++;

        averageTimePerLevel = (averageTimePerLevel * (successCount + failureCount - 1) + timeTaken) / (successCount + failureCount);
    }
}

