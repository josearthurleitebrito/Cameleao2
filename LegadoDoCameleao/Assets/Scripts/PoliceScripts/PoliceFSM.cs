using UnityEngine;

using System.Collections;



// Enums

public enum PoliceState { Patrol, Alert, Chase }

public enum PoliceBehaviorType { Patrulheiro, GuardaParado }



public class PoliceFSM : MonoBehaviour

{

    [Header("Configuração de Comportamento")]

    [Tooltip("Patrulheiro: Anda e pega o player. \nGuardaParado: Fica fixo, inofensivo.")]

    public PoliceBehaviorType tipoDeComportamento = PoliceBehaviorType.Patrulheiro;



    [Tooltip("Se for Guarda Parado, para onde ele olha? (X, Y)")]

    public Vector2 direcaoDoOlhar = new Vector2(0, -1);



    [Header("Componentes Auxiliares")]

    [SerializeField] private PoliceMovement _movement;

    [SerializeField] private PoliceVision _vision;

   

    [Header("Configurações da FSM")]

    public PoliceState currentState = PoliceState.Patrol;

    public float chaseSpeed = 4f;

   

    [HideInInspector] public Vector3 alertPosition;

    private bool capturando = false;

    private bool investigandoAtivo = false;



    void Start()

    {

        if (_movement == null) _movement = GetComponent<PoliceMovement>();

        if (_vision == null) _vision = GetComponent<PoliceVision>();



        if (_movement == null || _vision == null)

        {

            Debug.LogError("Erro: Faltam componentes no Policial!");

            enabled = false;

            return;

        }



        if (tipoDeComportamento == PoliceBehaviorType.GuardaParado)

        {

            _vision.enabled = false;

            if (_vision.flashlight != null) _vision.flashlight.enabled = false;

            _movement.SetStaticDirection(direcaoDoOlhar);

            this.enabled = false;

        }

        else

        {

            SetState(PoliceState.Patrol);

        }

    }



    void Update()

    {

        if (capturando) return;



        // CRÍTICO: A visão tem que rodar sempre para detectar o jogador

        _vision.CheckForPlayer(transform.position, _movement.CurrentDirection, SetState);



        switch (currentState)

        {

            case PoliceState.Patrol:

                // Lógica de Patrulha é automática no PoliceMovement

                break;



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

        // Se ainda não mandamos o policial investigar, mandamos agora

        if (!investigandoAtivo)

        {

            investigandoAtivo = true;

            _movement.GoToAlertLocation(alertPosition);

        }

       

        // A lógica de "terminar investigação" é tratada dentro do PoliceMovement,

        // que volta automaticamente para a patrulha.

    }



    void HandleChase()

    {

        if (!capturando) StartCoroutine(SequenciaDeCaptura());

    }



    // --- MÉTODOS DE CONTROLE ---



    public void TriggerInvestigation(Vector3 targetPos)

    {

        if (currentState == PoliceState.Chase) return;



        Debug.Log($"{name}: Recebi chamado da câmera. Calculando rota!");

        alertPosition = targetPos;

        SetState(PoliceState.Alert);

    }



    IEnumerator SequenciaDeCaptura()

    {

        capturando = true;

        _movement.RealizarCaptura();

       

        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (player != null) player.SerCapturado();



        yield return new WaitForSeconds(3.0f);



        if (GameManager.instance != null) GameManager.instance.GameOver();

    }



    public void SetState(PoliceState newState)

    {

        if (currentState == newState || capturando) return;

        currentState = newState;

       

        if (newState == PoliceState.Alert)

        {

            _movement.CancelPatrolWaiting();

            investigandoAtivo = false; // Reseta para permitir nova investigação

        }

        else if (newState == PoliceState.Patrol)

        {

            _movement.GoToNextPoint(true);

        }

        else if (newState == PoliceState.Chase)

        {

            _movement.StopMovement();

        }

    }

}