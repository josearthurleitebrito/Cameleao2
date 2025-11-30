using UnityEngine;

public class PlayerInteract2D : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;
    
    // Objeto atual que está na área de alcance
    private InteractableObject currentInteractable;

    [Header("UI de Ajuda (Opcional)")]
    public GameObject interactPrompt; // Ex: Ícone "Aperte E"

    void Update()
    {
        if (currentInteractable != null && Input.GetKeyDown(interactKey))
        {
            currentInteractable.StartInteraction();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Tenta pegar o componente genérico
        InteractableObject interactable = other.GetComponent<InteractableObject>();
        
        if (interactable != null)
        {
            currentInteractable = interactable;
            if(interactPrompt != null) interactPrompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Só limpa se for o objeto que estamos focando
        InteractableObject interactable = other.GetComponent<InteractableObject>();
        
        if (interactable != null && interactable == currentInteractable)
        {
            currentInteractable = null;
            if(interactPrompt != null) interactPrompt.SetActive(false);
        }
    }
}