using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Dados")]
    public string objectName;
    [TextArea] public string[] sentences;
    
    [Header("Config")]
    public bool freezePlayer = true;
    public bool ehObjetivoDaFase = false;
    private bool jaFoiInspecionado = false;

    private DialogueUI2D dialogueUI;
    private PlayerController playerController;
    private int index = 0;
    private bool isInteracting = false;
    
    // Controle interno para saber se estamos mostrando a msg de "Missão Cumprida"
    private bool mostrandoMensagemFinal = false; 

    void Start()
    {
        dialogueUI = FindFirstObjectByType<DialogueUI2D>();
        var p = GameObject.FindGameObjectWithTag("Player");
        if(p) playerController = p.GetComponent<PlayerController>();
    }

    public void StartInteraction()
    {
        if (isInteracting || sentences.Length == 0) return;
        isInteracting = true;
        index = 0;
        mostrandoMensagemFinal = false; // Reset

        // Codex
        if (CodexManager.instance != null) CodexManager.instance.UnlockEntry(objectName);

        // Fase 1: Contagem
        if (ehObjetivoDaFase && !jaFoiInspecionado)
        {
            jaFoiInspecionado = true;
            if (GameManager.instance != null) GameManager.instance.RegistrarInspecao();
        }

        if (freezePlayer && playerController) playerController.enabled = false;
        dialogueUI.ShowDialogue(objectName, sentences[index], this);
    }

    public void NextSentence()
    {
        index++;
        if (index < sentences.Length) dialogueUI.UpdateText(sentences[index]);
        else EndInteraction();
    }

    public void EndInteraction()
    {
        // 1. Fecha o diálogo atual
        dialogueUI.HideDialogue();

        // 2. Verifica se era a mensagem final especial
        if (mostrandoMensagemFinal)
        {
            isInteracting = false;
            if (freezePlayer && playerController) playerController.enabled = true;
            
            // Avisa o gerente que o player leu a mensagem final (Para Fase 2 trocar de cena)
            if (GameManager.instance != null) GameManager.instance.AposMensagemConclusao();
            return;
        }

        // 3. Verifica se tem mensagem de conclusão pendente no Gerente
        string msgFinal = null;
        if (GameManager.instance != null) msgFinal = GameManager.instance.ObterMensagemDeConclusao();

        if (msgFinal != null)
        {
            // Opa! Tem mensagem nova (ex: "Acabei tudo"). Mostra ela agora.
            mostrandoMensagemFinal = true; // Marca flag
            dialogueUI.ShowDialogue("Camaleão", msgFinal, this); // Reabre o diálogo
        }
        else
        {
            // Vida normal: acabou o papo, libera o player.
            isInteracting = false;
            if (freezePlayer && playerController) playerController.enabled = true;
        }
    }
}