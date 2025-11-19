using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Adicionada para usar .Distinct()

public class PathReservationManager : MonoBehaviour
{
    // Key: Waypoint Transform; Value: bool (true = Ocupado / false = Livre)
    private Dictionary<Transform, bool> _nodeStatus = new Dictionary<Transform, bool>();

    // NOVO: Referência ao Scriptable Object (SO) para obter o mapa de nós
    [SerializeField] private NPCPathData _pathData; 

    void Start()
    {
        if (_pathData == null)
        {
            Debug.LogError("ReservationManager não tem referência ao NPC Path Data!");
            return;
        }

        // Coleta todos os Waypoints de forma unificada (Pais, Filhos e Saídas)
        List<Transform> allNodes = new List<Transform>();
        foreach (var room in _pathData.allRooms)
        {
            if (room.parentNode != null) allNodes.Add(room.parentNode.transform);
            
            // Adiciona todos os filhos e saídas, extraindo o Transform
            allNodes.AddRange(room.childNodes.Where(go => go != null).Select(go => go.transform));
            allNodes.AddRange(room.exitNodes.Where(go => go != null).Select(go => go.transform));
        }

        // Filtra duplicatas e inicializa os nós
        foreach (Transform waypoint in allNodes.Distinct())
        {
            if (waypoint != null && !_nodeStatus.ContainsKey(waypoint))
            {
                _nodeStatus.Add(waypoint, false);
            }
        }
        
        Debug.Log($"Reservation Manager inicializado. Total de Nós rastreados: {_nodeStatus.Count}");
    }

    /// <summary> Tenta reservar um nó (Waypoint). </summary>
    public bool TryReserveNode(Transform node)
    {
        // Se o nó não for rastreado (não está no SO), assume-se que é um erro ou não rastreável.
        if (!_nodeStatus.ContainsKey(node)) 
        {
            // Opcional: Avisar se um nó não mapeado está sendo acessado
            // Debug.LogWarning($"Tentativa de reservar nó não mapeado: {node.name}");
            return false;
        }

        if (!_nodeStatus[node])
        {
            _nodeStatus[node] = true; 
            return true;
        }
        return false;
    }

    /// <summary> Libera um nó. Deve ser chamado quando o NPC chega ao alvo. </summary>
    public void FreeNode(Transform node)
    {
        if (_nodeStatus.ContainsKey(node))
        {
            _nodeStatus[node] = false; 
        }
    }

    /// <summary> Verifica se um nó está reservado. </summary>
    public bool IsNodeReserved(Transform node)
    {
        return _nodeStatus.ContainsKey(node) && _nodeStatus[node];
    }
}