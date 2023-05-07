using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCam : MonoBehaviour
{
    private float mouseX = 0;
    private float mouseY = 0;
    public float sensitivity = 0;

    private float xRotation = 0;
    private float yRotation = 0;

    public Transform orientation;
    public Transform CameraHolder;

    [Header("Camera Changes")]

    [Header("Wallrunning")]
    public float wallrunningFOV;
    public float wallrunningTiltAngle;

    //[Header("Sprinting")]

    //[Header("Sliding")]

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        //get player's mouse input
        mouseX = Input.GetAxis("Mouse X") * sensitivity;
        mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        yRotation += mouseX;
        xRotation -= mouseY;

        //clamp x rotation so player doesnt flip around
        xRotation = Mathf.Clamp(xRotation, sensitivity - 90f, 90f);

        //camera rotation and orientation
        CameraHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    //camera effects

    public void changeFOV(float newFOV)
    {
        GetComponent<Camera>().DOFieldOfView(newFOV, 0.25f);
    }

    public void cameraTilt(float newTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, newTilt), 0.25f);
    }
}
