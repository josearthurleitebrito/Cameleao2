using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NodePathfinder : MonoBehaviour
{
    private Dictionary<int, List<int>> _adjacencyList = new Dictionary<int, List<int>>();
    
    [Tooltip("Arraste TODOS os 9 Waypoints para este array na ordem de 0 a 8.")]
    public Transform[] allWaypoints; 

    void Awake()
    {
        InitializeGraph();
    }

    private void InitializeGraph()
    {
        if (allWaypoints.Length != 9)
        {
            Debug.LogError("O Grafo 3x3 requer exatamente 9 Waypoints!");
            return;
        }

        // Grafo 3x3 (Vizinhos sem diagonal)
        _adjacencyList.Add(0, new List<int> { 1, 3 });
        _adjacencyList.Add(1, new List<int> { 0, 2, 4 });
        _adjacencyList.Add(2, new List<int> { 1, 5 });
        _adjacencyList.Add(3, new List<int> { 0, 4, 6 });
        _adjacencyList.Add(4, new List<int> { 1, 3, 5, 7 });
        _adjacencyList.Add(5, new List<int> { 2, 4, 8 });
        _adjacencyList.Add(6, new List<int> { 3, 7 });
        _adjacencyList.Add(7, new List<int> { 4, 6, 8 });
        _adjacencyList.Add(8, new List<int> { 5, 7 });
    }

    public List<Transform> GetNeighbors(int currentWaypointIndex)
    {
        if (!_adjacencyList.ContainsKey(currentWaypointIndex)) return new List<Transform>();

        List<Transform> neighborTransforms = new List<Transform>();
        foreach (int index in _adjacencyList[currentWaypointIndex])
        {
            neighborTransforms.Add(allWaypoints[index]);
        }
        return neighborTransforms;
    }

    // --- NOVA FUNÇÃO: Encontrar Caminho (BFS) ---
    public List<Transform> FindPath(int startIndex, int targetIndex)
    {
        if (startIndex == targetIndex) return new List<Transform> { allWaypoints[targetIndex] };

        // Filas para o algoritmo de busca
        Queue<int> queue = new Queue<int>();
        Dictionary<int, int> cameFrom = new Dictionary<int, int>(); // Para reconstruir o caminho
        
        queue.Enqueue(startIndex);
        cameFrom[startIndex] = -1; // -1 significa início

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();

            if (current == targetIndex) break; // Chegou!

            foreach (int neighbor in _adjacencyList[current])
            {
                if (!cameFrom.ContainsKey(neighbor))
                {
                    queue.Enqueue(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        // Reconstroi o caminho de trás para frente
        if (!cameFrom.ContainsKey(targetIndex)) return null; // Caminho não encontrado

        List<Transform> path = new List<Transform>();
        int curr = targetIndex;
        
        while (curr != -1)
        {
            path.Add(allWaypoints[curr]);
            curr = cameFrom[curr];
        }

        path.Reverse(); // Inverte para ficar na ordem certa (Inicio -> Fim)
        path.RemoveAt(0); // Remove o ponto onde já estou
        return path;
    }

    // Acha o índice de um Transform na lista
    public int GetNodeIndex(Transform node)
    {
        return System.Array.IndexOf(allWaypoints, node);
    }
}