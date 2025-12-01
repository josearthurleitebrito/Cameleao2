using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; 

public enum GamePhase { Fase1_Inspecao, Fase2_Puzzle, Fase3_Final }

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

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

    [Header("Fase 2: Puzzle")]
    public string nomeCenaFase3; 

    // --- AQUI ESTÃO AS VARIÁVEIS QUE DEVEM FICAR FORA DO START ---
    [Header("Mensagens de Início (Objetivos)")]
    [TextArea] public string textoInicioFase1 = "Cheguei. Preciso me misturar aos visitantes e inspecionar as obras para encontrar irregularidades.";
    [TextArea] public string textoInicioFase2 = "O museu fechou. Agora é hora de encontrar a obra falsificada e corrigir o erro sem ser visto pelos guardas.";
    [TextArea] public string textoInicioFase3 = "O leilão ilegal já começou. A segurança está máxima. Preciso encontrar a Caixa de Força.";

    [Header("Mensagens de Conclusão")]
    [TextArea] public string textoConclusaoFase1 = "Já tenho provas suficientes. Preciso sair daqui agora.";
    [TextArea] public string textoConclusaoFase2 = "Descobri a falsificação! O Dr. Elias está no escritório. Preciso ir para a porta de acesso.";
    [TextArea] public string textoAposApagarLuz = "Escuridão total! O sistema caiu. É agora! Tenho que render o Dr. Elias.";
    // -------------------------------------------------------------

    // --- Controle Interno ---
    private bool mensagemPendente = false;
    private bool faseConcluida = false;
    public bool luzesApagadas = false;     
    
    private bool jogoPausado = false;
    private bool jogoAcabou = false; 
    
    public float tempoDeInvencibilidade = 2.0f;
    private float tempoDeJogo = 0f;

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

        // --- FLUXO DE INÍCIO ---
        if (painelIntro != null)
        {
            painelIntro.SetActive(true);
            Time.timeScale = 0f; 
            StartCoroutine(FecharAtroAutomaticamente());
        }
        else
        {
            // Se não tem intro, começa direto com o diálogo
            StartCoroutine(IniciarDialogoComAtraso(1.0f));
        }
    }

    void Update()
    {
        if (Time.timeScale > 0) tempoDeJogo += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Escape) && !jogoAcabou)
        {
            if (painelIntro == null || !painelIntro.activeSelf) TogglePause();
        }
    }

    // --- SISTEMA DE UI ---

    IEnumerator FecharAtroAutomaticamente()
    {
        yield return new WaitForSecondsRealtime(tempoDuracaoIntro);
        FecharIntro();
    }

    public void FecharIntro()
    {
        if (painelIntro != null) painelIntro.SetActive(false);
        Time.timeScale = 1f; 
        
        // Inicia o diálogo APÓS fechar a intro
        StartCoroutine(IniciarDialogoComAtraso(0.5f));
    }

    IEnumerator IniciarDialogoComAtraso(float delay)
    {
        yield return new WaitForSeconds(delay);
        VerificarDialogoInicial();
    }

    void VerificarDialogoInicial()
    {
        if (dialogueUI == null) return;

        string textoParaMostrar = null;

        switch (faseAtual)
        {
            case GamePhase.Fase1_Inspecao:
                textoParaMostrar = textoInicioFase1;
                break;
            case GamePhase.Fase2_Puzzle:
                textoParaMostrar = textoInicioFase2;
                break;
            case GamePhase.Fase3_Final:
                textoParaMostrar = textoInicioFase3;
                break;
        }

        if (!string.IsNullOrEmpty(textoParaMostrar))
        {
            dialogueUI.ShowDialogue("Camaleão", textoParaMostrar, null);
        }
    }

    public void TogglePause()
    {
        jogoPausado = !jogoPausado;
        if(painelPause) painelPause.SetActive(jogoPausado);
        Time.timeScale = jogoPausado ? 0f : 1f;
    }

    // --- LÓGICA DE FASES ---

    public void RegistrarInspecao()
    {
        if (faseAtual != GamePhase.Fase1_Inspecao) return;
        
        obrasInspecionadas++;
        Debug.Log($"Progresso: {obrasInspecionadas}/{totalObrasParaInspecionar}");

        if (obrasInspecionadas >= totalObrasParaInspecionar)
        {
            faseConcluida = true;
            mensagemPendente = true;
        }
    }

    public void PuzzleResolvido()
    {
        if (faseAtual != GamePhase.Fase2_Puzzle) return;
        faseConcluida = true;
        mensagemPendente = true; 
    }

    public void DesligarEnergia()
    {
        luzesApagadas = true;
        Debug.Log("Luzes Apagadas! Agora pegue o Elias.");
        
        if (dialogueUI != null)
        {
            dialogueUI.ShowDialogue("Camaleão", textoAposApagarLuz, null);
        }
    }
    
    public void ApagarLuzesPegarElias() { DesligarEnergia(); }

    // --- OUTROS ---

    public bool PodeSair() => faseConcluida;

    public string ObterMensagemDeConclusao()
    {
        if (mensagemPendente)
        {
            mensagemPendente = false;
            if (faseAtual == GamePhase.Fase1_Inspecao) return textoConclusaoFase1;
            if (faseAtual == GamePhase.Fase2_Puzzle) return textoConclusaoFase2;
        }
        return null;
    }
    
    public void AposMensagemConclusao() { } 

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
        // Tenta usar o LevelLoader se existir, senão usa SceneManager
        GameObject loader = GameObject.Find("_LEVEL_LOADER");
        if (loader != null)
             loader.GetComponent<LevelLoader>().CarregarCena("MenuPrincipal");
        else
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