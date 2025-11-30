using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class FuseBox : MonoBehaviour
{
    [Header("Configuração")]
    public KeyCode interactKey = KeyCode.E;
    
    [Header("Iluminação Global")]
    public Light2D luzGlobal; 
    public float intensidadeFinal = 0.2f; // Ambiente fica escuro
    
    [Header("Luz do Leilão (Dr. Elias)")]
    [Tooltip("Arraste a luz de foco que está em cima do Dr. Elias aqui.")]
    public Light2D luzElias; 
    
    [Header("Transição")]
    public float duracaoTransicao = 2.0f;

    private bool playerNaArea = false;
    private bool jaDesligou = false;

    void Update()
    {
        if (playerNaArea && Input.GetKeyDown(interactKey) && !jaDesligou)
        {
            jaDesligou = true;
            
            // 1. Avisa o GameManager (Lógica do Jogo e Diálogo)
            if (GameManager.instance != null)
            {
                GameManager.instance.DesligarEnergia();
            }

            // 2. Inicia o efeito visual (Apagar luzes)
            StartCoroutine(ApagarLuzes());
        }
    }

    IEnumerator ApagarLuzes()
    {
        float tempoDecorrido = 0f;
        
        // Pega as intensidades iniciais
        float globalInicial = (luzGlobal != null) ? luzGlobal.intensity : 1f;
        float eliasInicial = (luzElias != null) ? luzElias.intensity : 1f;

        while (tempoDecorrido < duracaoTransicao)
        {
            tempoDecorrido += Time.deltaTime;
            float percentual = tempoDecorrido / duracaoTransicao;
            
            // Fade da Luz Global (vai para intensidadeFinal)
            if (luzGlobal != null)
                luzGlobal.intensity = Mathf.Lerp(globalInicial, intensidadeFinal, percentual);

            // Fade da Luz do Elias (vai para ZERO - Breu total no palco)
            if (luzElias != null)
                luzElias.intensity = Mathf.Lerp(eliasInicial, 0f, percentual);
            
            yield return null; 
        }

        // Garante valores finais
        if (luzGlobal != null) luzGlobal.intensity = intensidadeFinal;
        if (luzElias != null) luzElias.intensity = 0f; // Garante que apagou
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNaArea = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNaArea = false;
    }
}