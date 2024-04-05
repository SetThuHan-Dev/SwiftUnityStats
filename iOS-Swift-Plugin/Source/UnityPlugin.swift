import Foundation
import Darwin
import UIKit
import SwiftUI
import Metal
import GPUUtilization

@objc public class UnityPlugin: NSObject {
    @objc public static let shared = UnityPlugin()

    private var timer: Timer?
    
    @objc public func startTracking() {
        // Start the timer to run the tracking functions every second
        timer = Timer.scheduledTimer(timeInterval: 1.0, target: self, selector: #selector(runTrackingFunctions), userInfo: nil, repeats: true)
    }
    
    @objc public func stopTracking() {
        // send all tracked data to Unity
        sendRamUsageDataToUnity()
        sendCpuUsageDataToUnity()
        sendGpuUsageDataToUnity()

        // stop timer
        timer?.invalidate()
        timer = nil
    }
    
    @objc private func runTrackingFunctions() {
        trackRamUsage()
        trackCpuUsage()
        trackGpuUsage()
    }
}

var ramUsageData: [String] = []
var cpuUsageData: [String] = []
var gpuUsageData: [String] = []

// Ram Usage
func trackRamUsage() {
    let totalRamBytes = ProcessInfo.processInfo.physicalMemory
    let totalRamMegabytes = Float(totalRamBytes) / (1024 * 1024)
    
    //_ = String(format: "%.2f", totalRamMegabytes)
    //print("Tota RAM: \(totalFormattedRam)%")
    
    let memoryResult = getMegabytesUsed()
    if let memoryValue = memoryResult {
        _ = String(format: "%.0f", memoryValue);
        //print("Used Memory: \(formattedUsedRamResult) MB")
    } else {
        print("Failed to retrieve memory usage")
    }

    if let usedRam = getMegabytesUsed() {
        let ramUsagePercentage = (usedRam / totalRamMegabytes) * 100
        let formattedRamPercentage = String(format: "%.2f", ramUsagePercentage)
        //print("RAM Usage: \(formattedRamPercentage)%")
        ramUsageData.append(formattedRamPercentage)
    }
}

func sendRamUsageDataToUnity() {
    guard let jsonData = try? JSONSerialization.data(withJSONObject: ramUsageData, options: []) else { return }
    if let jsonString = String(data: jsonData, encoding: .utf8) {
        SendRamUsageToUnity(data: String(jsonString))
    }
    ramUsageData.removeAll();
}

func getMegabytesUsed() -> Float? {
    var info = mach_task_basic_info()
    var count = mach_msg_type_number_t(MemoryLayout.size(ofValue: info) / MemoryLayout<integer_t>.size)
    let kerr = withUnsafeMutablePointer(to: &info) { infoPtr in
        return infoPtr.withMemoryRebound(to: integer_t.self, capacity: Int(count)) { (machPtr: UnsafeMutablePointer<integer_t>) in
            return task_info(
                mach_task_self(),
                task_flavor_t(MACH_TASK_BASIC_INFO),
                machPtr,
                &count
            )
        }
    }
    guard kerr == KERN_SUCCESS else {
        return nil
    }
    return Float(info.resident_size) / (1024 * 1024)
}

func mach_task_self() -> task_t {
    return mach_task_self_
}

// CPU Usage
func trackCpuUsage() {
    let cpuResult = cpuUsage()
    let formattedCpuPercentage = String(format: "%.0f", cpuResult)
    //print("CPU Usage: \(formattedCpuPercentage)")
    cpuUsageData.append(formattedCpuPercentage);
}

func sendCpuUsageDataToUnity() {
    guard let jsonData = try? JSONSerialization.data(withJSONObject: cpuUsageData, options: []) else { return }
    if let jsonString = String(data: jsonData, encoding: .utf8) {
        SendCpuUsageToUnity(data: String(jsonString))
    }
    cpuUsageData.removeAll();
}

func cpuUsage() -> Double {
    var kr: kern_return_t
    var task_info_count: mach_msg_type_number_t

    task_info_count = mach_msg_type_number_t(TASK_INFO_MAX)
    var tinfo = [integer_t](repeating: 0, count: Int(task_info_count))

    kr = task_info(mach_task_self_, task_flavor_t(TASK_BASIC_INFO), &tinfo, &task_info_count)
    if kr != KERN_SUCCESS {
        return -1
    }

    var thread_list: thread_act_array_t? = UnsafeMutablePointer(mutating: [thread_act_t]())
    var thread_count: mach_msg_type_number_t = 0
    defer {
        if let thread_list = thread_list {
            vm_deallocate(mach_task_self_, vm_address_t(UnsafePointer(thread_list).pointee), vm_size_t(thread_count))
        }
    }

    kr = task_threads(mach_task_self_, &thread_list, &thread_count)

    if kr != KERN_SUCCESS {
        return -1
    }

    var tot_cpu: Double = 0

    if let thread_list = thread_list {

        for j in 0 ..< Int(thread_count) {
            var thread_info_count = mach_msg_type_number_t(THREAD_INFO_MAX)
            var thinfo = [integer_t](repeating: 0, count: Int(thread_info_count))
            kr = thread_info(thread_list[j], thread_flavor_t(THREAD_BASIC_INFO),
                             &thinfo, &thread_info_count)
            if kr != KERN_SUCCESS {
                return -1
            }

            let threadBasicInfo = convertThreadInfoToThreadBasicInfo(thinfo)

            if threadBasicInfo.flags != TH_FLAGS_IDLE {
                tot_cpu += (Double(threadBasicInfo.cpu_usage) / Double(TH_USAGE_SCALE)) * 100.0
            }
        } // for each thread
    }

    return tot_cpu
}

fileprivate func convertThreadInfoToThreadBasicInfo(_ threadInfo: [integer_t]) -> thread_basic_info {
    var result = thread_basic_info()

    result.user_time = time_value_t(seconds: threadInfo[0], microseconds: threadInfo[1])
    result.system_time = time_value_t(seconds: threadInfo[2], microseconds: threadInfo[3])
    result.cpu_usage = threadInfo[4]
    result.policy = threadInfo[5]
    result.run_state = threadInfo[6]
    result.flags = threadInfo[7]
    result.suspend_count = threadInfo[8]
    result.sleep_time = threadInfo[9]

    return result
}

//GPU Usage
func trackGpuUsage()
{
    let gpuUtilization = GPUUtilization()
    let gpuUsage = GPUUtilization.gpuUsage
    //print("GPU usage: \(gpuUsage)%")
    gpuUsageData.append(String(gpuUsage));
}

func sendGpuUsageDataToUnity() {
    guard let jsonData = try? JSONSerialization.data(withJSONObject: gpuUsageData, options: []) else { return }
    if let jsonString = String(data: jsonData, encoding: .utf8) {
        SendGpuUsageToUnity(data: String(jsonString))
    }
    gpuUsageData.removeAll();
}

// Swift ~ Unity (Receiver Game Obj: "PluginHelper" in Unity Scene)
private func SendRamUsageToUnity(data: String) {
    let unityFramework = UnityFramework.getInstance()
    unityFramework?.sendMessageToGO(withName: "PluginHelper", functionName: "OnReceivedRamUsage", message: data)
}

private func SendCpuUsageToUnity(data: String) {
    let unityFramework = UnityFramework.getInstance()
    unityFramework?.sendMessageToGO(withName: "PluginHelper", functionName: "OnReceivedCpuUsage", message: data)
}

private func SendGpuUsageToUnity(data: String) {
    let unityFramework = UnityFramework.getInstance()
    unityFramework?.sendMessageToGO(withName: "PluginHelper", functionName: "OnReceivedGpuUsage", message: data)
}
