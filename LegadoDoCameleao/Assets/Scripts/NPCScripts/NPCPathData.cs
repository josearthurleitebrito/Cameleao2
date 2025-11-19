using UnityEngine;
using System.Collections.Generic;
using System.Linq; 

// Permite criar o Asset no menu Project -> Create -> AI -> NPC Path Data
[CreateAssetMenu(fileName = "NPCPathData", menuName = "AI/NPC Path Data")]
public class NPCPathData : ScriptableObject
{
    [System.Serializable]
    public class RoomNode
    {
        [Tooltip("O Waypoint Central da Sala (Nó Pai).")]
        public Transform parentNode; // CORRIGIDO: Tipo Transform

        [Tooltip("Lista de Waypoints de Interesse dentro desta sala (Nós Filhos).")]
        public List<Transform> childNodes = new List<Transform>(); // CORRIGIDO: Tipo List<Transform>
        
        [Tooltip("Lista de Waypoints para SAIR desta sala (Nós Vizinhos/Pais).")]
        public List<Transform> exitNodes = new List<Transform>(); // CORRIGIDO: Tipo List<Transform>
    }

    [Tooltip("Lista de todas as salas e suas conexões. Configure aqui!")]
    public List<RoomNode> allRooms = new List<RoomNode>();

    // --- MÉTODOS DE BUSCA ---

    /// <summary>
    /// Retorna os Waypoints de interesse (Nós Filhos) da sala atual.
    /// </summary>
    public List<Transform> GetNodesForExploration(Transform currentNodeTransform)
    {
        foreach (var room in allRooms)
        {
            // CORREÇÃO: Compara diretamente Transforms.
            if (room.parentNode == currentNodeTransform)
            {
                // CRÍTICO: Retorna a lista de Transform diretamente, sem conversão.
                return room.childNodes; 
            }
        }
        return new List<Transform>();
    }

    /// <summary>
    /// Retorna os Waypoints para transição (Nós Vizinhos/Pais) da sala atual.
    /// </summary>
    public List<Transform> GetExitNodes(Transform currentNodeTransform)
    {
        foreach (var room in allRooms)
        {
            // CORREÇÃO: Compara diretamente Transforms.
            if (room.parentNode == currentNodeTransform)
            {
                // CRÍTICO: Retorna a lista de Transform diretamente, sem conversão.
                return room.exitNodes;
            }
        }
        return new List<Transform>();
    }
    
    /// <summary>
    /// Retorna o Nó Pai associado a um Nó Filho (para retorno prioritário).
    /// </summary>
    public Transform GetParentNode(Transform childNodeTransform)
    {
        foreach (var room in allRooms)
        {
            // CORREÇÃO: Usa .Contains() simples para verificar se o Transform está na lista.
            if (room.childNodes.Contains(childNodeTransform))
            {
                // Retorna o Transform do Nó Pai.
                return room.parentNode; 
            }
        }
        return null;
    }
    
    /// <summary>
    /// Verifica se o nó fornecido é um Nó Filho.
    /// </summary>
    public bool IsChildNode(Transform node)
    {
        foreach (var room in allRooms)
        {
            // CORREÇÃO: Usa .Contains() simples.
            if (room.childNodes.Contains(node))
            {
                return true;
            }
        }
        return false;
    }
}