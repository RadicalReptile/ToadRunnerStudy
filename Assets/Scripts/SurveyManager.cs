using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages communication with the Firebase function to register a unique player ID,
/// then opens the survey link with the necessary data.
/// </summary>
public class SurveyManager : MonoBehaviour
{
    // Singleton Instance:
    /// <summary>
    /// Public static reference to allow easy access from other scripts (e.g., SurveyManager.Instance.OpenSurvey()).
    /// </summary>
    public static SurveyManager Instance { get; private set; }

    [Header("Firebase & Survey Configuration")]
    [Tooltip("URL of the Firebase Cloud Function to register the Unity ID.")]
    [SerializeField] private string firebaseRegisterUrl = "https://us-central1-YOUR_PROJECT.cloudfunctions.net/registerUnityId";
    [Tooltip("The secure token for accessing the Firebase Cloud Functions.")]
    [SerializeField] private string surveyToken = "SURVEY_TOKEN";
    [Tooltip("The base URL of your (Jotform) survey.")]
    [SerializeField] private string surveyUrlBase = "https://YOUR_SURVEY_URL";

    private string unityId;
    private string assignedGroup;
    private string chosenDirection;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Generate a persistent GUID for the player:
        // This is done once per player, even if they return to the game.
        if (!PlayerPrefs.HasKey("UnityGUID"))
        {
            unityId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString("UnityGUID", unityId);
        }
        else
        {
            unityId = PlayerPrefs.GetString("UnityGUID");
        }

        Debug.Log("Player's Unity ID is " + unityId);
    }

    /// <summary>
    /// Public method to call when the player finishes the game:
    /// It sets the required data and initiates the Firebase registration.
    /// </summary>
    /// <param name="groupName">The assigned group name (e.g., "TestGroup1Text").</param>
    /// <param name="finalDirection">The final chosen direction (e.g., "left").</param>
    public void OpenSurvey(string groupName, string finalDirection)
    {
        if (string.IsNullOrEmpty(groupName) || string.IsNullOrEmpty(finalDirection))
        {
            Debug.LogError("Group or direction data is missing. Cannot open survey.");
            return;
        }

        assignedGroup = groupName;
        chosenDirection = finalDirection;
        StartCoroutine(RegisterUnityId());
    }

    /// <summary>
    /// Coroutine to post the player's data to the Firebase function.
    /// </summary>
    private IEnumerator RegisterUnityId()
    {
        // Use a serializable class for the JSON payload.
        UnityIdPayload payload = new UnityIdPayload
        {
            token = surveyToken,
            unityId = unityId,
            direction = chosenDirection,
            group = assignedGroup
        };

        string jsonPayload = JsonUtility.ToJson(payload);

        using (UnityWebRequest request = new UnityWebRequest(firebaseRegisterUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                // E.g. even if the same GUID stopped playing and hadn't submitted the survey yet,
                // it is important to try and limit to the first impression in this experiment.
                Debug.LogError($"Error registering Unity ID: {request.error}\nResponse: {request.downloadHandler.text}");
            }
            else
            {
                Debug.Log("RegisterUnityId successful. Opening survey.");
                LaunchSurvey();
            }
        }
    }

    /// <summary>
    /// Builds and opens the final survey URL with all necessary parameters.
    /// </summary>
    private void LaunchSurvey()
    {
        string groupCode = MapGroupToCode(assignedGroup);
        string directionCode = MapDirectionToCode(chosenDirection);

        if (string.IsNullOrEmpty(groupCode) || string.IsNullOrEmpty(directionCode))
        {
            Debug.LogError("Failed to map group or direction to a valid code. Cannot open survey.");
            return;
        }

        string finalSurveyUrl = $"{surveyUrlBase}?unityId={Uri.EscapeDataString(unityId)}" +
                                $"&g={Uri.EscapeDataString(groupCode)}" +
                                $"&d={Uri.EscapeDataString(directionCode)}";
        Debug.Log("Opening survey at URL: " + finalSurveyUrl);
        Application.OpenURL(finalSurveyUrl);

        AudioListener.volume = 0.0f;
    }

    /// <summary>
    /// Maps a full group name to a short code for the URL.
    /// </summary>
    private string MapGroupToCode(string groupName)
    {
        switch (groupName)
        {
            case "TestGroup1Text": return "a";
            case "TestGroup2Arrows": return "b";
            case "ControlGroupBlank": return "c";
            default: return "";
        }
    }

    /// <summary>
    /// Maps a full direction name to a short code for the URL.
    /// </summary>
    private string MapDirectionToCode(string directionName)
    {
        switch (directionName.ToLower())
        {
            case "left": return "x";
            case "right": return "y";
            default: return "";
        }
    }

    [Serializable]
    private class UnityIdPayload
    {
        public string token;
        public string unityId;
        public string direction;
        public string group;
    }
}