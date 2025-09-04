using UnityEngine;

public class ServerTurntable : MonoBehaviour
{
    public float rotationSpeed;

    private float yRot;
    private bool rotate;
    private Vector3 mousePos;

    private void Update()
    {
        if (GlobalVar.commandProOpen)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            rotate = true;
        }

        if (Input.GetMouseButtonUp(1))
        {
            rotate = false;
        }

        //control by arrows too!
        yRot -= Input.GetAxis("Horizontal") * rotationSpeed;

        if (rotate)
        {
            yRot -= Input.GetAxis("Mouse X") * 3f * rotationSpeed;
        }

        //turn towards drag direction!
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, yRot, 0f), Time.deltaTime * 100.0f);
    }
}
