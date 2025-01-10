using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    [HideInInspector]
    public Path path;

    public Color anchorColor = Color.red;
    public Color controllColor = Color.white;
    public Color segmentColor = Color.green;
    public Color selectedSegmentColor = Color.blue;
    public float anchorDiamitere = 0.1f;
    public float controllDiamiter = 0.05f;
    public bool showControllPoints = true;


    public void CreatPath() {
        
        path = new Path(transform.position);
    }

    private void Reset() {
        CreatPath();
    }
}
