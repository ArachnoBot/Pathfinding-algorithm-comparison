using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera cam;
    public float moveSpeed = 5f;           // Speed at which the camera moves
    public float smoothTime = 0.3f;       // Time to smooth the movement
    public float zoomSpeed = 5f;          // Speed at which the camera zooms in and out
    public float minZoom = 5f;            // Minimum zoom level
    public float maxZoom = 20f;           // Maximum zoom level
    private float zoomMultiplier = 5f;

    public int tileCountX = 10;
    public int tileCountY = 10;

    private Vector3 velocity = Vector3.zero; // Used by SmoothDamp

    void Start()
    {
        cam = Camera.main;
        zoomMultiplier = cam.orthographicSize;
    }

    void Update()
    {
        // Get input from WASD keys
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Calculate the target position based on input
        Vector3 targetPosition = transform.position + moveSpeed * zoomMultiplier * Time.deltaTime * new Vector3(horizontal, vertical, 0).normalized;

        // Smoothly move the camera towards the target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // Handle zoom input
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scroll != 0.0f)
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);
            zoomMultiplier = cam.orthographicSize;
        }
    }
}
