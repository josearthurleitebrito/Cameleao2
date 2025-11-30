using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Configuração da Fase")]
    public int totalObrasParaInspecionar; 
    
    [Header("Mensagens")]
    [TextArea] public string textoDeConclusao = "Consegui provas suficientes. É melhor eu voltar para a entrada agora.";

    private int obrasInspecionadas = 0;
    private bool faseConcluida = false;
    
    // Variável de controle para avisar apenas uma vez
    private bool avisarJogador = false; 

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void RegistrarInspecao()
    {
        obrasInspecionadas++;
        Debug.Log($"Progresso: {obrasInspecionadas} / {totalObrasParaInspecionar}");

        if (obrasInspecionadas >= totalObrasParaInspecionar)
        {
            faseConcluida = true;
            avisarJogador = true; // Marca que precisamos avisar o jogador
        }
    }

    public bool PodeSair()
    {
        return faseConcluida;
    }

    // Função que o InteractableObject vai chamar ao fechar o diálogo
    public bool DeveAvisarConclusao()
    {
        if (avisarJogador)
        {
            avisarJogador = false; // Consome o aviso para não repetir
            return true;
        }
        return false;
    }
}