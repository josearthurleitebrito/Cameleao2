using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader instance;

    [Header("Configuração")]
    public GameObject fadePanel; 
    public Image fadeImage;      
    public float fadeDuration = 1.0f; 

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

    void Start()
    {
        if (fadePanel != null)
        {
            fadePanel.SetActive(true);
            StartCoroutine(Fade(0f)); 
        }
    }

    public void CarregarCena(string nomeCena)
    {
        StartCoroutine(TransitionSequence(nomeCena));
    }

    IEnumerator TransitionSequence(string nomeCena)
    {
        // 1. Fade Out (Escurece)
        yield return StartCoroutine(Fade(1f)); 

        // 2. Carrega a cena
        SceneManager.LoadScene(nomeCena);

        // 3. Espera um pouquinho para a cena carregar
        yield return new WaitForSeconds(0.1f);
        
        // --- REMOVIDO: Time.timeScale = 1f; --- 
        // Deixa o GameManager da nova cena decidir se pausa ou não!
        
        // 4. Fade In (Clareia) na nova cena
        yield return StartCoroutine(Fade(0f)); 
    }

    IEnumerator Fade(float targetAlpha)
    {
        if (fadeImage == null) yield break;

        fadePanel.SetActive(true);
        float startAlpha = fadeImage.color.a;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime; 
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;

            yield return null;
        }

        Color finalColor = fadeImage.color;
        finalColor.a = targetAlpha;
        fadeImage.color = finalColor;

        if (targetAlpha == 0f) fadePanel.SetActive(false);
    }
}