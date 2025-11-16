using UnityEngine;
using System; 

public class PoliceVision : MonoBehaviour
{
    [Header("Visão e Detecção")]
    public float visionDistance = 7f; 
    public LayerMask obstacleLayer; 
    public LayerMask playerLayer; 

    public void CheckForPlayer(Vector3 policePosition, Vector2 sightDirection, Action<PoliceState> stateChanger)
    {
        if (sightDirection.sqrMagnitude < 0.01f)
        {
            sightDirection = Vector2.up; 
        }

        RaycastHit2D hitPlayer = Physics2D.Raycast(policePosition, sightDirection, visionDistance, playerLayer);

        if (hitPlayer.collider != null)
        {
            RaycastHit2D hitBlocker = Physics2D.Raycast(policePosition, sightDirection, visionDistance, obstacleLayer);
            
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