using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class CameraController : MonoBehaviour
{
    public GameObject Target;
    public float DampSpeed = 0.2f;
    public Vector2 RegionOffset;
    public Vector2 RegionSize;

    #region
    Camera cam;
    float DampVelocityX = 0;
    #endregion

    // Start is called before the first frame update
    void Awake()
    {
        cam = GetComponentInChildren<Camera>();
        Target = Target == null ? GameObject.FindGameObjectWithTag("Player") : Target;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 current = new Vector3(Mathf.SmoothDamp(transform.position.x, Target.transform.position.x, ref DampVelocityX, DampSpeed), transform.position.y, -10);
        if (MathC.IsInSquare(current + new Vector3(cam.orthographicSize * Screen.width / Screen.height, 0), RegionOffset, RegionSize) && MathC.IsInSquare(current - new Vector3(cam.orthographicSize * Screen.width / Screen.height, 0), RegionOffset, RegionSize))
        {
            
            transform.position = current;
        } 
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(RegionOffset, RegionSize);
    }
}
