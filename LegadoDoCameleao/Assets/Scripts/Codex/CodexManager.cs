using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CodexManager : MonoBehaviour
{
    public static CodexManager instance;

    [System.Serializable]
    public class CodexEntry
    {
        [Header("Configuração")]
        // AGORA É UMA LISTA! Pode adicionar quantos nomes quiser aqui.
        public List<string> idNames; 
        
        public string title;
        [TextArea(3, 10)]
        public string description;
        public bool isUnlocked = false;
    }

    [Header("Dados")]
    public List<CodexEntry> allEntries;

    [Header("UI References")]
    public GameObject codexPanel;
    public Transform contentArea;
    public GameObject textTemplate;

    private bool isOpen = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        codexPanel.SetActive(false);
        textTemplate.SetActive(false);
    }

    public void ToggleCodex()
    {
        isOpen = !isOpen;
        codexPanel.SetActive(isOpen);

        if (isOpen)
        {
            UpdateUI();
        }
    }

    // Chamado quando o jogador interage com a obra
    public void UnlockEntry(string objectName)
    {
        foreach (var entry in allEntries)
        {
            // NOVA VERIFICAÇÃO: Checa se a lista de IDs contém o nome do objeto
            if (entry.idNames.Contains(objectName))
            {
                if (!entry.isUnlocked)
                {
                    entry.isUnlocked = true;
                    Debug.Log("Codex Desbloqueado: " + entry.title);
                    // Dica: Aqui você pode adicionar um som de "conquista"
                }
                break; // Encontrou e desbloqueou, pode parar de procurar
            }
        }
    }

    void UpdateUI()
    {
        foreach (Transform child in contentArea)
        {
            if (child.gameObject != textTemplate)
                Destroy(child.gameObject);
        }

        foreach (var entry in allEntries)
        {
            if (entry.isUnlocked)
            {
                GameObject newItem = Instantiate(textTemplate, contentArea);
                newItem.SetActive(true);
                
                TMP_Text itemText = newItem.GetComponent<TMP_Text>();
                itemText.text = $"<b><size=120%>{entry.title}</size></b>\n{entry.description}\n----------------";
            }
        }
    }
}