using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the retrieval of group counts from a Firebase Realtime Database
/// and assigns a player to the group with the fewest participants:
/// It's designed specifically for Unity WebGL builds.
/// </summary>
/// <remarks>
/// This class uses a sequential callback pattern to fetch each group's data individually.
/// </remarks>
// Based on a concept by Domenico Rotolo on YouTube: https://www.youtube.com/watch?app=desktop&v=fg_aiGVeKc4&ab_channel=uNicoDev
public class FirebaseWebGLManager : MonoBehaviour
{
    private int nextGroup = 0; // default is 0 for "NoOverride"

    /// <summary>
    /// Holds the current count of participants for each group.
    /// Array indices correspond to the following groups:
    /// [0] = NoOverride;
    /// [1] = TestGroup1Text;
    /// [2] = TestGroup2Arrows;
    /// [3] = ControlGroupBlank.
    /// </summary>
    public static int[] groupCounts = { 0, 0, 0, 0 };

    // Singleton Instance:
    /// <summary>
    /// Public static reference to allow easy access from other scripts (e.g., FirebaseWebGLManager.Instance.GetCurrentGroup()).
    /// </summary>
    public static FirebaseWebGLManager Instance { get; private set; }

    /// <summary>
    /// Indicates whether the group assignment process is complete.
    /// </summary>
    public bool IsReady { get; private set; } = false;

    // Declare the external JavaScript function from FirebaseWebGLBridge.jslib.
    [DllImportAttribute("__Internal")]
    public static extern void GetJSON(string path, string objectName, string callback, string fallback);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // The FirebaseWebGLManager GameObject should be persistent across scenes,
            // so group assignments are maintained.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Initiate the fetching of group counts from Firebase:
    private void Start()
    {
#if UNITY_EDITOR
        Debug.Log("FirebaseWebGLManager.cs: Firebase functionality not available in the editor.");
#elif UNITY_WEBGL            
        Log("FirebaseWebGLManager.cs: Fetching group counts sequentially.");
        GetJSON(path: "group_counts/TestGroup1Text", gameObject.name, callback: "OnRequestSuccess1", fallback: "OnRequestFailed");
#endif
    }

    /// <summary>
    /// Returns the currently assigned group ID:
    /// This method should be called after IsReady is true.
    /// </summary>
    /// <returns>The assigned group ID (1, 2, or 3), or 0 if assignment failed.</returns>
    public int GetCurrentGroup()
    {
        Log("GetCurrentGroup: returning " + nextGroup);
        return nextGroup;
    }


    // Callbacks from JavaScript: ---------------------------------------------------------------------

    /// <summary>
    /// Called by the JavaScript bridge if a Firebase request fails:
    /// Unlocks the game to proceed with a fallback assignment.
    /// </summary>
    /// <param name="error">The error message from Firebase.</param>
    private void OnRequestFailed(string error)
    {
        Log("Firebase request failed: " + error);

        // Unlock IsReady even after a failure to allow the game to start with a random group.
        IsReady = true;
    }

    /// <summary>
    /// Success callback for fetching the count of TestGroup1Text.
    /// </summary>
    /// <param name="data">The JSON string containing the count.</param>
    private void OnRequestSuccess1(string data)
    {
        Log("G1 data received: " + data);
        if (int.TryParse(data, out int count))
        {
            groupCounts[1] = count;
        }

        // Now fetch group 2:
        GetJSON(path: "group_counts/TestGroup2Arrows", gameObject.name, callback: "OnRequestSuccess2", fallback: "OnRequestFailed");
    }

    /// <summary>
    /// Success callback for fetching the count of TestGroup2Arrows.
    /// </summary>
    /// <param name="data">The JSON string containing the count.</param>
    private void OnRequestSuccess2(string data)
    {
        Log("G2 data received: " + data);
        if (int.TryParse(data, out int count))
        {
            groupCounts[2] = count;
        }

        // Now fetch group 3:
        GetJSON(path: "group_counts/ControlGroupBlank", gameObject.name, callback: "OnRequestSuccess3", fallback: "OnRequestFailed");
    }

    /// <summary>
    /// Success callback for fetching the count of ControlGroupBlank.
    /// </summary>
    /// <param name="data">The JSON string containing the count.</param>
    private void OnRequestSuccess3(string data)
    {
        Log("G3 data received: " + data);
        if (int.TryParse(data, out int count))
        {
            groupCounts[3] = count;
        }

        // After all counts are fetched, assign a group:
        AssignGroup();
    }


    // Group Assignment Logic: ------------------------------------------------------------------------

    /// <summary>
    /// Selects the group with the lowest current count.
    /// In case of a tie, it randomly selects one of the tied groups.
    /// </summary>
    private void AssignGroup()
    {
        // Find the minimum count among all groups.
        int minCount = int.MaxValue;
        if (groupCounts[1] < minCount) minCount = groupCounts[1];
        if (groupCounts[2] < minCount) minCount = groupCounts[2];
        if (groupCounts[3] < minCount) minCount = groupCounts[3];

        // Collect all group IDs that have the minimum count.
        var minCountGroups = new System.Collections.Generic.List<int>();
        if (groupCounts[1] == minCount) minCountGroups.Add(1);
        if (groupCounts[2] == minCount) minCountGroups.Add(2);
        if (groupCounts[3] == minCount) minCountGroups.Add(3);

        // Randomly select one of the tied groups.
        if (minCountGroups.Count > 0)
        {
            int randomIndex = Random.Range(0, minCountGroups.Count);
            nextGroup = minCountGroups[randomIndex];
        }
        else
        {
            // Fallback if all counts are somehow invalid.
            nextGroup = 0;
        }

        Log("Assigned to group: " + nextGroup);
        IsReady = true;
    }


    // Debugging and Logging: -------------------------------------------------------------------------

    /// <summary>
    /// Logs a message to the console.
    /// </summary>
    /// <param name="message">The message to log.</param>
    private void Log(string message)
    {
#if UNITY_WEBGL && DEVELOPMENT_BUILD
        Debug.Log(message);
#endif
    }

}