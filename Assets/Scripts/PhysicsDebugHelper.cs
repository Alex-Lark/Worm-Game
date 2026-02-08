using UnityEngine;

public class PhysicsDebugHelper : MonoBehaviour
{
    /* CAN BE PLACED IN SCENE TO TEST FRAMERATE */
    
    [Header("Framerate Testing")]
    [Tooltip("Enable to force a specific framerate")]
    public bool forceFramerate = false;
    
    [Tooltip("Target framerate to test (60 = typical editor, 144+ = typical build)")]
    [Range(30, 300)]
    public int targetFramerate = 144;
    
    [Header("Physics Settings")]
    [Tooltip("Show current physics timestep")]
    public bool showPhysicsInfo = true;
    
    [Header("VSync Testing")]
    [Tooltip("Disable VSync to test uncapped framerates")]
    public bool disableVSync = true;

    private float originalFixedDeltaTime;

    void Start()
    {
        originalFixedDeltaTime = Time.fixedDeltaTime;
        ApplySettings();
    }

    void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplySettings();
        }
    }

    void ApplySettings()
    {
        if (forceFramerate)
        {
            Application.targetFrameRate = targetFramerate;
        }
        else
        {
            Application.targetFrameRate = -1; // No limit
        }

        if (disableVSync)
        {
            QualitySettings.vSyncCount = 0;
        }
        else
        {
            QualitySettings.vSyncCount = 1;
        }
    }

    void OnGUI()
    {
        if (!showPhysicsInfo) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.normal.textColor = Color.white;
        style.padding = new RectOffset(10, 10, 10, 10);

        string info = $"FPS: {(1f / Time.deltaTime):F0}\n";
        info += $"Fixed Timestep: {Time.fixedDeltaTime:F4}s ({1f / Time.fixedDeltaTime:F0} Hz)\n";
        info += $"Target FPS: {(forceFramerate ? targetFramerate.ToString() : "Unlimited")}\n";
        info += $"VSync: {(disableVSync ? "OFF" : "ON")}\n";
        
        // Calculate how many physics steps per render frame
        float physicsStepsPerFrame = Time.deltaTime / Time.fixedDeltaTime;
        info += $"Physics Steps/Frame: {physicsStepsPerFrame:F2}\n";
        
        if (physicsStepsPerFrame < 0.5f)
        {
            info += "<color=red>WARNING: Very few physics steps per frame!</color>";
        }

        GUI.Label(new Rect(10, 10, 400, 200), info, style);
    }

    void OnDestroy()
    {
        // Restore original settings
        Time.fixedDeltaTime = originalFixedDeltaTime;
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 1;
    }
}

/// <summary>
/// Extension: Add this to check all rigidbodies in your scene for issues
/// </summary>
public static class PhysicsDebugExtensions
{
    [RuntimeInitializeOnLoadMethod]
    public static void LogPhysicsSettings()
    {
        Debug.Log($"Physics Settings:");
        Debug.Log($"  Fixed Timestep: {Time.fixedDeltaTime}");
        Debug.Log($"  Default Solver Iterations: {Physics.defaultSolverIterations}");
        Debug.Log($"  Default Solver Velocity Iterations: {Physics.defaultSolverVelocityIterations}");
    }
}