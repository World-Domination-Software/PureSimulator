using UnityEngine;
using UnityEngine.Rendering.Universal;

//a simple freelook camera with ability to move around but always looks at the server object!
public class CameraController : MonoBehaviour
{
    public Transform Target;
    public float zoomSpeed, moveSpeed, mouseMoveSpeed;
    public float minY, maxY, minX, maxX, minZ, maxZ;
    public bool invertXMotion = false;
    public bool invertZMotion = false;

    public Vector3 offset;
    public bool isFrontCam = true;

    private Camera cam;
    private float newX, newY, newZ;
    private Vector3 originalOffset;

    private void Start()
    {
        cam = GetComponent<Camera>();
        newX = offset.x;
        newY = offset.y;
        newZ = offset.z;

        originalOffset = offset;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Vector3 pos = Target.position + offset;
        transform.position = pos;
    }

    public void SetCameraToFocusOnChassis(Transform obj) 
    {
        if(!isFrontCam)
            offset = new Vector3(-20f, Mathf.Abs(obj.localPosition.x), -0.45f);
        else
            offset = new Vector3(-15f, Mathf.Abs(obj.localPosition.x) + 0.5f, -0.15f);
    }

    private void Update()
    {
        if (GlobalVar.commandProOpen)
            return;
        if (!Application.isFocused)
            return;

        //move up/down by arrow keys!
        if (!Input.GetMouseButton(1))
        {
            //up/down
            newY += Input.GetAxis("Vertical") * moveSpeed;
            newY = Mathf.Clamp(newY, minY, maxY);
            offset.y = newY;

            //left/right
            newZ -= Input.GetAxis("Horizontal") * moveSpeed * (invertXMotion ? -1f : 1f);
            newZ = Mathf.Clamp(newZ, minZ, maxZ);
            offset.z = newZ;
        }
        else //move up/down/left/right by right mouse button
        {
            newY += Input.GetAxis("Mouse Y") * mouseMoveSpeed;
            newY = Mathf.Clamp(newY, minY, maxY);
            offset.y = newY;

            newZ -= Input.GetAxis("Mouse X") * moveSpeed * (invertXMotion ? 1f : -1f);
            newZ = Mathf.Clamp(newZ, minZ, maxZ);
            offset.z = newZ;
        }

        //hide cursor while user holds right click to move!
        if (Input.GetMouseButtonDown(1)) 
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (Input.GetMouseButtonUp(1))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        //zooming
        newX += Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * (invertXMotion ? -1f : 1f);
        newX = Mathf.Clamp(newX, minX, maxX);
        offset.x = newX;

        //applying position
        Vector3 pos = Target.position + offset;
        transform.position = pos;
    }

    public void ResetPose() 
    {
        offset = originalOffset;
        newX = offset.x;
        newY = offset.y;
        newZ = offset.z;
    }
}
