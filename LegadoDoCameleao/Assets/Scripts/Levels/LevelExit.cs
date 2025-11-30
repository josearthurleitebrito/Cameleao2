using UnityEngine;
using UnityEngine.SceneManagement; 

public class LevelExit : MonoBehaviour
{
    [Header("Configuração")]
    public string nomeProximaCena; 
    public KeyCode teclaInteracao = KeyCode.E;

    [Header("Diálogos")]
    [TextArea] public string textoIncompleto = "Ainda sinto que deixei pistas para trás. Preciso investigar mais obras.";
    [TextArea] public string textoCompleto = "Já documentei tudo o que precisava aqui. Hora de sair antes que me vejam.";

    private DialogueUI2D dialogueUI;
    private bool playerNaPorta = false;
    private bool prontoParaSair = false; // Trava para saber se já mostramos o texto final

    void Start()
    {
        dialogueUI = FindFirstObjectByType<DialogueUI2D>();
    }

    void Update()
    {
        if (playerNaPorta && Input.GetKeyDown(teclaInteracao))
        {
            // Se já mostramos o texto de "Tudo pronto", o próximo clique sai da fase
            if (prontoParaSair)
            {
                CarregarCena();
            }
            else
            {
                VerificarProgresso();
            }
        }
    }

    void VerificarProgresso()
    {
        // 1. Verifica com o GameManager se acabou
        if (GameManager.instance != null && GameManager.instance.PodeSair())
        {
            // CASO SUCESSO: Mostra texto de conclusão e prepara para sair
            if (dialogueUI != null)
            {
                dialogueUI.ShowDialogue("Camaleão", textoCompleto, null); // Passamos null pois não é um InteractableObject comum
            }
            
            prontoParaSair = true; // Marca que o próximo 'E' vai sair do jogo
            Debug.Log("Fase concluída. Aperte E novamente para sair.");
        }
        else
        {
            // CASO INCOMPLETO: Mostra aviso
            if (dialogueUI != null)
            {
                // Dica: Você pode mostrar quantas faltam se quiser acessar o GameManager
                dialogueUI.ShowDialogue("Camaleão", textoIncompleto, null);
            }
        }
    }

    void CarregarCena()
    {
        Debug.Log("Saindo da fase...");
        SceneManager.LoadScene(nomeProximaCena);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNaPorta = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            playerNaPorta = false;
            prontoParaSair = false; // Reseta se o player se afastar da porta
            if (dialogueUI != null) dialogueUI.HideDialogue(); // Fecha o texto se sair de perto
        }
    }
}