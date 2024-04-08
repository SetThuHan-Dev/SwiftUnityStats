# Unity iOS Stats Monitoring Plugin

<div style="text-align: center;">
  <img src="UnityScene.png?raw=true" alt="SwiftUnityStats" width="220" />
</div>

## Plugin Setup
1. Clone this repo
2. Create a Unity project version `2021+ or Newer`
3. Create `Plugins` folder in `Assets` directory. Copy the `iOS-Swift-Plugin` folder in the cloned repo into `Assets/Plugins`

To track iOS device's RAM, CPU and GPU usages, ```_startTracking``` and ```_stopTracking``` swift functions are already defined in `iOS-Swift-Plugin`.

The following codes are Unity C# codes that call the functions defined in the `iOS-Swift-Plugin`

```csharp
using UnityEngine;
using System.Runtime.InteropServices;

[DllImport("__Internal")]
private static extern void _startTracking();

[DllImport("__Internal")]
private static extern void _stopTracking();
```

To send the tracked datas from Swift to Unity, already defined GameObject Name and FunctionNames in `iOS-Swift-Plugin/Source/UnityPlugin.swift` at line numbers `196, 201, 206`.

- `PluginHelper` - Current gameObject name in Hierarchy must be 
- `OnReceivedRamUsage`, `OnReceivedCpuUsage`, `OnReceivedGpuUsage` - Current defined function names in Swift to call the following Unity C# functions with its parameter name `jsonString`:

```csharp
public void OnReceivedRamUsage(string jsonString)

public void OnReceivedCpuUsage(string jsonString)

public void OnReceivedGpuUsage(string jsonString)
```

## Example Usage

*No extra dependency or library required*

```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class PluginHelper : MonoBehaviour
{
    [SerializeField] private Button startTrackingBtn;
    [SerializeField] private Button stopTrackingBtn;

    // Call Swift Func
    [DllImport("__Internal")]
    private static extern void _startTracking();

    [DllImport("__Internal")]
    private static extern void _stopTracking();

    private void Start()
    {
        startTrackingBtn.onClick.AddListener(StartTracker);
        stopTrackingBtn.onClick.AddListener(StopTracker);
    }
    private void StartTracker()
    {
        _startTracking();
    }

    private void StopTracker()
    {
        _stopTracking();
    }

    // Receiving tracked datas from Swift in %
    public void OnReceivedRamUsage(string jsonString)
    {
        Debug.Log($"RAM Usage: {jsonString}");
    }

    public void OnReceivedCpuUsage(string jsonString)
    {
        Debug.Log($"CPU Usage: {jsonString}");
    }

    public void OnReceivedGpuUsage(string jsonString)
    {
        Debug.Log($"GPU Usage: {jsonString}");
    }

}
```
## Unity iOS Build Setup

1. Make sure to change `Run in Xcode as` - `Release` -> `Debug` mode in `Build Settings`
2. After built, open terminal at build folder and run `pod install` to install `GPUUtilization` dependency and generate `xcworkspace`. At first try, `pod install` will fail to install the dependency due to `Enable Bitcode` is set to `No` in the newly generated `Unity-iPhone.xcworkspace`
3. Open `Unity-iPhone.xcworkspace` and then `TARGETS -> Unity-iPhone -> Build Settings -> Enable Bitcode -> Yes`.
4. Open terminal at built folder and try to run `pod install` again. In this time, installation success with "Pod installation complete!".
5. Wait for refresh in `Unity-iPhone.xcworkspace`, and Run the xcode for targeted iOS device.

## Unity Monitor Project

*Requires Unity 2022.1+*

- Open the ```Unity-Scene``` project in the cloned repo by adding the project in Unity Hub
- In ```PlayerSetting```, already set ```Automatically Sign => True``` and ```Target SDK => Simulator SDK```
- Build setups are same as the above "Unity iOS Build Setup".


