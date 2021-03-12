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
    public bool FollowY = false;

    #region
    Camera cam;
    float DampVelocityX = 0;
    float DampVelocityY = 0;
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
        Vector3 current = new Vector3(
            Mathf.SmoothDamp(transform.position.x, Target.transform.position.x, ref DampVelocityX, DampSpeed), 
            FollowY ? Mathf.SmoothDamp(transform.position.y, Target.transform.position.y, ref DampVelocityY, DampSpeed) : transform.position.y, 
            -10);

        if(current.x + cam.orthographicSize * Screen.width / Screen.height <= RegionOffset.x + RegionSize.x / 2 && current.x - cam.orthographicSize * Screen.width / Screen.height >= RegionOffset.x - RegionSize.x / 2)
        {
            transform.position = new Vector3(current.x, transform.position.y, -10);
        }

        if (current.y + cam.orthographicSize <= RegionOffset.y + RegionSize.y / 2 && current.y - cam.orthographicSize >= RegionOffset.y - RegionSize.y / 2)
        {
            transform.position = new Vector3(transform.position.x, current.y, -10);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(RegionOffset, RegionSize);
    }
}
