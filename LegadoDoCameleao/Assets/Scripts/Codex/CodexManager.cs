using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // Necessário para detectar troca de cena

public class CodexManager : MonoBehaviour
{
    public static CodexManager instance;

    [System.Serializable]
    public class CodexEntry
    {
        [Header("Configuração")]
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
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- CORREÇÃO AQUI: Se inscrever no evento de troca de cena ---
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Roda toda vez que uma nova fase carrega
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isOpen = false; // Reseta o estado lógico
        if (codexPanel != null) codexPanel.SetActive(false); // Reseta o visual
    }
    // -------------------------------------------------------------

    void Start()
    {
        if(codexPanel != null) codexPanel.SetActive(false);
        if(textTemplate != null) textTemplate.SetActive(false);
    }

    public void ToggleCodex()
    {
        isOpen = !isOpen;
        if (codexPanel != null) codexPanel.SetActive(isOpen);
        if (isOpen) UpdateUI();
    }

    public void UnlockEntry(string objectName)
    {
        foreach (var entry in allEntries)
        {
            if (entry.idNames.Contains(objectName))
            {
                if (!entry.isUnlocked)
                {
                    entry.isUnlocked = true;
                }
                break; 
            }
        }
    }

    void UpdateUI()
    {
        foreach (Transform child in contentArea)
        {
            if (child.gameObject != textTemplate) Destroy(child.gameObject);
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