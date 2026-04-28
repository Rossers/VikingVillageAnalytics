using System.Collections.Generic;
using UnityEngine;

public class TelemetryVisualizer : MonoBehaviour {

    private float markerElevationOffset;
    private float sizeModifier;
    private List<Vector3>[] actionLocs; // Holds all the V3s where actions take place.
    private List<Vector3>[] actionHeads; // Holds all the V3 headings of actions.

    private Color[] colors;    
    private int actionTypes; // How many AIActions (enums) there are.
    private float movementShrink = 3.0f;
    private float nonDeathShrink = 2.0f;
    private bool isEditor;  // Used to determine if we will draw gizmos or instantiate visualizer cubes.
    private Queue<VisualizerMarker> markerQueue; // Used to store all the markers, so we can destroy the oldest ones when maxxed.
    private int maxMarkerCount;

    public VisualizerMarker markerPrefab; // Used for visualization in builds since gizmos are editor only. Cubes to save tris over spheres.
    
    private void Start() {
        TelemetryManager manager = GetComponent<TelemetryManager>();
        if (manager == null || manager.settings.data.visualizerEnabled == false) {
            this.enabled = false;
        }
    }

    public void Initialize(float offset, float size, int maxCount) {
        isEditor = Application.isEditor; // Check if isEditor here, Initialize finishes before Start().
        markerElevationOffset = offset;
        sizeModifier = size;
        maxMarkerCount = maxCount;

        if (isEditor) { // We will use gizmos in editor  
            sizeModifier /= 3; // Gizmos are larger than the marker prefab.
            maxMarkerCount /= 2; // Since gizmos counts will be kept separate by action type, we reduce the overall count.
            actionTypes = (System.Enum.GetValues(typeof(AIAction)).Length); // This is how many AIActions there are.

            actionLocs = new List<Vector3>[actionTypes];
            actionHeads = new List<Vector3>[actionTypes];

            for (int i = 0; i < actionTypes; i++) {
                actionLocs[i] = new List<Vector3>();
                actionHeads[i] = new List<Vector3>();
            }
        }
        else {
            markerQueue = new Queue<VisualizerMarker>();
        }

        colors = new Color[] {  // TODO: Hard-coded colors for visualizer, not ideal.
            Color.green,    // Spawn
            Color.cyan,     // Move
            Color.magenta,  // Shoot
            Color.yellow,   // Take Damage
            Color.red       // Die
        };
    }

    // Stores the record information in our data structure.
    public void VisualizeRecord(TelemetryManager.TelemetryRecordSimple record) {
        Vector3 temp = new Vector3(record.xLocation, markerElevationOffset, record.zLocation);
        Vector3 head = new Vector3(record.xHeading, markerElevationOffset, record.zHeading);

        if (isEditor) { // If in editor, add record to the gizmo array.
            AddVisualizedGizmo(record.action, temp, head);
        }
        else { // If in build, instantiate an object to visualize the obj.
            CreateVisualizationObject(record.action, temp, head, transform.rotation);
        }
    }

    private void AddVisualizedGizmo(AIAction action, Vector3 location, Vector3 heading) {
        // If we've exceeded our max marker aka gizmo count, get rid of the oldest marker.
        if (actionLocs[(int)action].Count >= maxMarkerCount) {
            actionLocs[(int)action].RemoveAt(0);
            actionHeads[(int)action].RemoveAt(0);   
        } 

        // Add the new action location and heading.
        actionLocs[(int)action].Add(location);
        actionHeads[(int)action].Add(heading);
    }

    // In build only, creates visualization object in place of a gizmo.
    private void CreateVisualizationObject(AIAction action, Vector3 location, Vector3 heading, Quaternion rotation) {
        // If the the max amount of markers has been displayed, delete the oldest.
        if (markerQueue.Count > maxMarkerCount) {
            VisualizerMarker oldestMarker = markerQueue.Dequeue();
            Destroy(oldestMarker.gameObject);
        }

        // Instantiate new game object, modify size and color according to action.
        VisualizerMarker newMarker = Instantiate<VisualizerMarker>(markerPrefab, location, rotation);
        newMarker.GetComponent<MeshRenderer>().material.SetColor("_Color", colors[(int)action]);
        float tempSize = sizeModifier;
        if (action == AIAction.Move) {
            tempSize /= movementShrink;
        }
        else if (action == AIAction.Shoot || action == AIAction.TakeDamage) {
            tempSize /= nonDeathShrink;
        }
        newMarker.transform.localScale *= tempSize;
        
        // Draw a line to represent the heading.
        newMarker.GetComponent<LineRenderer>().SetPositions(new Vector3[] { location, heading });
        newMarker.GetComponent<LineRenderer>().material.color = colors[(int)action];

        // Add marker to the queue.
        markerQueue.Enqueue(newMarker);
    }

    // Draws all the stored records from our data.
    void OnDrawGizmos() {
        // OnDrawGizmos is only called in editor, checking here for debugging. TODO: remove once working.
        if (!isEditor) {
            return;
        }

        for (int i = 0; i < actionTypes; i++) {
            Color temp = colors[i];
            temp.a = 0.75f;
            Gizmos.color = temp;

            for (int j = 0; j < actionLocs[i].Count; j++) {
                if (i == 1) { // draw the movement spheres differently
                    Gizmos.DrawSphere(actionLocs[i][j], sizeModifier / movementShrink);
                    Gizmos.DrawWireSphere(actionLocs[i][j], sizeModifier / movementShrink);
                    // Draw headings.
                    Gizmos.DrawLine(actionLocs[i][j], actionHeads[i][j]);
                }
                else if (i == 2 || i == 3) {
                    Gizmos.DrawSphere(actionLocs[i][j], sizeModifier / nonDeathShrink);
                    Gizmos.DrawWireSphere(actionLocs[i][j], sizeModifier / nonDeathShrink);
                    // Draw headings.
                    Gizmos.DrawLine(actionLocs[i][j], actionHeads[i][j]);
                }
                else {
                    Gizmos.DrawSphere(actionLocs[i][j], sizeModifier);
                    Gizmos.DrawWireSphere(actionLocs[i][j], sizeModifier);
                    // Draw headings.
                    Gizmos.DrawLine(actionLocs[i][j], actionHeads[i][j]);
                }
            }
        }
    }
}
