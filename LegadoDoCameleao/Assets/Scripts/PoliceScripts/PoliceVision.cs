using UnityEngine;
using UnityEngine.Rendering.Universal;
using System; 

public class PoliceVision : MonoBehaviour
{
    [Header("Visão e Detecção")]
    public float visionDistance = 7f; 
    public LayerMask obstacleLayer; 
    public LayerMask playerLayer; 
    public Light2D flashlight; 

    private float timeSinceStart = 0f; 
    private float visionDelay = 2.0f; // Tempo de invencibilidade inicial

    void Start()
    {
        if (flashlight != null) flashlight.enabled = false; // Começa apagada
    }

    void Update()
    {
        timeSinceStart += Time.deltaTime;

        // Liga a luz após 2 segundos
        if (timeSinceStart >= visionDelay)
        {
            if (flashlight != null && !flashlight.enabled) flashlight.enabled = true;
        }
    }

    public void CheckForPlayer(Vector3 policePosition, Vector2 sightDirection, Action<PoliceState> stateChanger)
    {
        if (timeSinceStart < visionDelay) return; // Ainda cego

        if (sightDirection.sqrMagnitude < 0.01f) sightDirection = Vector2.up; 

        RaycastHit2D hitPlayer = Physics2D.Raycast(policePosition, sightDirection, visionDistance, playerLayer);

        if (hitPlayer.collider != null)
        {
            RaycastHit2D hitBlocker = Physics2D.Raycast(policePosition, sightDirection, visionDistance, obstacleLayer);
            
            // Se viu o player e não tem parede na frente
            if (hitBlocker.collider == null || hitPlayer.distance < hitBlocker.distance)
            {
                stateChanger(PoliceState.Chase);
            }
        }
    }
    
    void OnDrawGizmos()
    {
        PoliceMovement movement = GetComponent<PoliceMovement>();
        if (movement != null)
        {
            Gizmos.color = Color.yellow;
            Vector2 sightDirection = movement.CurrentDirection.normalized;
            Gizmos.DrawRay(transform.position, sightDirection * visionDistance);
        }
    }
}