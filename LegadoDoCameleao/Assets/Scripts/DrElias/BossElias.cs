using UnityEngine;
using System.Collections;

public class BossElias : MonoBehaviour
{
    [Header("Interação")]
    public KeyCode teclaInteracao = KeyCode.E;
    
    [Header("Diálogo Final")]
    [TextArea] 
    public string falaDaPrisao = "Acabou, Doutor. As luzes se apagaram para o seu show. A polícia já cercou o prédio.";

    // Variáveis Privadas
    private bool playerPerto = false;
    private bool sequenciaFinalIniciada = false;
    private DialogueUI2D dialogueUI;
    private Animator _animator; // Referência ao Animator do Elias

    void Start()
    {
        // Busca a UI de forma robusta
        dialogueUI = FindFirstObjectByType<DialogueUI2D>();
        
        // Busca o Animator no próprio objeto
        _animator = GetComponent<Animator>();

        if (dialogueUI == null) 
            Debug.LogError("BossElias não encontrou o DialogueUI2D na cena! O diálogo final não vai aparecer.");

        // Garante que ele comece em Idle (Movimento = 0)
        if (_animator != null)
        {
            _animator.SetInteger("Movimento", 0);
        }
    }

    void Update()
    {
        // 1. Se a sequência começou, só monitora o fechamento
        if (sequenciaFinalIniciada)
        {
            // Se o painel fechou (player terminou de ler), ganha o jogo
            if (dialogueUI != null && !dialogueUI.panel.activeSelf)
            {
                FinalizarJogo();
            }
            return;
        }

        // 2. Interação normal
        if (playerPerto && Input.GetKeyDown(teclaInteracao))
        {
            Confrontar();
        }
    }

    void Confrontar()
    {
        if (GameManager.instance == null) return;

        if (GameManager.instance.luzesApagadas)
        {
            // WIN CONDITION: Luzes apagadas

            // --- LÓGICA DE ANIMAÇÃO ---
            // Toca a animação de 'Action' (susto/rendição) antes de abrir o diálogo
            if (_animator != null)
            {
                _animator.SetInteger("Movimento", 2); 
            }
            // --------------------------

            if (dialogueUI != null)
            {
                // Abre o diálogo e trava o script
                dialogueUI.ShowDialogue("Camaleão", falaDaPrisao, null);
                
                // Inicia uma coroutine para ativar a flag com segurança
                StartCoroutine(IniciarEsperaDoDialogo());
            }
            else
            {
                // Fallback se a UI estiver quebrada
                FinalizarJogo();
            }
        }
        else
        {
            // LOSE CONDITION: Luzes acesas
            // Elias continua em Idle (0) pois ele não foi pego, ele que te pegou!
            Debug.Log("Elias te viu! Game Over.");
            GameManager.instance.GameOver();
        }
    }

    IEnumerator IniciarEsperaDoDialogo()
    {
        // Espera 1 frame para garantir que o painel abriu visualmente antes de checar se fechou
        yield return null; 
        sequenciaFinalIniciada = true;
    }

    void FinalizarJogo()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.Vitoria();
        }
        this.enabled = false; 
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerPerto = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerPerto = false;
    }
}