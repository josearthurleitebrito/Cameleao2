using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoliceMovement : MonoBehaviour
{
    // --- Componentes ---
    private Rigidbody2D _npcRigidbody2D;
    private Animator _npcAnimator;

    [Header("Patrulha e Velocidade")]
    public float patrolSpeed = 2f; 
    [Tooltip("Pontos que o policial deve seguir na patrulha (Waypoints).")]
    public Transform[] patrolPoints; 
    public float waitTimeAtPoint = 1.5f; 

    [Header("Animações e Lanterna")]
    public Transform _lanternLightTransform;
    public float _lanternRotationOffset = 90f; 

    // --- Variáveis Internas ---
    private int currentPointIndex = 0;
    private int previousPointIndex = -1; // Armazena o índice do nó anterior ao atual (usado para filtro)
    private bool isWaiting = false;
    private float currentMovementSpeed;
    
    private Vector3 targetPosition; 

    [HideInInspector] public Vector2 CurrentDirection = Vector2.up; 
    private float _lastMoveX = 0f;
    private float _lastMoveY = 1f;

    [Header("Componentes Auxiliares do Grafo")]
    [SerializeField] private PolicePathfinder _pathfinder; 

    void Awake()
    {
        _npcRigidbody2D = GetComponent<Rigidbody2D>();
        _npcAnimator = GetComponent<Animator>();

        if (_npcRigidbody2D != null)
        {
            _npcRigidbody2D.interpolation = RigidbodyInterpolation2D.Extrapolate;
            _npcRigidbody2D.gravityScale = 0f;
            _npcRigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation; 
        }
    }

    void Start()
    {
         if (patrolPoints.Length > 0)
         {
            transform.position = patrolPoints[currentPointIndex].position;
            
            // Define o primeiro alvo. O nó atual (0) se tornará o nó anterior na PRÓXIMA chamada.
            GoToNextPoint(false); 
         }
         else
         {
             Debug.LogWarning("O Policial não tem pontos de patrulha definidos!");
         }
    }

    void FixedUpdate()
    {
        if (targetPosition != transform.position && !isWaiting)
        {
            HandleMovement(currentMovementSpeed);
        }
        else
        {
            StopMovement();
        }
    }
    
    // --- LÓGICA DE MOVIMENTO E FÍSICA ---
    
    private void HandleMovement(float speed)
    {
        Vector3 direction3D = targetPosition - transform.position;
        Vector2 moveDirection2D = new Vector2(direction3D.x, direction3D.y).normalized;
        float distanceToTarget = direction3D.magnitude;
        
        // Se atingiu o alvo dentro da tolerância (0.3f)
        if (distanceToTarget < 0.3f) 
        {
            _npcRigidbody2D.linearVelocity = Vector2.zero;
            UpdateAnimation(Vector2.zero);
            
            // Se o alvo é um PONTO DE PATRULHA
            if (Vector3.Distance(targetPosition, patrolPoints[currentPointIndex].position) < 0.01f)
            {
                // Inicia a espera APENAS se não estiver esperando (evita loop)
                if (!isWaiting)
                {
                    isWaiting = true;
                    StartCoroutine(WaitAndGoToNextPoint());
                }
            }
            // CRÍTICO: Retorna para garantir que o FixedUpdate pare o movimento neste frame.
            return;
        }
        else
        {
            _npcRigidbody2D.MovePosition(_npcRigidbody2D.position + moveDirection2D * speed * Time.fixedDeltaTime);
            
            UpdateAnimation(moveDirection2D);
            CurrentDirection = moveDirection2D;
        }
    }

    public void StopMovement()
    {
        _npcRigidbody2D.linearVelocity = Vector2.zero;
        UpdateAnimation(Vector2.zero);
    }
    
    // --- LÓGICA DE PATRULHA E COROUTINE ---

    public void SetTarget(Vector3 newTarget, float speed)
    {
        targetPosition = newTarget;
        currentMovementSpeed = speed;
        isWaiting = false;
    }

    public IEnumerator WaitAndGoToNextPoint() 
    {
        yield return new WaitForSeconds(waitTimeAtPoint);
        isWaiting = false; 
        GoToNextPoint(true); 
    }

    // Calcula e define o próximo ponto de patrulha
    public void GoToNextPoint(bool randomize)
    {
        isWaiting = false;
        if (_pathfinder == null || _pathfinder.allWaypoints.Length == 0) return;

        // CRÍTICO: O nó a ser EXCLUÍDO no filtro é o nó anterior ao atual.
        // Se previousPointIndex for -1 (Start), ele não exclui nada.
        int nodeToExcludeIndex = previousPointIndex; 

        // Obtém vizinhos do ponto onde ele está (currentPointIndex)
        List<Transform> neighborTransforms = _pathfinder.GetNeighbors(currentPointIndex);
        
        // --- 2. Filtra o Retorno Imediato ---
        List<Transform> validTargets = new List<Transform>();
        
        foreach (Transform neighbor in neighborTransforms)
        {
            int neighborIndex = System.Array.IndexOf(_pathfinder.allWaypoints, neighbor);

            // Regra: Se o índice do vizinho é igual ao índice que ele veio (previousPointIndex), ignore.
            if (neighborIndex != nodeToExcludeIndex)
            {
                validTargets.Add(neighbor);
            }
        }
        
        // --- 3. Seleção Aleatória do Alvo ---
        Transform nextTarget;

        if (validTargets.Count > 0)
        {
            nextTarget = validTargets[UnityEngine.Random.Range(0, validTargets.Count)];
        }
        else
        {
            // Se não houver vizinhos válidos, permanece no ponto atual
            nextTarget = _pathfinder.allWaypoints[currentPointIndex]; 
            StartCoroutine(WaitAndGoToNextPoint()); 
            return; 
        }

        // --- 4. Atualiza os Índices e o Target ---
        int newPointIndex = System.Array.IndexOf(_pathfinder.allWaypoints, nextTarget);
        
        // CRÍTICO: O nó que ele acabou de sair (currentPointIndex) se torna o nó anterior.
        previousPointIndex = currentPointIndex; 
        
        // O nó atual se torna o novo alvo
        currentPointIndex = newPointIndex; 

        Debug.Log($"Policial no Nó {previousPointIndex} escolheu ir para o Nó {currentPointIndex} ({nextTarget.name}).");

        SetTarget(nextTarget.position, patrolSpeed);
    }
    
    public void CancelPatrolWaiting()
    {
        StopAllCoroutines(); 
        isWaiting = false;
    }
    
    // --- LÓGICA DE ANIMAÇÃO E LANTERNA ---
    
    void UpdateAnimation(Vector2 moveDirection)
    {
        if (_npcAnimator == null) return;

        if (moveDirection.sqrMagnitude < 0.01f) 
        {
            _npcAnimator.SetInteger("Movimento", 0); 
            _npcAnimator.SetFloat("LastMoveX", _lastMoveX);
            _npcAnimator.SetFloat("LastMoveY", _lastMoveY);
            RotateLanternToDirection(new Vector2(_lastMoveX, _lastMoveY));
            return;
        }
        
        _npcAnimator.SetInteger("Movimento", 1); 
        _npcAnimator.SetFloat("AxisX", moveDirection.x);
        _npcAnimator.SetFloat("AxisY", moveDirection.y);
        
        _lastMoveX = moveDirection.x;
        _lastMoveY = moveDirection.y;
        
        RotateLanternToDirection(moveDirection);
    }

    void RotateLanternToDirection(Vector2 direction)
    {
        if (_lanternLightTransform != null && direction.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            _lanternLightTransform.rotation = Quaternion.Euler(0, 0, angle + _lanternRotationOffset);
        }
    }
}