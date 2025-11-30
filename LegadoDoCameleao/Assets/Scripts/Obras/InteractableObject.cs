using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Dados da Interação")]
    public string objectName = "Vaso Antigo"; // Nome que aparece no título
    [TextArea(3, 10)]
    public string[] sentences; // As falas ou descrições

    [Header("Configurações")]
    public bool freezePlayer = true; // Obras congelam o player? NPCs congelam?

    [Header("Missão")]
    public bool ehObjetivoDaFase = false; // Marque TRUE se for uma obra necessária para passar de fase
    private bool jaFoiInspecionado = false; // Controle interno

    private DialogueUI2D dialogueUI;
    private PlayerController playerController;
    private int index = 0;
    private bool isInteracting = false;

    void Start()
    {
        // Atualize para FindFirstObjectByType
        if (dialogueUI == null)
            dialogueUI = FindFirstObjectByType<DialogueUI2D>();

        // Atualize para FindFirstObjectByType
        // Tenta achar o PlayerController de forma segura (se tiver tag Player é melhor)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null) playerController = player.GetComponent<PlayerController>();
    }

    public void StartInteraction()
    {
        if (isInteracting || sentences.Length == 0) return;

        isInteracting = true;
        index = 0;

        // --- LÓGICA DO CODEX (Já existia) ---
        if (CodexManager.instance != null) 
            CodexManager.instance.UnlockEntry(objectName);

        // --- NOVA LÓGICA DE PROGRESSO DA FASE ---
        if (ehObjetivoDaFase && !jaFoiInspecionado)
        {
            jaFoiInspecionado = true; // Marca como visto para não contar de novo
            
            if (GameManager.instance != null)
            {
                GameManager.instance.RegistrarInspecao();
            }
        }
        // ----------------------------------------

        if (freezePlayer && playerController != null)
            playerController.enabled = false;

        dialogueUI.ShowDialogue(objectName, sentences[index], this);
    }

    public void NextSentence()
    {
        index++;
        if (index < sentences.Length)
        {
            dialogueUI.UpdateText(sentences[index]);
        }
        else
        {
            EndInteraction();
        }
    }

    public void EndInteraction()
    {
        isInteracting = false;
        dialogueUI.HideDialogue(); // Fecha o texto da obra

        // --- NOVA LÓGICA ---
        // Verifica se acabamos de completar a missão com essa obra
        if (GameManager.instance != null && GameManager.instance.DeveAvisarConclusao())
        {
            // Abre o aviso do Camaleão
            // Passamos 'null' no terceiro parâmetro pois é um pensamento, não uma interação repetível
            dialogueUI.ShowDialogue("Camaleão", GameManager.instance.textoDeConclusao, null);
            
            // O Player continua travado porque abriu um novo diálogo
            return; 
        }
        // -------------------

        if (freezePlayer && playerController != null)
            playerController.enabled = true; // Destrava o player
    }
}