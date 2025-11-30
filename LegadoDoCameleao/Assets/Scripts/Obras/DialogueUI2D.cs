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

    private InteractableObject currentSource; // Referência genérica
    private bool isTyping = false;
    private string currentSentence;
    private Coroutine typingCoroutine;

    void Start()
    {
        panel.SetActive(false);
    }

    void Update()
    {
        // Se o painel não está ativo, não faz nada
        if (!panel.activeSelf) return;

        // Input para avançar texto (Espaço ou E)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                StopTypingInstantly();
            }
            else
            {
                // SE TIVER DONO (NPC/Obra): Avança para a próxima frase dele
                if (currentSource != null)
                {
                    currentSource.NextSentence();
                }
                // SE NÃO TIVER DONO (Mensagem do Sistema/GameManager):
                else
                {
                    // Apenas fecha a caixa imediatamente
                    HideDialogue();
                }
            }
        }
    }

    public void ShowDialogue(string title, string text, InteractableObject source)
    {
        currentSource = source;
        nameText.text = title; // Pode ser nome do NPC ou Título da Obra
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