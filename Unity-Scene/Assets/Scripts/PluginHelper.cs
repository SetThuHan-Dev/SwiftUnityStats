using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

public class PluginHelper : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerTxt;
    [SerializeField] private Button startTrackingBtn;
    [SerializeField] private Button stopTrackingBtn;

    [Header("Ram Usuage")]
    [SerializeField] private UILineRenderer ramLR;
    [SerializeField] private TextMeshProUGUI ramAvgTxt;
    [SerializeField] private TextMeshProUGUI ramMinTxt;
    [SerializeField] private TextMeshProUGUI ramMaxTxt;

    [Header("CPU Usuage")]
    [SerializeField] private UILineRenderer cpuLR;
    [SerializeField] private TextMeshProUGUI cpuAvgTxt;
    [SerializeField] private TextMeshProUGUI cpuMinTxt;
    [SerializeField] private TextMeshProUGUI cpuMaxTxt;

    [Header("GPU Usuage")]
    [SerializeField] private UILineRenderer gpuLR;
    [SerializeField] private TextMeshProUGUI gpuAvgTxt;
    [SerializeField] private TextMeshProUGUI gpuMinTxt;
    [SerializeField] private TextMeshProUGUI gpuMaxTxt;

    private List<float> trackedRamUsage = new();
    private List<float> trackedCpuUsage = new();
    private List<float> trackedGpuUsage = new();
    private float currentTime;   // Current time elapsed
    private bool isTimerRunning; // Flag to track if the timer is running

    // Call Swift Func
    [DllImport("__Internal")]
    private static extern void _startTracking();

    [DllImport("__Internal")]
    private static extern void _stopTracking();

    
    private void Start()
    {
        ResetGraph();
        stopTrackingBtn.interactable = false;
        startTrackingBtn.onClick.AddListener(StartTracker);
        stopTrackingBtn.onClick.AddListener(StopTracker);
    }
    private void StartTracker()
    {
        ResetTimer();
        isTimerRunning = true;

        _startTracking();
        stopTrackingBtn.interactable = true;
        startTrackingBtn.interactable = false;
        ResetGraph();
    }

    private void StopTracker()
    {
        isTimerRunning = false;
        UpdateTimerDisplay();

        _stopTracking();
        startTrackingBtn.interactable = true;
        stopTrackingBtn.interactable = false;
    }

/// <summary>
/// Receive tracked datas from Swift
/// limit to list since tracked datas might get infinite unless the stopTracking button is pressed
/// then only show the data in the last 17 seconds 
/// </summary>
    public void OnReceivedRamUsage(string jsonString)
    {
        string[] receivedData = JsonConvert.DeserializeObject<string[]>(jsonString);
        trackedRamUsage.AddRange(receivedData.Select(float.Parse));

        int total = trackedRamUsage.Count;
        int removeCount = total / 2;
        if (trackedRamUsage.Count >= 40)
        {
            trackedRamUsage.RemoveRange(0, removeCount);
        }

        // Display on Graph
        int count = trackedRamUsage.Count;
        int startIndex = Mathf.Max(0, count - 18); // only can display last 17 data on graph since 17 grid line
        int rangeCount = Mathf.Min(18, count - startIndex); // Adjust rangeCount based on available elements
        List<float> last18Elements = trackedRamUsage.GetRange(startIndex, rangeCount);

        for (int i = 0; i < last18Elements.Count; i++)
        {
            Vector2 ramPoint = new(i, last18Elements[i]/20);
            ramLR.points.Add(ramPoint);
        }

        ramLR.SetVerticesDirty();

        float average = last18Elements.Average();
        float min = last18Elements.Min();
        float max = last18Elements.Max();

        string averageFormatted = average.ToString("F2");
        string minFormatted = min.ToString("F2");
        string maxFormatted = max.ToString("F2");

        ramAvgTxt.text = averageFormatted.ToString();
        ramMinTxt.text = minFormatted.ToString();
        ramMaxTxt.text = maxFormatted.ToString();
    }

    public void OnReceivedCpuUsage(string jsonString)
    {
        string[] receivedData = JsonConvert.DeserializeObject<string[]>(jsonString);
        trackedCpuUsage.AddRange(receivedData.Select(float.Parse));

        int total = trackedCpuUsage.Count;
        int removeCount = total / 2;
        if (trackedCpuUsage.Count >= 40)
        {
            trackedCpuUsage.RemoveRange(0, removeCount);
        }

        // Display on Graph
        int count = trackedCpuUsage.Count;
        int startIndex = Mathf.Max(0, count - 18); // only can display last 17 data on graph since 17 grid line
        int rangeCount = Mathf.Min(18, count - startIndex); // Adjust rangeCount based on available elements
        List<float> last18Elements = trackedCpuUsage.GetRange(startIndex, rangeCount);

        for (int i = 0; i < last18Elements.Count; i++)
        {
            Vector2 ramPoint = new(i, last18Elements[i]/20);
            cpuLR.points.Add(ramPoint);
        }

        cpuLR.SetVerticesDirty();

        float average = last18Elements.Average();
        float min = last18Elements.Min();
        float max = last18Elements.Max();

        string averageFormatted = average.ToString("F2");
        string minFormatted = min.ToString("F2");
        string maxFormatted = max.ToString("F2");

        cpuAvgTxt.text = averageFormatted.ToString();
        cpuMinTxt.text = minFormatted.ToString();
        cpuMaxTxt.text = maxFormatted.ToString();
    }

    public void OnReceivedGpuUsage(string jsonString)
    {
        string[] receivedData = JsonConvert.DeserializeObject<string[]>(jsonString);
        trackedGpuUsage.AddRange(receivedData.Select(float.Parse));

        int total = trackedGpuUsage.Count;
        int removeCount = total / 2;
        if (trackedGpuUsage.Count >= 40)
        {
            trackedGpuUsage.RemoveRange(0, removeCount);
        }

        // Display on Graph
        int count = trackedGpuUsage.Count;
        int startIndex = Mathf.Max(0, count - 18); // only can display last 17 data on graph since 17 grid line
        int rangeCount = Mathf.Min(18, count - startIndex); // Adjust rangeCount based on available elements
        List<float> last18Elements = trackedGpuUsage.GetRange(startIndex, rangeCount);

        for (int i = 0; i < last18Elements.Count; i++)
        {
            Vector2 ramPoint = new(i, last18Elements[i]/20);
            gpuLR.points.Add(ramPoint);
        }

        gpuLR.SetVerticesDirty();

        float average = last18Elements.Average();
        float min = last18Elements.Min();
        float max = last18Elements.Max();

        string averageFormatted = average.ToString("F2");
        string minFormatted = min.ToString("F2");
        string maxFormatted = max.ToString("F2");

        gpuAvgTxt.text = averageFormatted.ToString();
        gpuMinTxt.text = minFormatted.ToString();
        gpuMaxTxt.text = maxFormatted.ToString();

    }

    private void ResetGraph()
    {
        ramAvgTxt.text = 0.ToString();
        ramMinTxt.text = 0.ToString();
        ramMaxTxt.text = 0.ToString();

        cpuAvgTxt.text = 0.ToString();
        cpuMinTxt.text = 0.ToString();
        cpuMaxTxt.text = 0.ToString();

        gpuAvgTxt.text = 0.ToString();
        gpuMinTxt.text = 0.ToString();
        gpuMaxTxt.text = 0.ToString();

        ramLR.points.Clear();
        cpuLR.points.Clear();
        gpuLR.points.Clear();

        trackedRamUsage.Clear();
        trackedCpuUsage.Clear();
        trackedGpuUsage.Clear();

        ramLR.SetVerticesDirty();
        cpuLR.SetVerticesDirty();
        gpuLR.SetVerticesDirty();

    }

    private void Update()
    {
        if (isTimerRunning)
        {
            currentTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    private void ResetTimer()
    {
        currentTime = 0f;
        isTimerRunning = false;
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        int wholeSeconds = Mathf.FloorToInt(currentTime);
        timerTxt.text = $"Time elapsed: { wholeSeconds - 1 }" ;
    }
}