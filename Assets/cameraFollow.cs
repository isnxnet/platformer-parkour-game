using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;         // Player's transform to follow
    public float smoothSpeed = 0.125f;  // Smooth speed for following
    public Vector3 offset;          // Offset of the camera from the player

    private void LateUpdate()
    {
        Vector3 desiredPosition = player.position + offset;  // Calculate the desired camera position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);  // Smooth movement
        transform.position = smoothedPosition;  // Move the camera
    }
}