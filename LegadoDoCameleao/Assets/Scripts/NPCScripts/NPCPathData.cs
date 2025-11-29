using UnityEngine;
using System.Collections.Generic;

// Permite criar o Asset no menu Project -> Create -> AI -> NPC Path Data
[CreateAssetMenu(fileName = "NPCPathData", menuName = "AI/NPC Path Data")]
public class NPCPathData : ScriptableObject
{
    [System.Serializable]
    public class RoomNode
    {
        [Tooltip("O Waypoint Central da Sala (Nó Pai).")]
        public Transform parentNode;

        [Tooltip("Lista de Waypoints de Interesse dentro desta sala (Nós Filhos).")]
        public List<Transform> childNodes = new List<Transform>();
        
        [Tooltip("Lista de Waypoints para SAIR desta sala (Nós Vizinhos/Pais).")]
        public List<Transform> exitNodes = new List<Transform>();
    }

    [Tooltip("Lista de todas as salas e suas conexões. Configure aqui!")]
    public List<RoomNode> allRooms = new List<RoomNode>();

    // --- MÉTODOS DE BUSCA ---

    public List<Transform> GetNodesForExploration(Transform currentNodeTransform)
    {
        foreach (var room in allRooms)
        {
            if (room.parentNode == currentNodeTransform)
            {
                return room.childNodes;
            }
        }
        return new List<Transform>();
    }

    public List<Transform> GetExitNodes(Transform currentNodeTransform)
    {
        foreach (var room in allRooms)
        {
            if (room.parentNode == currentNodeTransform)
            {
                return room.exitNodes;
            }
        }
        return new List<Transform>();
    }
    
    public Transform GetParentNode(Transform childNodeTransform)
    {
        foreach (var room in allRooms)
        {
            if (room.childNodes.Contains(childNodeTransform))
            {
                return room.parentNode; 
            }
        }
        return null;
    }
}