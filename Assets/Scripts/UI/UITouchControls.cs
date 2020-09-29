using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITouchControls : MonoBehaviour
{


    private void OnGUI()
    {
        foreach (Touch t in Input.touches)
        {
            Vector3 point = Vector3.zero;

            point = t.position;
            point.y = Screen.height - point.y;

            GUI.Label(new Rect(point, Vector2.one * 256f), "finger#" + t.fingerId);
        }
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        
    }
}
