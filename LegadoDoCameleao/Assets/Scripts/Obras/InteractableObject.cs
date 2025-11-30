using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Dados da Interação")]
    public string objectName = "Vaso Antigo"; // Nome que aparece no título
    [TextArea(3, 10)]
    public string[] sentences; // As falas ou descrições

    [Header("Configurações")]
    public bool freezePlayer = true; // Obras congelam o player? NPCs congelam?

    private DialogueUI2D dialogueUI;
    private PlayerController playerController;
    private int index = 0;
    private bool isInteracting = false;

    void Start()
    {
        dialogueUI = FindObjectOfType<DialogueUI2D>();
        // Tenta achar o PlayerController de forma segura (se tiver tag Player é melhor)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null) playerController = player.GetComponent<PlayerController>();
    }

    public void StartInteraction()
    {
        if (isInteracting || sentences.Length == 0) return;

        isInteracting = true;
        index = 0;

        if (freezePlayer && playerController != null)
            playerController.enabled = false; // Trava o movimento

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
        dialogueUI.HideDialogue();

        if (freezePlayer && playerController != null)
            playerController.enabled = true; // Destrava
    }
}