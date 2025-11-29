using UnityEngine;

public class NodeParent : MonoBehaviour
{
    [Header("Configuração da Sala")]
    [Tooltip("Tempo (em segundos) que o NPC fica parado no centro desta sala.")]
    public float tempoDeEsperaCentro = 3.0f; // <--- A variável que estava faltando!

    // Arraste os filhos (obras) para cá no Inspector
    public NodeChild[] filhos;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.3f);

        if (filhos != null)
        {
            Gizmos.color = Color.cyan;
            foreach (var filho in filhos)
            {
                if (filho != null)
                    Gizmos.DrawLine(transform.position, filho.transform.position);
            }
        }
    }
}