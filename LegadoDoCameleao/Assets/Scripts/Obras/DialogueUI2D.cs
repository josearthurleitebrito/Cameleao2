using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueUI2D : MonoBehaviour
{
    [Header("Referências")]
    public GameObject panel;
    public TMP_Text nameText;
    public TMP_Text dialogueText;

    [Header("Configuração")]
    public float typingSpeed = 0.03f;

    private InteractableObject currentSource; 
    private bool isTyping = false;
    private string currentSentence;
    private Coroutine typingCoroutine;

    // Referência ao Player para travar movimento
    private PlayerController playerController;

    void Start()
    {
        panel.SetActive(false);
        
        // Encontra o player automaticamente
        playerController = FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
        if (!panel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                StopTypingInstantly();
            }
            else
            {
                if (currentSource != null)
                {
                    currentSource.NextSentence();
                }
                else
                {
                    HideDialogue();
                }
            }
        }
    }

    public void ShowDialogue(string title, string text, InteractableObject source)
    {
        // --- TRAVA O JOGADOR ---
        if (playerController != null)
        {
            playerController.enabled = false; // Desativa os inputs
            
            // Zera a velocidade para ele não deslizar se estiver andando
            Rigidbody2D rb = playerController.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            
            // Opcional: Forçar animação de Idle
            Animator anim = playerController.GetComponent<Animator>();
            if (anim != null) anim.SetInteger("Movimento", 0);
        }
        // -----------------------

        currentSource = source;
        nameText.text = title; 
        panel.SetActive(true);
        DisplaySentence(text);
    }

    public void UpdateText(string newSentence)
    {
        DisplaySentence(newSentence);
    }

    public void HideDialogue()
    {
        panel.SetActive(false);
        currentSource = null;

        // --- DESTRAVA O JOGADOR ---
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        // --------------------------
    }

    private void DisplaySentence(string sentence)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        currentSentence = sentence;
        typingCoroutine = StartCoroutine(TypeSentence(sentence));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    private void StopTypingInstantly()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        dialogueText.text = currentSentence;
        isTyping = false;
    }
}