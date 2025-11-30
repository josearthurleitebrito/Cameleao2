using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Configurações")]
    public float velocidade = 1.5f;
    public float tempoObservando = 3f;

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
            Debug.LogError("NPC precisa da referência ao PolicePathfinder!");
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
            // Teleporta para o inicio
            transform.position = salaAtual.transform.position;
            
            // Define o alvo atual como o nó onde nasci
            alvoAtual = salaAtual.transform; 

            // Tenta reservar onde estou (para ninguém vir em cima de mim)
            if (reservationManager != null) reservationManager.TryReserveNode(alvoAtual);

            if (mostrarDebug) Debug.Log($"<color=green>[START] {name} começou na sala: {salaAtual.name}</color>");
            
            // Começa o ciclo de decisão
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
        // Se já estou no alvo (distância pequena), não movo
        float distancia = Vector2.Distance(transform.position, alvoAtual.position);

        if (distancia < 0.2f) 
        {
            if(rb != null) rb.linearVelocity = Vector2.zero;
            AtualizarAnimacao(Vector2.zero);
            
            // Só chama a rotina de chegada se ainda não estiver esperando
            if (!esperando) StartCoroutine(AoChegarNoAlvo());
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
        
        // NOTA: Não liberamos o nó aqui! O nó só é liberado quando garantimos o PRÓXIMO.
        // Isso evita que o nó fique livre enquanto o NPC ainda está parado em cima dele pensando.

        if (estadoAtual == Estado.IndoParaCentro)
        {
            float tempoSala = (salaAtual != null) ? salaAtual.tempoDeEsperaCentro : 2f;
            yield return new WaitForSeconds(tempoSala);
            StartCoroutine(TomarDecisao());
        }
        else if (estadoAtual == Estado.IndoParaObra)
        {
            if (mostrarDebug) Debug.Log($"<color=blue>[NPC {name}]</color> Observando obra: {alvoAtual.name}...");
            
            ultimoFilhoVisitado = alvoAtual; 
            yield return new WaitForSeconds(tempoObservando);
            
            // Tenta voltar para o centro
            TentarIrPara(salaAtual.transform, Estado.VoltandoParaCentro, "Voltando para Centro");
        }
        else if (estadoAtual == Estado.VoltandoParaCentro)
        {
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

        // Tenta estratégia 1
        if (!querSair && temObras)
        {
            logDecisao += "Decidi VER OBRA. ";
            if (TentarEscolherObra(ref logDecisao)) decidiu = true;
            else logDecisao += "Mas candidatas ocupadas. ";
        }
        
        if (querSair && !decidiu)
        {
            logDecisao += "Decidi SAIR DA SALA. ";
            if (TentarMudarDeSala(ref logDecisao)) decidiu = true;
            else logDecisao += "Mas saídas ocupadas. ";
        }

        // Tenta estratégia 2 (Fallback)
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
            if (mostrarDebug) Debug.Log($"<color=orange>[NPC {name}]</color> {logDecisao} -> <color=red>Tudo bloqueado. Esperando...</color>");
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f)); 
            StartCoroutine(TomarDecisao());
        }
    }

    // Função unificada para tentar reservar e mover
    bool TentarIrPara(Transform destino, Estado novoEstado, string logExtra = "")
    {
        if (reservationManager != null)
        {
            if (reservationManager.TryReserveNode(destino))
            {
                // SUCESSO! Reservou o novo.
                // Agora sim, libera o antigo (onde estou parado agora).
                if (alvoAtual != null) reservationManager.FreeNode(alvoAtual);

                alvoAtual = destino;
                estadoAtual = novoEstado;
                esperando = false; // Começa a andar
                
                if (mostrarDebug && logExtra != "") Debug.Log($"<color=cyan>[NPC {name}]</color> {logExtra}");
                return true;
            }
            else
            {
                // FALHA! O destino está ocupado.
                // Se era pra voltar pro centro, preciso esperar liberar.
                if (novoEstado == Estado.VoltandoParaCentro)
                {
                    StartCoroutine(EsperarCentroLiberar(destino));
                }
                return false;
            }
        }
        
        // Se não tem manager, só vai.
        alvoAtual = destino;
        estadoAtual = novoEstado;
        esperando = false;
        return true;
    }

    bool TentarEscolherObra(ref string log)
    {
        if (salaAtual.filhos == null || salaAtual.filhos.Length == 0) return false;

        List<NodeChild> candidatas = new List<NodeChild>();
        foreach(var filho in salaAtual.filhos)
        {
            if (filho != null && filho.transform != ultimoFilhoVisitado)
                candidatas.Add(filho);
        }

        if (candidatas.Count == 0) return false;

        // Shuffle
        for (int i = 0; i < candidatas.Count; i++) {
             NodeChild temp = candidatas[i];
             int r = Random.Range(i, candidatas.Count);
             candidatas[i] = candidatas[r];
             candidatas[r] = temp;
        }

        foreach (var obra in candidatas)
        {
            // Usa a função unificada
            if (TentarIrPara(obra.transform, Estado.IndoParaObra))
            {
                log += $"<color=cyan>SUCESSO! Alvo: {obra.name} (Obra)</color>";
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
                candidatos.Add(scriptSala);
        }

        if (candidatos.Count == 0) // Beco sem saída, permite voltar
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
            // Usa a função unificada
            if (TentarIrPara(cand.transform, Estado.IndoParaCentro))
            {
                salaAnterior = salaAtual;
                salaAtual = cand; 
                ultimoFilhoVisitado = null; 
                log += $"<color=cyan>SUCESSO! Alvo: {cand.name} (Sala Vizinha)</color>";
                return true;
            }
        }
        return false;
    }

    IEnumerator EsperarCentroLiberar(Transform centro)
    {
        if (mostrarDebug) Debug.Log($"<color=orange>[NPC {name}]</color> Centro ocupado. Esperando para voltar...");
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        TentarIrPara(centro, Estado.VoltandoParaCentro, "Tentando voltar pro centro de novo...");
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