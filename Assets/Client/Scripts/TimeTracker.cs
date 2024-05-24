using UnityEngine;

public class TimeTracker : MonoBehaviour
{
    public float thresholdTime; // Time threshold in seconds

    public float timeObserved = 0f;
    private float startTime;

    public void StartCounting()
    {
        Debug.Log("Start counting...");
        startTime = Time.time;
    }

    public void StopCounting()
    {
        float currentTime = Time.time;
        timeObserved += (currentTime - startTime);
        Debug.Log("Time spent looking at target: " + timeObserved);
    }

}
