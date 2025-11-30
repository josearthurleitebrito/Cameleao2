using UnityEngine;
using UnityEngine.SceneManagement;

public enum GamePhase { Fase1_Inspecao, Fase2_Puzzle, Fase3_Final }

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Interação")]
    public KeyCode teclaInteracao = KeyCode.E;

    [Header("Configuração Geral")]
    public GamePhase faseAtual;

    [Header("Telas e UI")]
    public GameObject painelIntro;    
    public float tempoDuracaoIntro = 3.0f;
    public GameObject painelPause;    
    public GameObject painelGameOver; 
    public GameObject painelVitoria;  

    [Header("Fase 1: Inspeção")]
    public int totalObrasParaInspecionar;
    private int obrasInspecionadas = 0;

    [Header("Mensagens de Conclusão (Fases 1 e 2)")]
    [TextArea] public string textoConclusaoFase1 = "Já tenho provas suficientes. Preciso sair daqui agora.";
    [TextArea] public string textoConclusaoFase2 = "Achei o erro na pintura! O Dr. Elias está no escritório. Preciso ir para a porta de acesso.";

    [Header("Fase 3: Narrativa")]
    [TextArea] public string textoInicioFase3 = "O leilão ilegal já começou. A segurança está máxima. Preciso encontrar a Caixa de Força.";
    [TextArea] public string textoAposApagarLuz = "Escuridão total! O sistema caiu. É agora! Tenho que render o Dr. Elias.";

    [Header("Diálogo Final")]
    [TextArea] 
    public string falaDaPrisao = "Acabou, Doutor. As luzes se apagaram para o seu show. A polícia já cercou o prédio e suas falsificações foram expostas.";

    // --- Controle Interno ---
    private bool mensagemPendente = false; // "Tem uma mensagem para mostrar?"
    private bool faseConcluida = false;    // "A porta está aberta?"
    public bool luzesApagadas = false;     
    
    private bool jogoPausado = false;
    private bool jogoAcabou = false; 
    
    public float tempoDeInvencibilidade = 2.0f;
    private float tempoDeJogo = 0f;
    private bool playerPerto = false;
    private bool sequenciaFinalIniciada = false;

    private DialogueUI2D dialogueUI;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        dialogueUI = FindFirstObjectByType<DialogueUI2D>();

        if (painelGameOver) painelGameOver.SetActive(false);
        if (painelVitoria) painelVitoria.SetActive(false);
        if (painelPause) painelPause.SetActive(false);
        
        Time.timeScale = 1f; 

        if (painelIntro != null)
        {
            painelIntro.SetActive(true);
            Time.timeScale = 0f; 
            StartCoroutine(FecharIntroAutomaticamente());
        }
        else
        {
            VerificarDialogoInicial();
        }
    }

    void Update()
    {
        if (Time.timeScale > 0) tempoDeJogo += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Escape) && !jogoAcabou)
        {
            if (painelIntro == null || !painelIntro.activeSelf) TogglePause();
        }

        // 1. Se a sequência final começou, monitora se o jogador fechou o diálogo
        if (sequenciaFinalIniciada)
        {
            // Se o painel de diálogo fechou (player terminou de ler)
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

    // --- SISTEMA DE UI ---

    System.Collections.IEnumerator FecharIntroAutomaticamente()
    {
        yield return new WaitForSecondsRealtime(tempoDuracaoIntro);
        FecharIntro();
    }

    public void FecharIntro()
    {
        if (painelIntro != null) painelIntro.SetActive(false);
        Time.timeScale = 1f; 
        VerificarDialogoInicial();
    }

    public void TogglePause()
    {
        jogoPausado = !jogoPausado;
        if(painelPause) painelPause.SetActive(jogoPausado);
        Time.timeScale = jogoPausado ? 0f : 1f;
    }

    // --- LÓGICA DE FASES ---

    void VerificarDialogoInicial()
    {
        // Apenas na Fase 3 o Camaleão pensa alto ao começar
        if (faseAtual == GamePhase.Fase3_Final && dialogueUI != null)
        {
            dialogueUI.ShowDialogue("Camaleão", textoInicioFase3, null);
        }
    }

    // FASE 1
    public void RegistrarInspecao()
    {
        if (faseAtual != GamePhase.Fase1_Inspecao) return;
        
        obrasInspecionadas++;
        Debug.Log($"Progresso: {obrasInspecionadas}/{totalObrasParaInspecionar}");

        if (obrasInspecionadas >= totalObrasParaInspecionar)
        {
            faseConcluida = true; // Destranca a porta
            mensagemPendente = true; // Prepara a mensagem da Fase 1
        }
    }

    // FASE 2 (Chame isso quando completar o Puzzle dos 3 erros)
    public void PuzzleResolvido()
    {
        if (faseAtual != GamePhase.Fase2_Puzzle) return;
        
        faseConcluida = true; // Destranca a porta
        mensagemPendente = true; // Prepara a mensagem da Fase 2
    }

    // FASE 3
    public void DesligarEnergia()
    {
        luzesApagadas = true;
        Debug.Log("Luzes Apagadas! Agora pegue o Elias.");
        
        if (dialogueUI != null)
        {
            dialogueUI.ShowDialogue("Camaleão", textoAposApagarLuz, null);
        }
    }

    void Confrontar()
    {
        if (GameManager.instance == null) return;

        if (GameManager.instance.luzesApagadas)
        {
            // WIN CONDITION: Inicia o diálogo
            if (dialogueUI != null)
            {
                dialogueUI.ShowDialogue("Camaleão", falaDaPrisao, null);
                sequenciaFinalIniciada = true; // Trava o script esperando o diálogo fechar
            }
            else
            {
                // Se não tiver UI, ganha direto (fallback)
                FinalizarJogo();
            }
        }
        else
        {
            // LOSE CONDITION: Luzes acesas
            Debug.Log("Elias te viu! Game Over.");
            GameManager.instance.GameOver();
        }
    }

    void FinalizarJogo()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.Vitoria();
        }
        this.enabled = false; // Desativa este script para não rodar mais nada
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerPerto = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerPerto = false;
    }

    // --- MÉTODOS DE MENSAGEM (O InteractableObject chama isso) ---

    public string ObterMensagemDeConclusao()
    {
        if (mensagemPendente)
        {
            mensagemPendente = false; // Consome para não repetir
            
            // Retorna o texto correto dependendo da fase atual
            if (faseAtual == GamePhase.Fase1_Inspecao) return textoConclusaoFase1;
            if (faseAtual == GamePhase.Fase2_Puzzle) return textoConclusaoFase2;
        }
        return null; // Nenhuma mensagem nova
    }
    
    public void AposMensagemConclusao() { } 

    // --- OUTROS ---

    public bool PodeSair() => faseConcluida;

    public void GameOver()
    {
        if (tempoDeJogo < tempoDeInvencibilidade) return;
        if (jogoAcabou) return;
        
        jogoAcabou = true;
        Debug.Log("GAME OVER!");
        
        if (painelGameOver != null) painelGameOver.SetActive(true);
        Time.timeScale = 0f; 
    }

    public void Vitoria()
    {
        if (jogoAcabou) return;
        jogoAcabou = true;

        Debug.Log("VITÓRIA!");
        if (painelVitoria != null) painelVitoria.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ReiniciarFase()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VoltarMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal"); 
    }
    
    public void SairDoJogo()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}