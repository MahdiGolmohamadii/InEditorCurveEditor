using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    PathCreator creator;
    Path Path {
        get {
            return creator.path;
        }
    }

    const float segmentSelectDistTreshhold = 0.1f;
    int selectedSegmentIndex = -1;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        if (GUILayout.Button("create new")) {
            Undo.RecordObject(creator,"Creat New");
            creator.CreatPath();
        }

        bool isClosed = GUILayout.Toggle(Path.IsClosed,"close");

        if (isClosed != Path.IsClosed) {
            Undo.RecordObject(creator,"Toggel Closed");
            Path.IsClosed = isClosed;
            
        }

        bool autoSetControll = GUILayout.Toggle(Path.AutoSetControllPoints,"Auto Set Controll Points");
        if (autoSetControll != Path.AutoSetControllPoints) {
            Undo.RecordObject(creator,"Auto set controll points toggel");
            Path.AutoSetControllPoints = autoSetControll;
        }

        if (EditorGUI.EndChangeCheck()) {
            SceneView.RepaintAll();
        }
    }

    void OnSceneGUI() {
        Input();
        Draw();
    }

    void Input() {

        Event guiEvent = Event.current;
        Vector2 mousPos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

        if(guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift) {
            if (selectedSegmentIndex != -1) {
                Undo.RecordObject(creator, "Spilite Segment");
                Path.SpiliteSegment(mousPos, selectedSegmentIndex);
            } else if(!Path.IsClosed) {
                Undo.RecordObject(creator, "Add Points");
                Path.AddSegment(mousPos);
            }
            
        }

        if(guiEvent.type == EventType.MouseDown && guiEvent.button == 1) {
            float minDisToAnchor = creator.anchorDiamitere * 0.5f;
            int closestAnchorIndex = -1;

            for (int i = 0; i < Path.NumPoints; i += 3) {
                float dist = Vector2.Distance(mousPos, Path[i]);
                if (dist < minDisToAnchor) {
                    minDisToAnchor = dist;
                    closestAnchorIndex = i;
                }
            }
            if (closestAnchorIndex != -1) {
                Undo.RecordObject(creator, "Delete Anchor");
                Path.RemoveSegment(closestAnchorIndex);
            }
        }

        if (guiEvent.type == EventType.MouseMove) {
            float minDistToSegment = segmentSelectDistTreshhold;
            int newSelectedSegmentIndex = -1;

            for (int i = 0; i < Path.NumSegments; i++) {

                Vector2[] points = Path.GetPointsInSegment(i);
                float dst = HandleUtility.DistancePointBezier(mousPos, points[0], points[3], points[1], points[2]);
                if (dst < minDistToSegment) {
                    minDistToSegment = dst;
                    newSelectedSegmentIndex = i;
                }
            }

            if (newSelectedSegmentIndex != selectedSegmentIndex) {
                selectedSegmentIndex = newSelectedSegmentIndex;
                HandleUtility.Repaint();
            }
        }

        HandleUtility.AddDefaultControl(0);

    }

    void Draw() {
        
        //DRAW LINES
        for(int i = 0; i<Path.NumSegments; i++) {
            Vector2[] points = Path.GetPointsInSegment(i);
            if (creator.showControllPoints) {
                Handles.color = Color.white;
                Handles.DrawLine(points[1], points[0]);
                Handles.DrawLine(points[2], points[3]);
            }
            
            Color SegmentColor = (i==selectedSegmentIndex && Event.current.shift)? creator.selectedSegmentColor : creator.segmentColor;
            Handles.DrawBezier(points[0], points[3], points[1], points[2], SegmentColor, null, 2);
        }
        
        //DRAW HANDELS
        
        for (int i = 0; i < Path.NumPoints; i++) {
            if (i % 3 == 0 || creator.showControllPoints) {
                Handles.color = (i % 3 == 0) ? creator.anchorColor : creator.controllColor;
                float handelSize = (i % 3 == 0) ? creator.anchorDiamitere : creator.controllDiamiter;
                Vector2 newPos = Handles.FreeMoveHandle(Path[i], handelSize, Vector2.zero, Handles.CylinderHandleCap);

                if (newPos != Path[i]) {
                    Undo.RecordObject(creator, "Move Point");
                    Path.MovePoint(i, newPos);

                }
            }
        }

    }


     void OnEnable() {

        creator = (PathCreator)target;

        if (creator.path == null) {
            creator.CreatPath();
        }

     }
}
