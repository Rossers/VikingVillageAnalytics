/* Telemetry Manager 
 * Copyright: Ross A. Metcalf 2019
 * Summary: Manages telemetry data provided by telemetry emitters attached to game objects.
 * Description:
 * #### Settings ####
 * This program will always attempt to load settings from a TelemetrySettings.json file located in
 * a relative filepath. If the file doesn't exist, it will use default settings and create the file.
 * If the file does exist, it will check to see if hasRemoteTelemetrySettings is true. If so, it 
 * will load the settings from the given URL.
 * No matter where the settings come from, this program will copy them and write them to the 
 * SettingsFile locally.
 * #### HOW TO USE ####
 * Attatch a TelemetryEmitter script to each prefab you want telemetry data from.
 * The emitter will automatically record Spawn and Death data when Start() and OnDestroy() are called.
 * The emitter will automatically record positional/movement data if trackUnitMovement setting is true.
 * To track additional behavior, you must manually call the TelemetryEmitter XX function from the 
 * code of the prefab object.
 */

using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class TelemetryManager : MonoBehaviour {
    // Singelton setup.
    private static TelemetryManager _instance;
    public static TelemetryManager Instance { get { return _instance; } }

    // File IO variables for settings file and record saving location.
    private bool hasLocalTelemetrySettings;   // Local settings file exists?
    private string fullInitFilepath;          // Full path to the TelemetrySettings.json file.
    private int maxPoolOverflow = 120;        // The empty record pool will overflow by this amount before refusing to give empty records.
    public int sessionID;                     // Unique ID generated for each game session.  
    private string telemetryFilename;
    private string fileExtension = ".json";
    public string telemetryDirectory;
    public Camera mainCamera;                 // Must manually assign this from the scene.

    // Default Telemetry settings, overwritten if TelemetrySettings.json exists.
    public bool DEFAULT_telemetryEnabled = true;            // Will disable all TelemetryEmitter actions if false.
    public bool DEFAULT_visualizerEnabled = true;
    public bool DEFAULT_trackUnitMovement = true;           // Track positional movement every frame? Generates tons of records.    
    public float DEFAULT_trackUnitMovementInterval = 0.6f;  // How often movement records should be recorded.
    public string DEFAULT_telemetryURL = "https://parseapi.back4app.com"; // ex. https://parseapi.back4app.com
    public string DEFAULT_telemetryAppID = "I35rhH6ylsr3IsUSVNin3k7RUILz73Gw6LKPXZen"; // ex. I35rhH6ylsr3IsUSVNin3k7RUILz73Gw6LKPXZen
    public string DEFAULT_telemetryAPIKey = "bp4rO0HIAqOxrNtRX9zWteQsnfYDNPPRAhCToIeN"; // ex. bp4rO0HIAqOxrNtRX9zWteQsnfYDNPPRAhCToIeN
    public int DEFAULT_recordsPerSend = 1000;               // How many records to store before saving/sending.    
    public int DEFAULT_visualizerMaxMarkerCount = 1400;           // Number of markers the visualizer displays before deleting old ones.
    public float DEFAULT_visualizerMarkerSize = 3.0f;
    public float DEFAULT_visualizerMarkerElevationOffset = 7.0f;
    public float DEFAULT_visualizerMarkerHeadingLength = 3.0f;   // How far the heading vector will be away from the unit.

    // The data objects.
    private TelemetryRecordList dataPool;
    private Stack<TelemetryRecordSimple> blankRecordPool;
    public TelemetrySettings settings;
    private TelemetryVisualizer visualizer;

    [System.Serializable]
    public struct TelemetryRecordSimple {
        public int sessionID;
        public float timestamp;
        public AIAction action;
        public int teamID;
        public int botID;
        public float xLocation;
        public float zLocation;
        public float xHeading;
        public float zHeading;
    }

    // Class for storing serializable data records.
    public class TelemetryRecordList {
        public List<TelemetryRecordSimple> data = new List<TelemetryRecordSimple>();
    }

    [System.Serializable]
    public struct TelemetrySettingData {
        public bool telemetryEnabled;
        public bool visualizerEnabled;
        public bool trackUnitMovement;
        public float trackUnitMovementInterval;
        public string telemetryURL;
        public string telemetryAppID;
        public string telemetryAPIKey;
        public int recordsPerSend;
        public int visualizerMaxMarkerCount;
        public float visualizerMarkerElevationOffset;
        public float visualizerMarkerSize;
        public float visualizerMarkerHeadingLength;
        public int lastSessionRecorded;
    }

    // Class for managing serializable telemetry settings.
    public class TelemetrySettings {
        public TelemetrySettingData data = new TelemetrySettingData();
    }

    private void Awake() {
        // Setup singleton.
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        }
        else {
            _instance = this;
        }

        // Setup the relative filepath.
        telemetryDirectory = Path.Combine(Application.dataPath, "Telemetry");

        // Setup relative paths.
        string initFilename = "TelemetrySettings" + fileExtension;
        fullInitFilepath = Path.Combine(telemetryDirectory, initFilename);

        // Check to see if local TelemetrySettings.json exists.
        if (File.Exists(fullInitFilepath)) {
            hasLocalTelemetrySettings = true;
        }
        else {
            hasLocalTelemetrySettings = false;
        }
        settings = new TelemetrySettings();

        // Set the filename based on the generated session ID.
        string date = System.DateTime.Now.ToString("MMddHHmmss"); // Can't add year, or string is too large to convert to int.
        sessionID = int.Parse(date); //ex. 0313143405 = March 13th at 2:34:05pm "MMddHHmmss"
        telemetryFilename = sessionID + fileExtension;

        // If there is a local TelemetrySettings file, import and apply the settings.
        if (hasLocalTelemetrySettings) {
            Debug.Log("has local settings, fetching...");
            GetLocalTelemetrySettings();
        }
        else { // Use the default settings from the inspector/default values.
            Debug.Log("doesn't have local settings, applying default values...");
            ApplyDefaultTelemetrySettings();
        }

        // After getting and applying the settings from any source, save them locally.
        SaveTelemetrySettings();

        // Initialize pool of blank telemetry data records.
        blankRecordPool = new Stack<TelemetryRecordSimple>(DEFAULT_recordsPerSend + maxPoolOverflow);
        for (int i = 0; i < DEFAULT_recordsPerSend + maxPoolOverflow; i++) {
            TelemetryRecordSimple record = new TelemetryRecordSimple();
            record = ClearRecordData(record);
            blankRecordPool.Push(record);
        }

        if (settings.data.visualizerEnabled) {
            visualizer = GetComponent<TelemetryVisualizer>();
            visualizer.Initialize(settings.data.visualizerMarkerElevationOffset, settings.data.visualizerMarkerSize, settings.data.visualizerMaxMarkerCount);
        }

        // Initialize an empty queue to be filled with populated telemetry data records.
        dataPool = new TelemetryRecordList();
    }

    private void Start() {
        GetScreenshot();
    }

    private void GetScreenshot() {
        Camera overviewCam = GetComponentInChildren<Camera>();
        mainCamera.enabled = false;
        overviewCam.enabled = true;

        // Call custom screenshot capture function.
        TakeHiResScreenshot(overviewCam, 1, "OverheadShot");
        TakeHiResScreenshot(overviewCam, 2, "OverheadShot");

        overviewCam.enabled = false;
        mainCamera.enabled = true;
        print("Screenshot captured, called MapOverheadView.png");
    }

    private void GetLocalTelemetrySettings() {
        StreamReader reader = new StreamReader(fullInitFilepath, true);
        settings.data = JsonUtility.FromJson<TelemetrySettingData>(reader.ReadToEnd());
        reader.Close();
    }

    private void ApplyDefaultTelemetrySettings() {
        settings.data.telemetryEnabled = DEFAULT_telemetryEnabled;
        settings.data.visualizerEnabled = DEFAULT_visualizerEnabled;
        settings.data.trackUnitMovement = DEFAULT_trackUnitMovement;
        settings.data.trackUnitMovementInterval = DEFAULT_trackUnitMovementInterval;
        settings.data.telemetryURL = DEFAULT_telemetryURL;
        settings.data.telemetryAppID = DEFAULT_telemetryAppID;
        settings.data.telemetryAPIKey = DEFAULT_telemetryAPIKey;
        settings.data.recordsPerSend = DEFAULT_recordsPerSend;
        settings.data.visualizerMaxMarkerCount = DEFAULT_visualizerMaxMarkerCount;
        settings.data.visualizerMarkerHeadingLength = DEFAULT_visualizerMarkerHeadingLength;
        settings.data.visualizerMarkerElevationOffset = DEFAULT_visualizerMarkerElevationOffset;
        settings.data.visualizerMarkerSize = DEFAULT_visualizerMarkerSize;
    }

    private void SaveTelemetrySettings() {
        // Verify or create the directory
        if (!Directory.Exists(telemetryDirectory)) {
            Directory.CreateDirectory(telemetryDirectory);
        }

        // Verify or create the file
        if (File.Exists(fullInitFilepath)) {
            File.Delete(fullInitFilepath);
        }
        File.Create(fullInitFilepath).Dispose();

        settings.data.lastSessionRecorded = sessionID;

        string jsonSettings = JsonUtility.ToJson(settings.data);
        Debug.Log("TM: jsonSettings = " + jsonSettings);

        // Write the data to the file
        StreamWriter writer = new StreamWriter(fullInitFilepath, true);
        writer.Write(jsonSettings);
        writer.Close();
        writer.Dispose();
    }

    private TelemetryRecordSimple ClearRecordData(TelemetryRecordSimple record) {
        record.timestamp = -1;
        record.action = AIAction.Die;
        record.teamID = -1;
        record.botID = -1;
        record.xLocation = -1;
        record.zLocation = -1;
        return record;
    }

    public TelemetryRecordSimple GetEmptyRecord() {
        return blankRecordPool.Pop();
    }

    public void AddDataRecord(TelemetryRecordSimple record) {
        dataPool.data.Add(record);
        Debug.Log("data record added, record #" + dataPool.data.Count());

        // If visualizer is enabled, send it the new record.
        if (settings.data.visualizerEnabled) {
            // VisualizeRecord was sometimes being called after the visualizer was destroyed, so we check if its null first.
            if (visualizer != null) {
                visualizer.VisualizeRecord(record);
            }
        }

        if (dataPool.data.Count() >= settings.data.recordsPerSend) {
            SendData();
        }
    }

    private void SendData() {
        // serialize each record into json.
        Debug.Log("Sending " + dataPool.data.Count() + " data records to " + telemetryDirectory);

        string payload = JsonUtility.ToJson(dataPool);
        Debug.Log("Payload: " + payload);

        bool saveSuccessful = SaveDataToFile(payload);
        bool postSucessful = PostDataToURL(payload);

        // sanitize and return the telemetry records to the blankRecordPool.
        for (int i = 0; i < dataPool.data.Count(); i++) {
            TelemetryRecordSimple temp = dataPool.data[i];
            temp = ClearRecordData(temp);
            blankRecordPool.Push(temp);
        }
        dataPool.data.Clear();
        Debug.Log("TM: Cleared dataRecordPool, current count = " + dataPool.data.Count());
        Debug.Log("TM: New blankRecordPool count = " + blankRecordPool.Count);
    }

    private bool PostDataToURL(string payload) {
        Http http = new Http(settings.data.telemetryURL, settings.data.telemetryAppID, settings.data.telemetryAPIKey);
        var pst = http.Put("classes/TopScoresTest/m6pZW0WuMM", payload);

        Debug.Log(http.Get("classes/TopScoresTest")); // Get request to test.
        if (http.Get("classes/TopScoresTest") != null) {
            return true;
        }
        return false;
    }

    private bool SaveDataToFile(string payload) {
        string fullPath = Path.Combine(telemetryDirectory, telemetryFilename);

        // Verify or create the directory
        if (!Directory.Exists(telemetryDirectory)) {
            Directory.CreateDirectory(telemetryDirectory);
        }

        // Verify or create the file
        if (!File.Exists(fullPath)) {
            File.Create(fullPath).Dispose();
        }

        // Write the data to the file
        StreamWriter writer = new StreamWriter(fullPath, true);
        writer.Write(payload);
        writer.Close();

        if (File.Exists(fullPath)) {
            return true;
        }
        return false;
    }

    // Takes a custom resolution screenshot.
    private void TakeHiResScreenshot(Camera camera, int detailModifier, string nameModifier) {
        int resWidth = 1920 * detailModifier; // Calculated manually for this specific camera.
        int resHeight = 1080 * detailModifier; // Calculated manually for this specific camera.
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = ScreenshotName(nameModifier, resWidth, resHeight, detailModifier);
        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("Took screenshot to: {0}", filename));
    }

    // Create and return a screenshot name.
    private string ScreenshotName(string nameMod, int width, int height, int detailMod) {
        return string.Format("{0}/{1}_{2}x{3}_z{4}.png", // Add "_{3}" to add date to end.            
            telemetryDirectory,      // Put desired filepath here, ex. Application.dataPath
            nameMod,
            width, height,
            detailMod);          // Here goes the pixel dimenions.
            //System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    void OnDestroy() {
        SendData();
    }
}

/* TakeHiResScreenshot code taken and modified from Unity Answers forum.
 * Username: jashan
 * Date Used: 3/29/2019 19:14
 * Date Posted: Jul 21, 2010 at 11:12AM
 * Link: https://answers.unity.com/questions/22954/how-to-save-a-picture-take-screenshot-from-a-camer.html
 */
