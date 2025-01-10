using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPlacer : MonoBehaviour
{
    public float spacing = 0.1f;
    public float res = 1;

    // Start is called before the first frame update
    void Start()
    {
        Vector2[] points = FindAnyObjectByType<PathCreator>().path.CalculateEvenSpacedPoint(spacing,res);

        foreach (Vector2 point in points) {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g.transform.position = point;
            g.transform.localScale = Vector3.one * spacing * 0.5f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
