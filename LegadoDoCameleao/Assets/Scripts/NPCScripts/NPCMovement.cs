using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Adicionada para usar .TrueForAll

public class NPCMovement : MonoBehaviour
{
    // --- Componentes ---
    private Rigidbody2D _npcRigidbody2D;
    
    [Header("Configurações do NPC")]
    public float npcSpeed = 1.2f; 
    public float waitTimeAtPoint = 3.0f;
    public float retryTime = 0.5f; 

    [Header("Referências de Sistema")]
    [SerializeField] private NPCPathData _pathData; 
    [SerializeField] private PathReservationManager _reservationManager; 

    // --- Variáveis de Rota ---
    private Transform currentNode; 
    private Transform previousNode; 
    private bool isMoving = false;

    void Awake()
    {
        _npcRigidbody2D = GetComponent<Rigidbody2D>();
        if (_npcRigidbody2D != null)
        {
            _npcRigidbody2D.gravityScale = 0f;
            _npcRigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation; 
        }
    }

    void Start()
    {
        if (_pathData == null || _reservationManager == null) 
        { 
            Debug.LogError("NPC precisa de Path Data e Reservation Manager!"); 
            return; 
        }
        
        // CORREÇÃO CRÍTICA: Define o primeiro currentNode (o nó Pai inicial da sala).
        if (_pathData.allRooms.Count > 0 && _pathData.allRooms[0].parentNode != null)
        {
            currentNode = _pathData.allRooms[0].parentNode.transform; 
            transform.position = currentNode.position;
            StartCoroutine(WaitAndSelectNextNode());
        }
        else
        {
            Debug.LogError("NPCPathData não tem um nó inicial válido configurado!");
            return;
        }
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            HandleMovement();
        }
        else
        {
            StopMovement();
        }
    }
    
    // --- LÓGICA DE MOVIMENTO E CHEGADA ---
    
    private void HandleMovement()
    {
        Vector3 direction3D = currentNode.position - transform.position;
        Vector2 moveDirection2D = new Vector2(direction3D.x, direction3D.y).normalized;
        float distanceToTarget = direction3D.magnitude;
        
        if (distanceToTarget < 0.3f) 
        {
            StopMovement();
            StartCoroutine(WaitAndSelectNextNode()); 
            return;
        }
        
        _npcRigidbody2D.MovePosition(_npcRigidbody2D.position + moveDirection2D * npcSpeed * Time.fixedDeltaTime);
        // [ADICIONAR: Atualização de Animação e Direção (como no PoliceMovement)]
    }

    public void StopMovement()
    {
        _npcRigidbody2D.linearVelocity = Vector2.zero;
        isMoving = false;
    }

    // --- LÓGICA DE DECISÃO HIERÁRQUICA ---

    private IEnumerator WaitAndSelectNextNode() 
    {
        // 1. Libera a reserva do nó que o NPC acabou de sair
        if (previousNode != null)
        {
             _reservationManager.FreeNode(previousNode);
        }
        
        // 2. Simula o tempo de observação/parada
        yield return new WaitForSeconds(waitTimeAtPoint);
        
        // 3. Inicia a tentativa de seleção do próximo nó
        StartCoroutine(TrySelectNextNode());
    }
    
    private IEnumerator TrySelectNextNode()
    {
        Transform nextNode = null;
        List<Transform> potentialTargets = new List<Transform>();
        
        // Define o nó de onde o NPC está saindo como o nó anterior
        previousNode = currentNode; 

        // --- LÓGICA 1: Retorno Prioritário (Filho -> Pai) ---
        Transform parentToReturn = _pathData.GetParentNode(currentNode);
        
        if (parentToReturn != null)
        {
            // Se está em um Nó Filho, força o retorno ao Nó Pai (Central da Sala)
            if (_reservationManager.TryReserveNode(parentToReturn))
            {
                nextNode = parentToReturn;
            }
        }
        else 
        {
            // --- LÓGICA 2: Decisão no Nó Pai (Explorar Filhos ou Sair) ---
            
            // 2a: Prioriza a exploração de Filhos (Obras de Arte)
            potentialTargets.AddRange(_pathData.GetNodesForExploration(currentNode));
            
            // 2b: Se não há Filhos válidos, busca Saídas (Outros Pais)
            // CRÍTICO: Se a lista de filhos estiver vazia OU se todos os filhos estiverem reservados, busca saída.
            if (potentialTargets.Count == 0 || potentialTargets.All(_reservationManager.IsNodeReserved)) 
            {
                // Limpa a lista para focar nas saídas, se necessário
                potentialTargets.Clear(); 
                potentialTargets.AddRange(_pathData.GetExitNodes(currentNode));
            }

            // --- Filtragem de Não-Retorno ---
            List<Transform> filteredTargets = potentialTargets.FindAll(t => t != previousNode);
            
            // --- Tentativa de Reserva e Movimento ---
            filteredTargets.Shuffle(); 
            
            foreach (Transform target in filteredTargets)
            {
                if (_reservationManager.TryReserveNode(target))
                {
                    nextNode = target;
                    break;
                }
            }
        }

        if (nextNode != null)
        {
            currentNode = nextNode; 
            isMoving = true;
        }
        else
        {
            // Evasão Passiva: Todos os alvos válidos estão ocupados. Tenta novamente.
            yield return new WaitForSeconds(retryTime);
            StartCoroutine(TrySelectNextNode());
        }
    }
}