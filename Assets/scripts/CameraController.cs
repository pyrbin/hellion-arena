using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// https://gamedevacademy.org/unity-rts-camera-tutorial/
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Move Settings")]
    public float moveSpeed = 15;

    [Header("Zoom Settings")]
    public float zoomSpeed = 1000;

    public float minZoomDist = 10;
    public float maxZoomDist = 50;

    private Camera cam;

    public void Awake()
    {
        cam = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    public void Update()
    {
        Move();
        Zoom();
    }

    private void Move()
    {
        var xInput = Input.GetAxis("Horizontal");
        var yInput = Input.GetAxis("Vertical");

        var dir = (transform.forward) * yInput + transform.right * xInput;

        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    private void Zoom()
    {
        var scrollInput = Input.GetAxis("Mouse ScrollWheel");
        var dist = Vector3.Distance(transform.position, cam.transform.position);

        if (dist < minZoomDist && scrollInput > 0.0f)
            return;
        else if (dist > maxZoomDist && scrollInput < 0.0f)
            return;

        cam.transform.position += cam.transform.forward * scrollInput * zoomSpeed * Time.deltaTime;
    }
}