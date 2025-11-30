using UnityEngine;
using System.Collections.Generic;

public class PathReservationManager : MonoBehaviour
{
    // Dicionário dinâmico. Se o nó não existe, criamos na hora.
    private Dictionary<Transform, bool> _nodeStatus = new Dictionary<Transform, bool>();

    // Não precisa mais de referências complexas no Start

    public bool TryReserveNode(Transform node)
    {
        if (!_nodeStatus.ContainsKey(node))
        {
            // Se é a primeira vez que vemos esse nó, adicionamos como Ocupado (pois estamos reservando agora)
            _nodeStatus.Add(node, true);
            return true;
        }

        // Se já existe e está false (livre), reservamos
        if (!_nodeStatus[node])
        {
            _nodeStatus[node] = true;
            return true;
        }

        // Se está true (ocupado), falha
        return false;
    }

    public void FreeNode(Transform node)
    {
        if (_nodeStatus.ContainsKey(node))
        {
            _nodeStatus[node] = false;
        }
    }
    
    public bool IsNodeReserved(Transform node)
    {
        return _nodeStatus.ContainsKey(node) && _nodeStatus[node];
    }
}