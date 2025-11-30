using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Configurações")]
    public float velocidade = 1.5f;
    public float tempoObservandoObra = 4f; // Tempo fixo para olhar OBRAS (filhos)
    
    // REMOVI: public float waitTimeAtPoint; -> Agora usamos o tempo da SALA.

    [Header("Comportamento")]
    [Tooltip("0 = Nunca sai da sala (se tiver obras). 1 = Sempre muda de sala.")]
    [Range(0f, 1f)] 
    public float chanceDeMudarDeSala = 0.4f;

    [Header("Debug")]
    public bool mostrarDebug = true;

    [Header("Sistemas")]
    public NodePathfinder pathfinder; 
    public PathReservationManager reservationManager; 

    // Estado interno
    private Transform alvoAtual;
    private NodeParent salaAtual;
    private NodeParent salaAnterior; 
    private Transform ultimoFilhoVisitado; 
    private bool esperando = false;

    // Variáveis de Animação
    private float _lastMoveX = 0f;
    private float _lastMoveY = -1f;

    private enum Estado { IndoParaCentro, IndoParaObra, VoltandoParaCentro }
    private Estado estadoAtual = Estado.IndoParaCentro;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        if (pathfinder == null)
        {
            Debug.LogError("NPC precisa da referência ao NodePathfinder!");
            return;
        }

        StartCoroutine(InicioAtrasado());
    }

    IEnumerator InicioAtrasado()
    {
        yield return new WaitForSeconds(Random.Range(0.1f, 0.5f)); 
        
        Transform salaInicialTrans = pathfinder.allWaypoints[Random.Range(0, pathfinder.allWaypoints.Length)];
        salaAtual = salaInicialTrans.GetComponent<NodeParent>();

        if (salaAtual != null)
        {
            transform.position = salaAtual.transform.position;
            if (reservationManager != null) reservationManager.TryReserveNode(salaAtual.transform);

            if (mostrarDebug) Debug.Log($"<color=green>[START] {name} começou na sala: {salaAtual.name}</color>");
            
            StartCoroutine(TomarDecisao()); 
        }
    }

    void FixedUpdate()
    {
        if (alvoAtual != null && !esperando)
        {
            MoverParaAlvo();
        }
        else
        {
            if(rb != null) rb.linearVelocity = Vector2.zero;
            AtualizarAnimacao(Vector2.zero);
        }
    }

    void MoverParaAlvo()
    {
        float distancia = Vector2.Distance(transform.position, alvoAtual.position);

        if (distancia < 0.2f) 
        {
            if(rb != null) rb.linearVelocity = Vector2.zero;
            AtualizarAnimacao(Vector2.zero);
            StartCoroutine(AoChegarNoAlvo());
        }
        else
        {
            Vector2 direcao = (alvoAtual.position - transform.position).normalized;
            rb.MovePosition(rb.position + direcao * velocidade * Time.fixedDeltaTime);
            AtualizarAnimacao(direcao);
        }
    }

    IEnumerator AoChegarNoAlvo()
    {
        esperando = true;
        
        if (reservationManager != null) reservationManager.FreeNode(alvoAtual);

        if (estadoAtual == Estado.IndoParaCentro)
        {
            // AQUI ESTÁ A MUDANÇA: Usa o tempo configurado na PRÓPRIA SALA
            float tempoSala = (salaAtual != null) ? salaAtual.tempoDeEsperaCentro : 2f;
            yield return new WaitForSeconds(tempoSala);
            
            StartCoroutine(TomarDecisao());
        }
        else if (estadoAtual == Estado.IndoParaObra)
        {
            if (mostrarDebug) Debug.Log($"<color=blue>[NPC {name}]</color> Observando obra: {alvoAtual.name}...");
            
            ultimoFilhoVisitado = alvoAtual; 
            
            // Usa o tempo fixo para obras (pode customizar no NodeChild também se quiser)
            yield return new WaitForSeconds(tempoObservandoObra);
            
            DefinirAlvo(salaAtual.transform, Estado.VoltandoParaCentro, "Voltando para Centro");
        }
        else if (estadoAtual == Estado.VoltandoParaCentro)
        {
            // Volta rápida pro centro, espera pouquinho (0.5s) pra decidir
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(TomarDecisao());
        }
    }

    IEnumerator TomarDecisao()
    {
        string logDecisao = $"Estou no Centro ({salaAtual.name}). ";
        bool decidiu = false;

        bool temObras = salaAtual.filhos != null && salaAtual.filhos.Length > 0;
        bool querSair = Random.value < chanceDeMudarDeSala;

        if (!temObras) querSair = true; 

        if (!querSair && temObras)
        {
            logDecisao += "Decidi VER OBRA. ";
            if (TentarEscolherObra(ref logDecisao)) decidiu = true;
            else logDecisao += "Mas candidatas estavam ocupadas ou repetidas. ";
        }
        
        if (querSair && !decidiu)
        {
            logDecisao += "Decidi SAIR DA SALA. ";
            if (TentarMudarDeSala(ref logDecisao)) decidiu = true;
            else logDecisao += "Mas as saídas estavam ocupadas. ";
        }

        if (!decidiu)
        {
            logDecisao += "Trocando estratégia... ";
            if (querSair && temObras) 
            {
                if (TentarEscolherObra(ref logDecisao)) decidiu = true;
            }
            else if (!querSair) 
            {
                if (TentarMudarDeSala(ref logDecisao)) decidiu = true;
            }
        }

        if (decidiu)
        {
            if (mostrarDebug) Debug.Log($"<color=yellow>[NPC {name}]</color> {logDecisao}");
        }
        else
        {
            if (mostrarDebug) Debug.Log($"<color=orange>[NPC {name}]</color> {logDecisao} -> <color=red>FALHA: Tudo bloqueado. Esperando...</color>");
            yield return new WaitForSeconds(1f); 
            StartCoroutine(TomarDecisao());
        }
    }

    bool TentarEscolherObra(ref string log)
    {
        if (salaAtual.filhos == null || salaAtual.filhos.Length == 0) return false;

        List<NodeChild> candidatas = new List<NodeChild>();
        foreach(var filho in salaAtual.filhos)
        {
            if (filho != null && filho.transform != ultimoFilhoVisitado)
            {
                candidatas.Add(filho);
            }
        }

        if (candidatas.Count == 0) return false;

        for (int i = 0; i < candidatas.Count; i++) {
             NodeChild temp = candidatas[i];
             int r = Random.Range(i, candidatas.Count);
             candidatas[i] = candidatas[r];
             candidatas[r] = temp;
        }

        foreach (var obra in candidatas)
        {
            if (reservationManager == null || reservationManager.TryReserveNode(obra.transform))
            {
                log += $"<color=cyan>SUCESSO! Alvo: {obra.name} (Obra)</color>";
                DefinirAlvo(obra.transform, Estado.IndoParaObra, null);
                return true;
            }
        }
        
        return false;
    }

    bool TentarMudarDeSala(ref string log)
    {
        int indiceAtual = -1;
        for (int i = 0; i < pathfinder.allWaypoints.Length; i++)
        {
            if (pathfinder.allWaypoints[i] == salaAtual.transform)
            {
                indiceAtual = i;
                break;
            }
        }

        if (indiceAtual == -1) return false;

        List<Transform> vizinhos = pathfinder.GetNeighbors(indiceAtual);
        List<NodeParent> candidatos = new List<NodeParent>();

        foreach (Transform t in vizinhos)
        {
            NodeParent scriptSala = t.GetComponent<NodeParent>();
            if (scriptSala != null && scriptSala != salaAnterior)
            {
                candidatos.Add(scriptSala);
            }
        }

        if (candidatos.Count == 0)
        {
            foreach (Transform t in vizinhos)
            {
                NodeParent scriptSala = t.GetComponent<NodeParent>();
                if (scriptSala != null) candidatos.Add(scriptSala);
            }
        }

        for (int i = 0; i < candidatos.Count; i++) {
             NodeParent temp = candidatos[i];
             int r = Random.Range(i, candidatos.Count);
             candidatos[i] = candidatos[r];
             candidatos[r] = temp;
        }

        foreach (NodeParent cand in candidatos)
        {
            if (reservationManager == null || reservationManager.TryReserveNode(cand.transform))
            {
                salaAnterior = salaAtual;
                salaAtual = cand; 
                
                ultimoFilhoVisitado = null; 

                log += $"<color=cyan>SUCESSO! Alvo: {cand.name} (Sala Vizinha)</color>";
                DefinirAlvo(cand.transform, Estado.IndoParaCentro, null);
                return true;
            }
        }
        return false;
    }

    void DefinirAlvo(Transform novoAlvo, Estado novoEstado, string logExtra)
    {
        if (novoEstado == Estado.VoltandoParaCentro)
        {
            if (reservationManager != null && !reservationManager.TryReserveNode(novoAlvo))
            {
                StartCoroutine(EsperarCentroLiberar(novoAlvo));
                return;
            }
            if (mostrarDebug && logExtra != null) Debug.Log($"<color=yellow>[NPC {name}]</color> {logExtra}");
        }

        alvoAtual = novoAlvo;
        estadoAtual = novoEstado;
        esperando = false; 
    }

    IEnumerator EsperarCentroLiberar(Transform centro)
    {
        if (mostrarDebug) Debug.Log($"<color=orange>[NPC {name}]</color> Quero voltar pro centro, mas tá ocupado. Esperando...");
        yield return new WaitForSeconds(0.5f);
        DefinirAlvo(centro, Estado.VoltandoParaCentro, "Tentando voltar pro centro de novo...");
    }

    void AtualizarAnimacao(Vector2 dir)
    {
        if (anim == null) return;
        
        if (dir.magnitude > 0.1f)
        {
            anim.SetInteger("Movimento", 1);
            anim.SetFloat("AxisX", dir.x);
            anim.SetFloat("AxisY", dir.y);
            _lastMoveX = dir.x;
            _lastMoveY = dir.y;
        }
        else
        {
            anim.SetInteger("Movimento", 0);
            anim.SetFloat("LastMoveX", _lastMoveX);
            anim.SetFloat("LastMoveY", _lastMoveY);
        }
    }
}