using UnityEngine;
using UnityEngine.UI; // Necessário para mexer na Image

public class PuzzleManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject painelPuzzle;
    public GameObject[] botoesErro; 

    [Header("Diálogo Pós-Puzzle")]
    [TextArea] public string textoRevelacao = "Inacreditável... Esta obra é uma falsificação barata. O Dr. Elias está vendendo cópias como originais!";

    [Header("Conexão")]
    public PuzzleTrigger triggerDaObra; // Arraste o objeto do quadro aqui para avisar que acabou

    private int errosEncontrados = 0;
    private DialogueUI2D dialogueUI;

    void Start()
    {
        dialogueUI = FindFirstObjectByType<DialogueUI2D>();
        if (painelPuzzle != null) painelPuzzle.SetActive(false);
    }

    public void IniciarPuzzle()
    {
        if (painelPuzzle != null)
        {
            painelPuzzle.SetActive(true);
            Time.timeScale = 0f; 
            errosEncontrados = 0;
            Debug.Log("Puzzle Iniciado - Tempo: " + Time.timeScale);
            
            // Reseta os botões para ficarem invisíveis e clicáveis novamente (caso reinicie)
            foreach (var btn in botoesErro)
            {
                btn.SetActive(true);
                Button b = btn.GetComponent<Button>();
                Image i = btn.GetComponent<Image>();
                
                if (b) b.interactable = true;
                if (i) 
                {
                    Color c = Color.clear; // Totalmente transparente
                    i.color = c;
                }
            }
        }
    }

    // Função chamada pelo botão
    public void ClicarNoErro(GameObject botaoClicado)
    {
        // 1. FEEDBACK VISUAL (Quadrado Verde)
        Image img = botaoClicado.GetComponent<Image>();
        Button btn = botaoClicado.GetComponent<Button>();

        if (img != null)
        {
            // Define a cor para Verde com 50% de transparência (para ver a arte embaixo)
            Color verdeTransparente = new Color(0f, 1f, 0f, 0.5f); 
            img.color = verdeTransparente;
        }

        if (btn != null)
        {
            btn.interactable = false; // Impede de clicar no mesmo erro duas vezes
        }

        // 2. LÓGICA
        errosEncontrados++;
        Debug.Log($"Erros encontrados: {errosEncontrados}/3");

        if (errosEncontrados >= 3)
        {
            // Pequeno delay para o jogador ver o último quadrado verde antes de fechar
            StartCoroutine(FinalizarComDelay());
        }
    }

    System.Collections.IEnumerator FinalizarComDelay()
    {
        yield return new WaitForSecondsRealtime(0.5f); // Espera tempo real (ignora o pause)
        CompletarPuzzle();
    }

    void CompletarPuzzle()
    {
        painelPuzzle.SetActive(false);
        Time.timeScale = 1f; 

        if (GameManager.instance != null)
        {
            GameManager.instance.PuzzleResolvido();
        }

        // Avisa o Trigger que acabou para bloquear novas interações
        if (triggerDaObra != null)
        {
            triggerDaObra.MarcarComoConcluido();
        }

        if (dialogueUI != null)
        {
            dialogueUI.ShowDialogue("Camaleão", textoRevelacao, null);
        }
    }
    
    public void FecharSemTerminar()
    {
        painelPuzzle.SetActive(false);
        Time.timeScale = 1f;
    }
}