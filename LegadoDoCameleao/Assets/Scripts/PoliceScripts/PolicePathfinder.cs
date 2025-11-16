using UnityEngine;
using System.Collections.Generic;

public class PolicePathfinder : MonoBehaviour
{
    // Mapeamento: Key = Índice do Waypoint; Value = Lista de Índices dos Waypoints Vizinhos VÁLIDOS (sem diagonal)
    // Exemplo: 0 -> 1 e 3 (Baixo Central e Meio Esquerdo)
    private Dictionary<int, List<int>> _adjacencyList = new Dictionary<int, List<int>>();
    
    // Lista de Referência de TODOS os Waypoints (Configurada no Inspector)
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

        // --- CONSTRUÇÃO DO GRAFO 3x3 (Regra: Apenas adjacentes, sem diagonal) ---
        // Indices: 6 7 8
        //          3 4 5
        //          0 1 2
        
        // Exemplo: O nó 4 (centro) está ligado a 1, 3, 5, 7.
        // O dicionário armazena: Índice do Nó -> Lista de Vizinhos Válidos
        
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

    /// <summary>
    /// Retorna os objetos Transform dos Waypoints vizinhos do nó atual.
    /// </summary>
    public List<Transform> GetNeighbors(int currentWaypointIndex)
    {
        if (!_adjacencyList.ContainsKey(currentWaypointIndex))
        {
            Debug.LogError("Índice de Waypoint fora do Grafo: " + currentWaypointIndex);
            return new List<Transform>();
        }

        List<Transform> neighborTransforms = new List<Transform>();
        List<int> neighborIndices = _adjacencyList[currentWaypointIndex];

        foreach (int index in neighborIndices)
        {
            neighborTransforms.Add(allWaypoints[index]);
        }
        return neighborTransforms;
    }
}