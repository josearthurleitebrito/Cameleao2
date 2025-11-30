using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoliceMovement : MonoBehaviour
{
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
    private int previousPointIndex = -99; 
    private bool isWaiting = false;
    private float currentMovementSpeed;
    
    private Vector3 targetPosition; 
    private Transform currentTargetNode; // Para liberar a reserva depois

    [HideInInspector] public Vector2 CurrentDirection = Vector2.up; 
    private float _lastMoveX = 0f;
    private float _lastMoveY = 1f;

    [Header("Sistemas de IA")]
    [SerializeField] private NodePathfinder _pathfinder; 
    [SerializeField] private PathReservationManager _reservationManager; // NOVO: Gerente de Colisão

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
         if (patrolPoints != null && patrolPoints.Length > 0)
         {
            // Tenta reservar o ponto inicial para evitar spawn duplo
            if (_reservationManager != null) _reservationManager.TryReserveNode(patrolPoints[currentPointIndex]);
            currentTargetNode = patrolPoints[currentPointIndex];

            transform.position = patrolPoints[currentPointIndex].position;
            GoToNextPoint(false); 
         }
         else
         {
             Debug.LogWarning($"O Policial '{name}' não tem pontos de patrulha definidos!");
         }
    }

    void FixedUpdate()
    {
        if (targetPosition != Vector3.zero && targetPosition != transform.position && !isWaiting)
        {
            HandleMovement(currentMovementSpeed);
        }
        else
        {
            StopMovement();
        }
    }
    
    private void HandleMovement(float speed)
    {
        Vector3 direction3D = targetPosition - transform.position;
        Vector2 moveDirection2D = new Vector2(direction3D.x, direction3D.y).normalized;
        float distanceToTarget = direction3D.magnitude;
        
        if (distanceToTarget < 0.3f) 
        {
            _npcRigidbody2D.linearVelocity = Vector2.zero;
            UpdateAnimation(Vector2.zero);
            
            // Verifica se chegou ao Ponto de Patrulha
            if (patrolPoints.Length > 0 && Vector3.Distance(targetPosition, patrolPoints[currentPointIndex].position) < 0.1f)
            {
                if (!isWaiting)
                {
                    isWaiting = true;
                    StartCoroutine(WaitAndGoToNextPoint());
                }
            }
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
        if (_npcRigidbody2D != null) _npcRigidbody2D.linearVelocity = Vector2.zero;
        UpdateAnimation(Vector2.zero);
    }
    
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

    public void GoToNextPoint(bool randomize)
    {
        isWaiting = false;
        if (_pathfinder == null || _pathfinder.allWaypoints.Length == 0) return;

        // 1. Libera o nó anterior (Onde ele estava antes de começar a andar para o novo)
        // Nota: Policiais liberam o nó assim que SAEM dele, para manter o fluxo.
        if (_reservationManager != null && currentTargetNode != null)
        {
            _reservationManager.FreeNode(currentTargetNode);
        }

        int nodeToExcludeIndex = previousPointIndex; 
        List<Transform> neighborTransforms = _pathfinder.GetNeighbors(currentPointIndex);
        List<Transform> validTargets = new List<Transform>();
        
        // Filtra retorno imediato
        foreach (Transform neighbor in neighborTransforms)
        {
            int neighborIndex = System.Array.IndexOf(_pathfinder.allWaypoints, neighbor);
            if (neighborIndex != nodeToExcludeIndex)
            {
                validTargets.Add(neighbor);
            }
        }
        
        Transform nextTarget = null;

        // Se tiver opções válidas, tenta achar uma LIVRE
        if (validTargets.Count > 0)
        {
            // Embaralha para tentar aleatoriamente
            for (int i = 0; i < validTargets.Count; i++) {
                 Transform temp = validTargets[i];
                 int r = Random.Range(i, validTargets.Count);
                 validTargets[i] = validTargets[r];
                 validTargets[r] = temp;
            }

            foreach (var target in validTargets)
            {
                // Tenta reservar. Se conseguir, define como alvo.
                if (_reservationManager == null || _reservationManager.TryReserveNode(target))
                {
                    nextTarget = target;
                    break;
                }
            }
        }

        // Se não conseguiu nenhum alvo (todos ocupados ou sem vizinhos)
        if (nextTarget == null)
        {
            // Fica parado no nó atual (seguro) e tenta de novo em breve
            // Não libera o nó atual se for ficar nele!
            if (_reservationManager != null) _reservationManager.TryReserveNode(_pathfinder.allWaypoints[currentPointIndex]);
            
            currentTargetNode = _pathfinder.allWaypoints[currentPointIndex];
            nextTarget = currentTargetNode;
            
            // Força uma espera antes de tentar de novo para não travar o processamento
            StartCoroutine(WaitAndGoToNextPoint()); 
            return;
        }

        // Atualiza índices e define movimento
        int newPointIndex = System.Array.IndexOf(_pathfinder.allWaypoints, nextTarget);
        previousPointIndex = currentPointIndex; 
        currentPointIndex = newPointIndex; 
        currentTargetNode = nextTarget;

        SetTarget(nextTarget.position, patrolSpeed);
    }
    
    public void CancelPatrolWaiting()
    {
        StopAllCoroutines(); 
        isWaiting = false;
        // Se entrar em alerta, libera o nó de patrulha para outros não ficarem esperando
        if (_reservationManager != null && currentTargetNode != null)
             _reservationManager.FreeNode(currentTargetNode);
    }
    
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