using System.Collections;
using UnityEngine;
using Assets.Scripts;

public enum AIAction {
    Spawn = 0,
    Move,
    Shoot,
    TakeDamage,
    Die = 4
};

public class TelemetryEmitter : MonoBehaviour {

    private TelemetryManager manager;
    private int sessionID;
    private int teamID;
    private int botID;
    private bool movementTrackingEnabled = false;
    private Vector2 lastLocation;
    private float movementTick = 0.5f;
    private float headingLength = 3.0f; // Multiply the foraward vector by this, increases heading line.

    void Start() {
        manager = FindObjectOfType<TelemetryManager>();
        if (!manager.settings.data.telemetryEnabled) {
            this.enabled = false; // Disable the emitter if telemetry is disabled.
        }
        else { // Telemetry is enabled.
            sessionID = manager.sessionID;
            teamID = GetComponent<Unit>().Team;
            botID = FindObjectsOfType<TelemetryEmitter>().Length;
            headingLength = manager.settings.data.visualizerMarkerHeadingLength;
        }

        if (manager.settings.data.trackUnitMovement) {
            movementTick = manager.settings.data.trackUnitMovementInterval;
            lastLocation = new Vector2(transform.position.x, transform.position.z);
            StartCoroutine("TrackLocation");
        }

        EmitTelemetryRecord(AIAction.Spawn);
    }

    IEnumerator TrackLocation() {
        for (; ;) {
            Vector2 newLoc = new Vector2(transform.position.x, transform.position.z);
            if (lastLocation != newLoc) {
                lastLocation = newLoc;
                EmitTelemetryRecord(AIAction.Move);
            }
            yield return new WaitForSeconds(movementTick);
        }
    }

    private void OnDestroy() {
        StopCoroutine("TrackLocation");
        EmitTelemetryRecord(AIAction.Die);
    }

    public void EmitTelemetryRecord(AIAction newAction) {
        float xLoc = transform.position.x;
        float zLoc = transform.position.z;
        float xHead = xLoc + (transform.forward.x * headingLength);
        float zHead = zLoc + (transform.forward.z * headingLength);
        TelemetryManager.TelemetryRecordSimple record = manager.GetEmptyRecord();
        record.sessionID = sessionID;
        record.timestamp = Time.timeSinceLevelLoad;
        record.action = newAction;
        record.teamID = teamID;
        record.botID = botID;
        record.xLocation = xLoc;
        record.zLocation = zLoc;
        record.xHeading = xHead;
        record.zHeading = zHead;
        manager.AddDataRecord(record);
    }
}
