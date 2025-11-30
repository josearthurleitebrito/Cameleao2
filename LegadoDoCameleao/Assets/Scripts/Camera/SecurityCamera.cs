using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SecurityCamera : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("Arraste aqui o Nó Pai (Centro da Sala) que esta câmera vigia.")]
    public Transform roomCenterNode; 
    public float alertCooldown = 5.0f;

    [Header("Visual")]
    public Light2D cameraLight;
    public Color normalColor = Color.yellow;
    public Color alertColor = Color.red;

    [Header("Colisão (Importante!)")]
    public LayerMask obstacleLayer; // Arraste a layer 'Default' ou 'Walls'
    public LayerMask playerLayer;   // Arraste a layer 'Player'

    private float lastAlertTime = -10f;

    void OnTriggerStay2D(Collider2D other)
    {
        // 1. Verifica se é o Player
        if (other.CompareTag("Player"))
        {
            // Debug para saber se o colisor funcionou
            // Debug.Log("Câmera: Player está dentro da luz (Trigger).");

            if (Time.time > lastAlertTime + alertCooldown)
            {
                // 2. Verifica se tem parede na frente
                if (PlayerIsVisible(other.transform))
                {
                    Debug.Log("<color=red>CÂMERA: VISÃO CONFIRMADA! Disparando Alarme...</color>");
                    TriggerAlarm();
                }
                else
                {
                    // Debug.Log("Câmera: Player no trigger, mas Raycast falhou (bloqueado).");
                }
            }
        }
    }

    bool PlayerIsVisible(Transform player)
    {
        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;
        
        // Combina as layers
        int mask = obstacleLayer | playerLayer;

        // --- DEBUG VISUAL ---
        // Desenha uma linha vermelha da câmera até o player na aba Scene
        Debug.DrawRay(transform.position, direction, Color.red);
        // --------------------

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, mask);

        if (hit.collider != null)
        {
            // --- DEBUG DE COLISÃO ---
            // Isso vai te dizer exatamente no que o raio bateu primeiro
            Debug.Log($"Raio da Câmera bateu em: {hit.collider.name} (Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");
            // ------------------------

            if (hit.collider.CompareTag("Player"))
            {
                return true; 
            }
        }
        else
        {
            Debug.Log("Raio da Câmera não bateu em nada (passou direto ou layers erradas).");
        }
        return false; 
    }

    void TriggerAlarm()
    {
        lastAlertTime = Time.time;
        
        if (cameraLight != null) cameraLight.color = alertColor;
        Invoke("ResetLight", 2.0f); 

        // 3. Verifica o Gerente
        if (PoliceManager.instance != null)
        {
            if (roomCenterNode != null)
            {
                Debug.Log($"Câmera: Enviando chamado para a sala {roomCenterNode.name}...");
                PoliceManager.instance.DispatchNearestOfficer(roomCenterNode.position);
            }
            else
            {
                Debug.LogError("ERRO: Câmera não tem 'Room Center Node' configurado!");
            }
        }
        else
        {
            Debug.LogError("ERRO: PoliceManager não encontrado na cena!");
        }
    }

    void ResetLight()
    {
        if (cameraLight != null) cameraLight.color = normalColor;
    }
}