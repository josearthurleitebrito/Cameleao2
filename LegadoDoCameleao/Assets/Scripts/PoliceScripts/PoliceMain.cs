using UnityEngine;
using System; 

public enum PoliceState { Patrol, Alert, Chase }

public class PoliceFSM : MonoBehaviour
{
    // --- Referências de Componentes ---
    [Header("Componentes Auxiliares")]
    [SerializeField] private PoliceMovement _movement;
    [SerializeField] private PoliceVision _vision;
    
    [Header("Configurações da FSM")]
    public PoliceState currentState = PoliceState.Patrol;
    public float chaseSpeed = 4f; 
    
    [HideInInspector] public Vector3 alertPosition; 

    void Start()
    {
        if (_movement == null || _vision == null)
        {
            Debug.LogError("O FSM do Policial precisa de referências para Movement e Vision!");
            enabled = false;
            return;
        }
        
        SetState(PoliceState.Patrol); 
    }

    void Update()
    {
        _vision.CheckForPlayer(transform.position, _movement.CurrentDirection, SetState);

        switch (currentState)
        {
            case PoliceState.Alert:
                HandleAlert();
                break;
            case PoliceState.Chase:
                HandleChase();
                break;
        }
    }

    // --- MÉTODOS DE ESTADO ---

    void HandleAlert()
    {
        _movement.SetTarget(alertPosition, chaseSpeed);
        
        if (Vector3.Distance(transform.position, alertPosition) < 0.1f)
        {
            SetState(PoliceState.Patrol);
        }
    }

    void HandleChase()
    {
        Debug.Log("GAME OVER: Policial te viu!");
        Time.timeScale = 0; 
    }

    // --- MÉTODOS DE CONTROLE ---

    public void SetState(PoliceState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        
        if (newState == PoliceState.Alert)
        {
            _movement.CancelPatrolWaiting();
        }
        else if (newState == PoliceState.Patrol)
        {
            _movement.GoToNextPoint(true);
        }
        
        if (newState == PoliceState.Chase)
        {
            _movement.StopMovement();
        }
    }
}