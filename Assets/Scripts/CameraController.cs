using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is used for the basic 1st person camera movement
// Interpreted from this video: https://www.youtube.com/watch?v=f473C43s8nE&t=193s
public class CameraController : MonoBehaviour
{
    public float xSensitivity = 400;
    public float ySensitivity = 400;

    float xRotation;
    float yRotation;

    // orientation represents the player
    public Transform orientation;

   
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * xSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * ySensitivity;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // apply rotation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);

        // move camera to player position
        transform.position = orientation.position + new Vector3(0, 1, 0);
    }
}
