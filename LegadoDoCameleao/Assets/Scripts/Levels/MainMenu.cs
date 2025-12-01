using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string nomePrimeiraFase = "Fase1_Dia"; // Coloque o nome EXATO da sua cena 1

    public void Jogar()
    {
        if (LevelLoader.instance != null)
            LevelLoader.instance.CarregarCena(nomePrimeiraFase);
        else
            SceneManager.LoadScene(nomePrimeiraFase);
    }

    public void Sair()
    {
        Debug.Log("Saindo...");
        Application.Quit();
    }
}