using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PoliceManager : MonoBehaviour
{
    // --- ESTA É A LINHA QUE ESTAVA FALTANDO OU ERRADA ---
    public static PoliceManager instance; 
    // ----------------------------------------------------

    // Lista automática de todos os policiais na cena
    private List<PoliceFSM> allOfficers = new List<PoliceFSM>();

    void Awake()
    {
        // Configuração do Singleton (Garante que só existe um e é acessível globalmente)
        if (instance == null) 
        {
            instance = this;
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Encontra todos os policiais ativos na cena automaticamente
        // Nota: FindObjectsByType é o comando moderno, mas se sua Unity for antiga use FindObjectsOfType
        allOfficers = FindObjectsByType<PoliceFSM>(FindObjectsSortMode.None).ToList();
    }

    /// <summary>
    /// Chamado pela Câmera para enviar um policial até o local.
    /// </summary>
    /// <param name="targetLocation">A posição do Nó Pai (centro da sala).</param>
    public void DispatchNearestOfficer(Vector3 targetLocation)
    {
        PoliceFSM nearestOfficer = null;
        float shortestDistance = Mathf.Infinity;

        foreach (var officer in allOfficers)
        {
            if (officer == null) continue;

            // Ignora policiais que já estão perseguindo o jogador (prioridade máxima)
            if (officer.currentState == PoliceState.Chase) continue;

            // Calcula a distância
            float distance = Vector3.Distance(officer.transform.position, targetLocation);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestOfficer = officer;
            }
        }

        if (nearestOfficer != null)
        {
            Debug.Log($"<color=red>ALERTA DE CÂMERA!</color> Enviando {nearestOfficer.name} para investigar.");
            
            // Manda o policial investigar
            nearestOfficer.TriggerInvestigation(targetLocation);
        }
        else
        {
            Debug.LogWarning("Nenhum policial disponível para responder ao chamado!");
        }
    }
}