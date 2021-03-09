using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject Target;
    public float DampSpeed = 0.2f;

    #region
    float DampVelocityX = 0;
    #endregion

    // Start is called before the first frame update
    void Awake()
    {
        Target = Target == null ? GameObject.FindGameObjectWithTag("Player") : Target;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = new Vector3(Mathf.SmoothDamp(transform.position.x, Target.transform.position.x, ref DampVelocityX, DampSpeed), transform.position.y, -10);
    }
}
