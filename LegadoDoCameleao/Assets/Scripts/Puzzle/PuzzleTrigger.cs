using UnityEngine;

public class PuzzleTrigger : MonoBehaviour
{
    public KeyCode teclaInteracao = KeyCode.E;
    private bool playerPerto = false;
    
    // Variável de controle
    private bool puzzleConcluido = false; 
    private DialogueUI2D dialogueUI;

    [Header("Referência")]
    public PuzzleManager puzzleManager;

    [Header("Mensagem Pós-Puzzle")]
    public string textoJaFeito = "Já identifiquei as falhas nesta obra. Não preciso olhar de novo.";

    void Start()
    {
        dialogueUI = FindFirstObjectByType<DialogueUI2D>();
    }

    void Update()
    {
        if (playerPerto && Input.GetKeyDown(teclaInteracao))
        {
            // SE AINDA NÃO FEZ: Abre o Puzzle
            if (!puzzleConcluido)
            {
                if (puzzleManager != null)
                {
                    puzzleManager.IniciarPuzzle();
                }
            }
            // SE JÁ FEZ: Mostra apenas o texto de "Já fiz"
            else
            {
                if (dialogueUI != null)
                {
                    dialogueUI.ShowDialogue("Camaleão", textoJaFeito, null);
                }
            }
        }
    }

    // Chamado pelo PuzzleManager quando ganha o jogo
    public void MarcarComoConcluido()
    {
        puzzleConcluido = true;
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