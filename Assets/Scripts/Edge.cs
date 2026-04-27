using System;
using UnityEngine;


enum EdgeType {
    Critical, 
    Secondary, 
    Inactive
}
;
public class Edge : MonoBehaviour
{
    private Room A;
    private Room B;
    [SerializeField] private EdgeType type = EdgeType.Critical;
    [SerializeField] private float lineWidth = 0.1f;

    private LineRenderer lr;

    public void Init(Room a, Room b)
    {
        A = a;
        B = b;

        lr = gameObject.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.widthMultiplier = 0.1f;
    }

    public Room GetOtherEdge(Room current)
    {
        if (current.Id == A.Id)
        {
            return B;
        }  
        else if (current.Id == B.Id)
        {
            return A;
        } else {
            return null;
        }
        
    }

    public void DrawEdge()
    {
        lr.SetPosition(0, A.GetRoomCenter());
        lr.SetPosition(1, B.GetRoomCenter());
    }
    
}
