using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[System.Serializable]
public class Path {
    [SerializeField,HideInInspector]
    List<Vector2> points;
    [SerializeField, HideInInspector]
    bool isClosed;
    [SerializeField, HideInInspector]
    bool autoSetControllPoints;

    public Path(Vector2 center) {
        points = new List<Vector2> {
            center+Vector2.left,
            center+(Vector2.left +Vector2.up) * 0.5f,
            center+(Vector2.right + Vector2.down) * 0.5f,
            center+Vector2.right
        };
    }

    //INDEXER
    public Vector2 this[int i]{
        get {
            return points[i];
        } 
    }

    public bool IsClosed {
        get {
            return isClosed;
        }
        set {
            if (isClosed != value) {
                isClosed = value;

                if (isClosed) {
                    points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
                    points.Add(points[0] * 2 - points[1]);

                    if (autoSetControllPoints) {
                        AutoSetAnchorPoints(0);
                        AutoSetAnchorPoints(points.Count - 3);
                    }
                } else {
                    points.RemoveRange(points.Count - 2, 2);
                    if (autoSetControllPoints) {
                        AutoSetSartEndControlls();
                    }
                }

            }
        }
    }

    public bool AutoSetControllPoints{
        get {
            return autoSetControllPoints;
        }
        set {
            if (autoSetControllPoints != value) {
                autoSetControllPoints = value;
                if (autoSetControllPoints) {
                    AutoSetAllControllPoints();
                }
            }
        }
    }

    public int NumPoints { 
        get { 
            return points.Count; 
        }
    }
    public int NumSegments{
        get{
            return points.Count/3;
        }
    }

    public void AddSegment(Vector2 anchPos) {
        points.Add(points[points.Count-1]*2 - points[points.Count-2]);
        points.Add((points[points.Count-1] + anchPos) * 0.5f);
        points.Add(anchPos);

        if (autoSetControllPoints) {
            AutoSetAffectedControllPoints(points.Count-1);
        }
    }


    public void SpiliteSegment(Vector2 anchorPos, int segmentIndex) {

        points.InsertRange(segmentIndex * 3 + 2, new Vector2[] {Vector2.zero, anchorPos, Vector2.zero });
        if (autoSetControllPoints) {
            AutoSetAffectedControllPoints(segmentIndex * 3 + 3);
        } else {
            AutoSetAnchorPoints(segmentIndex*3+3);
        }

    }

    public void RemoveSegment(int anchorIndex) {
        if (NumSegments > 2 || !isClosed && NumSegments > 1) {
            if (anchorIndex == 0) {
                if (isClosed) {
                    points[points.Count - 1] = points[2];
                }
                points.RemoveRange(0, 3);
            } else if (anchorIndex == points.Count - 1 && !isClosed) {
                points.RemoveRange(anchorIndex - 2, 3);
            } else {
                points.RemoveRange(anchorIndex - 1, 3);
            }
        }
    }


    public Vector2[] GetPointsInSegment(int indx) {
        return new Vector2[] { points[indx * 3], points[indx*3+1], points[indx * 3 + 2], points[LoopIndex(indx * 3 + 3)]};
    }

    public void MovePoint(int indx, Vector2 newPos) {

        Vector2 deltaMove = newPos - points[indx];

        if(indx%3==0 || !autoSetControllPoints){

        points[indx] = newPos;


            if (autoSetControllPoints) {
                AutoSetAffectedControllPoints(indx);
            } else {

                if (indx % 3 == 0) {
                    if (indx + 1 < points.Count || isClosed) points[LoopIndex(indx + 1)] += deltaMove;
                    if (indx - 1 > 0 || isClosed) points[LoopIndex(indx - 1)] += deltaMove;
                } else {
                    bool nextPoinIsAnch = (indx + 1) % 3 == 0;
                    int corresspondingControllIndx = (nextPoinIsAnch) ? indx + 2 : indx - 2;
                    int anchorIndx = (nextPoinIsAnch) ? indx + 1 : indx - 1;

                    if (corresspondingControllIndx > 0 && corresspondingControllIndx < points.Count || isClosed) {
                        float dis = (points[LoopIndex(anchorIndx)] - points[LoopIndex(corresspondingControllIndx)]).magnitude;
                        Vector2 dir = (points[LoopIndex(anchorIndx)] - newPos).normalized;
                        points[LoopIndex(corresspondingControllIndx)] = points[LoopIndex(anchorIndx)] + dir * dis;
                    }
                }
            }
        }

    }

    public Vector2[] CalculateEvenSpacedPoint(float spacing, float res=1.0f) {

        List<Vector2> evenlySpacedPoints = new List<Vector2>();
        evenlySpacedPoints.Add(points[0]);
        Vector2 previusPoint = points[0];
        float distanceSinceLastEvenPoint = 0;

        for (int segmentIndex=0; segmentIndex < NumSegments; segmentIndex++) {

            Vector2[] p = GetPointsInSegment(segmentIndex);
            float ControllNetLength = Vector2.Distance(p[0], p[1]) + Vector2.Distance(p[1], p[2]) + Vector2.Distance(p[2], p[3]);
            float estimatedCurveLength = Vector2.Distance(p[0], p[3]) + ControllNetLength / 2f;
            int divisions = Mathf.CeilToInt(estimatedCurveLength * res * 10);
            float t = 0;
            while (t<=1) {
                t += 1f / divisions;
                Vector2 pointsOnCurve = Bezier.EvaluateCubic(p[0], p[1], p[2], p[3], t);
                distanceSinceLastEvenPoint += Vector2.Distance(previusPoint,pointsOnCurve);

                while (distanceSinceLastEvenPoint >= spacing) {
                    float oveShoot = distanceSinceLastEvenPoint-spacing;
                    Vector2 newEvenlySpacedPoint = pointsOnCurve + (previusPoint-pointsOnCurve).normalized * oveShoot;
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    distanceSinceLastEvenPoint = oveShoot;
                    previusPoint = newEvenlySpacedPoint;
                }
                previusPoint = pointsOnCurve;
            }
        }

        return evenlySpacedPoints.ToArray();
    }

    void AutoSetAffectedControllPoints(int updatedAnchorIndex) {
        for (int i = updatedAnchorIndex - 3; i <= updatedAnchorIndex + 3; i += 3) {
            if (i >= 0 && i < points.Count || isClosed) {
                AutoSetAnchorPoints(LoopIndex(i));
            }

            AutoSetSartEndControlls();
        }
    }

    void AutoSetAllControllPoints() {
        for (int i = 0; i < points.Count; i += 3) {
            AutoSetAnchorPoints(i);
        }
        AutoSetSartEndControlls();
    }

    void AutoSetAnchorPoints(int anchorIndex) {

        Vector2 anchorPos = points[anchorIndex];
        Vector2 dir = Vector2.zero;
        float[] neighbourDis = new float[2];

        if (anchorIndex - 3 >= 0 || isClosed) {
            Vector2 offset = points[LoopIndex(anchorIndex-3)] - anchorPos;
            dir += offset.normalized;
            neighbourDis[0] = offset.magnitude;
        }

        if (anchorIndex + 3 >= 0 || isClosed) {
            Vector2 offset = points[LoopIndex(anchorIndex + 3)] - anchorPos;
            dir -= offset.normalized;
            neighbourDis[1] = -offset.magnitude;
        }

        dir.Normalize();

        for (int i = 0; i < 2; i++) {
            int controllIndex = anchorIndex + i * 2 - 1;
            if (controllIndex >= 0 && controllIndex<points.Count || isClosed) {
                points[LoopIndex(controllIndex)] = anchorPos + dir * neighbourDis[i] * 0.5f;
            }
        }
    }

    void AutoSetSartEndControlls() {
        if (!isClosed) {
            points[1] = (points[0] + points[2]) * 0.5f;
            points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * 0.5f;
        }
    }

    int LoopIndex(int i) {
        return (points.Count +i) % points.Count;
    }

}
