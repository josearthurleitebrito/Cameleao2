using UnityEngine;
using System.Collections.Generic;

public class PathReservationManager : MonoBehaviour
{
    // Dicionário simples: O nó existe? Ele está ocupado (true) ou livre (false)?
    private Dictionary<Transform, bool> _nodeStatus = new Dictionary<Transform, bool>();

    public bool TryReserveNode(Transform node)
    {
        // Se o sistema nunca viu esse nó antes, adiciona ele à lista e já reserva
        if (!_nodeStatus.ContainsKey(node))
        {
            _nodeStatus.Add(node, true);
            return true;
        }

        // Se o nó já existe na lista e está marcado como LIVRE (false)
        if (!_nodeStatus[node])
        {
            _nodeStatus[node] = true; // Marca como OCUPADO
            return true; // Sucesso, pode ir
        }

        // Se chegou aqui, é porque está OCUPADO (true)
        return false; // Falha, procure outro caminho
    }

    public void FreeNode(Transform node)
    {
        // Só libera se o nó existir na lista
        if (node != null && _nodeStatus.ContainsKey(node))
        {
            _nodeStatus[node] = false; // Marca como LIVRE
        }
    }
}