using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoliceMovement : MonoBehaviour
{
    private Rigidbody2D _npcRigidbody2D;
    private Animator _npcAnimator;

    [Header("Patrulha e Velocidade")]
    public float patrolSpeed = 2f; 
    public float chaseSpeed = 4f; 
    public float waitTimeAtPoint = 1.5f; 

    [Header("Configuração de Rota")]
    [Tooltip("Opcional: Arraste o objeto PAI dos waypoints aqui.")]
    public Transform patrolPathContainer; 
    
    // Deixe vazia se quiser usar o Pathfinder Global
    public List<Transform> patrolPoints = new List<Transform>(); 

    [Header("Animações e Lanterna")]
    public Transform _lanternLightTransform;
    public float _lanternRotationOffset = 90f; 

    // Variáveis Internas
    private int currentPointIndex = 0;
    private int previousPointIndex = -99; 
    private bool isWaiting = false;
    private float currentMovementSpeed;
    private Vector3 targetPosition; 
    private Transform currentTargetNode;

    private Queue<Transform> alertPathQueue = new Queue<Transform>();
    private bool isRespondingToAlert = false;

    [HideInInspector] public Vector2 CurrentDirection = Vector2.up; 
    private float _lastMoveX = 0f;
    private float _lastMoveY = 1f;

    [Header("Sistemas de IA")]
    [SerializeField] private NodePathfinder _pathfinder; 
    [SerializeField] private PathReservationManager _reservationManager;

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

        // Auto-Preenchimento (apenas se usar Container)
        if (patrolPathContainer != null)
        {
            patrolPoints.Clear();
            foreach (Transform child in patrolPathContainer)
            {
                if (child != patrolPathContainer) patrolPoints.Add(child);
            }
        }
    }

    void Start()
    {
         // MODO 1: Rota Fixa (Manual)
         if (patrolPoints != null && patrolPoints.Count > 0)
         {
            // Acha o ponto mais próximo da rota fixa para começar
            Transform pontoMaisPerto = GetClosestTransform(patrolPoints);
            currentPointIndex = patrolPoints.IndexOf(pontoMaisPerto);
            
            IniciarMovimento(patrolPoints[currentPointIndex]);
         }
         else
         {
             // MODO 2: Pathfinder Global (Vizinhos)
             if (_pathfinder != null && _pathfinder.allWaypoints.Length > 0)
             {
                 // CORREÇÃO: Encontra qual nó do mapa está mais perto de onde coloquei o policial
                 Transform startNode = GetClosestTransform(new List<Transform>(_pathfinder.allWaypoints));
                 
                 // Atualiza o índice para o código saber onde estou
                 currentPointIndex = _pathfinder.GetNodeIndex(startNode);
                 
                 // Debug para confirmar
                 Debug.Log($"Policial {name} iniciou no nó: {currentPointIndex} ({startNode.name})");

                 IniciarMovimento(startNode);
             }
             else
             {
                 Debug.LogWarning($"O Policial '{name}' não tem rota nem Pathfinder configurado!");
             }
         }
    }

    void IniciarMovimento(Transform startNode)
    {
        currentTargetNode = startNode;
        
        // Se o policial já nasce em cima do ponto, tenta reservar
        if (_reservationManager != null) _reservationManager.TryReserveNode(currentTargetNode);
        
        // Se ele estiver longe, anda até lá. Se estiver perto, já começa a patrulha.
        if (Vector3.Distance(transform.position, currentTargetNode.position) > 0.5f)
        {
             SetTarget(currentTargetNode.position, patrolSpeed);
        }
        else
        {
             transform.position = currentTargetNode.position; // Garante posição exata
             GoToNextPoint(false);
        }
    }

    // Helper para achar o nó mais próximo da posição inicial
    Transform GetClosestTransform(List<Transform> options)
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach(Transform potentialTarget in options)
        {
            Vector3 directionToTarget = potentialTarget.position - currentPos;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if(dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }
        return bestTarget;
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
    
    public void SetStaticDirection(Vector2 direction)
    {
        StopMovement(); 
        _lastMoveX = direction.x;
        _lastMoveY = direction.y;
        UpdateAnimation(Vector2.zero); 
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
            
            if (Vector3.Distance(targetPosition, currentTargetNode.position) < 0.1f)
            {
                if (!isWaiting)
                {
                    if (isRespondingToAlert) ProcessNextAlertNode();
                    else 
                    {
                        isWaiting = true;
                        StartCoroutine(WaitAndGoToNextPoint());
                    }
                }
            }
            return;
        }
        
        _npcRigidbody2D.MovePosition(_npcRigidbody2D.position + moveDirection2D * speed * Time.fixedDeltaTime);
        UpdateAnimation(moveDirection2D);
        CurrentDirection = moveDirection2D;
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

    // --- ALERTA ---
    public void GoToAlertLocation(Vector3 alertPos)
    {
        if (_pathfinder == null) return;

        Transform targetNode = GetClosestNode(alertPos);
        int targetIndex = _pathfinder.GetNodeIndex(targetNode);
        
        List<Transform> path = _pathfinder.FindPath(currentPointIndex, targetIndex);
        
        if (path != null && path.Count > 0)
        {
            isRespondingToAlert = true;
            isWaiting = false;
            StopAllCoroutines(); 

            alertPathQueue.Clear();
            foreach (Transform t in path) alertPathQueue.Enqueue(t);

            ProcessNextAlertNode();
        }
    }

    void ProcessNextAlertNode()
    {
        if (alertPathQueue.Count > 0)
        {
            Transform nextStep = alertPathQueue.Dequeue();
            
            if (_reservationManager != null && currentTargetNode != null)
                _reservationManager.FreeNode(currentTargetNode);

            if (_reservationManager != null) _reservationManager.TryReserveNode(nextStep);

            int nextIndex = _pathfinder.GetNodeIndex(nextStep);
            previousPointIndex = currentPointIndex;
            currentPointIndex = nextIndex;
            currentTargetNode = nextStep;

            SetTarget(nextStep.position, chaseSpeed);
        }
        else
        {
            isRespondingToAlert = false;
            isWaiting = true;
            StartCoroutine(WaitAndGoToNextPoint());
        }
    }

    Transform GetClosestNode(Vector3 pos)
    {
        float minDist = Mathf.Infinity;
        Transform closest = null;
        foreach(Transform t in _pathfinder.allWaypoints)
        {
            float d = Vector3.Distance(t.position, pos);
            if(d < minDist) { minDist = d; closest = t; }
        }
        return closest;
    }

    // --- PATRULHA ---
    public IEnumerator WaitAndGoToNextPoint() 
    {
        yield return new WaitForSeconds(waitTimeAtPoint);
        isWaiting = false; 
        GoToNextPoint(true); 
    }

    public void GoToNextPoint(bool randomize)
    {
        isRespondingToAlert = false; 
        isWaiting = false;
        
        // MODO 1: Rota Fixa
        if (patrolPoints != null && patrolPoints.Count > 0)
        {
            if (_reservationManager != null && currentTargetNode != null)
                 _reservationManager.FreeNode(currentTargetNode);

            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
            currentTargetNode = patrolPoints[currentPointIndex];
            
            if (_reservationManager != null) _reservationManager.TryReserveNode(currentTargetNode);
            SetTarget(currentTargetNode.position, patrolSpeed);
            return;
        }

        // MODO 2: Pathfinder Global
        if (_pathfinder == null || _pathfinder.allWaypoints.Length == 0) return;

        if (_reservationManager != null && currentTargetNode != null)
            _reservationManager.FreeNode(currentTargetNode);

        int nodeToExcludeIndex = previousPointIndex; 
        List<Transform> neighborTransforms = _pathfinder.GetNeighbors(currentPointIndex);
        List<Transform> validTargets = new List<Transform>();
        
        foreach (Transform neighbor in neighborTransforms)
        {
            int neighborIndex = _pathfinder.GetNodeIndex(neighbor);
            if (neighborIndex != nodeToExcludeIndex) validTargets.Add(neighbor);
        }
        
        Transform nextTarget = null;

        if (validTargets.Count > 0)
        {
            for (int i = 0; i < validTargets.Count; i++) {
                 Transform temp = validTargets[i];
                 int r = Random.Range(i, validTargets.Count);
                 validTargets[i] = validTargets[r];
                 validTargets[r] = temp;
            }

            foreach (var target in validTargets)
            {
                if (_reservationManager == null || _reservationManager.TryReserveNode(target))
                {
                    nextTarget = target;
                    break;
                }
            }
        }

        if (nextTarget == null)
        {
            if (_reservationManager != null) _reservationManager.TryReserveNode(_pathfinder.allWaypoints[currentPointIndex]);
            currentTargetNode = _pathfinder.allWaypoints[currentPointIndex];
            nextTarget = currentTargetNode;
            StartCoroutine(WaitAndGoToNextPoint()); 
            return;
        }

        int newPointIndex = _pathfinder.GetNodeIndex(nextTarget);
        previousPointIndex = currentPointIndex; 
        currentPointIndex = newPointIndex; 
        currentTargetNode = nextTarget;

        if (true) // Pode usar sua variável 'mostrarDebug' se tiver criado uma
        {
            Debug.Log($"<color=cyan>[POLICIAL {name}]</color> Escolhi ir para o Vizinho: <b>{nextTarget.name}</b> (Index: {newPointIndex})");
        }

        SetTarget(nextTarget.position, patrolSpeed);
    }
    
    public void CancelPatrolWaiting()
    {
        StopAllCoroutines(); 
        isWaiting = false;
        if (_reservationManager != null && currentTargetNode != null)
             _reservationManager.FreeNode(currentTargetNode);
    }
    
    void UpdateAnimation(Vector2 moveDirection)
    {
        if (_npcAnimator == null) return;
        if (moveDirection.sqrMagnitude < 0.01f) {
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

    public void RealizarCaptura()
    {
        StopMovement();
        StopAllCoroutines(); 
        this.enabled = false; 
        if (_npcAnimator != null) _npcAnimator.SetInteger("Movimento", 2); 
    }
}