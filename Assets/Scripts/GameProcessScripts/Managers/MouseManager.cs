using UnityEngine;


public class MouseManager : MonoBehaviour
{
    // Camera
    [SerializeField] private Camera nonStaticCam;
    private static Camera Cam;
    public static Camera getCamera() { return Cam; }


    // Camera movement and zooming
    [SerializeField] private float movementSpeed, zoomSpeed, minCamZoom, maxCamZoom;

    private float zoom;
    private Vector3 movement, dragOrigin, dragDifference;


    private void Awake()
    {
        Cam = nonStaticCam;
    }
    void Update()
    {
        Zoom();
        MoveCamera();
    }

    // Camera Movement

    private void MoveCamera()
    {
        if (!MouseMovement())
            KeyboardMovement();
    }
    private void Zoom()
    {
        if (Input.GetKey(KeyCode.KeypadPlus)) zoom = - zoomSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.KeypadMinus)) zoom = zoomSpeed * Time.deltaTime;

        if (Input.mouseScrollDelta.y > 0) zoom = - zoomSpeed * Time.deltaTime * 10f;
        if (Input.mouseScrollDelta.y < 0) zoom = zoomSpeed * Time.deltaTime * 10f;

        Cam.orthographicSize = Mathf.Clamp(Cam.orthographicSize + zoom, minCamZoom, maxCamZoom);
        zoom = 0;
    }
    private bool MouseMovement()
    {
        if (Input.GetMouseButtonDown(2)) dragOrigin = Cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(2))
        {
            dragDifference = dragOrigin - Cam.ScreenToWorldPoint(Input.mousePosition);
            Cam.transform.position += dragDifference;

            return true;
        }
        return false;
    }
    private void KeyboardMovement()
    {
        movement = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.DownArrow)) movement.y -= 1;
        if (Input.GetKey(KeyCode.UpArrow)) movement.y += 1;
        if (Input.GetKey(KeyCode.LeftArrow)) movement.x -= 1;
        if (Input.GetKey(KeyCode.RightArrow)) movement.x += 1;

        Cam.transform.position += movement * movementSpeed * Time.deltaTime;
    }


}
