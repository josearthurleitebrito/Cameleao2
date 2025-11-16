using UnityEngine;
using UnityEngine.Rendering.Universal; // Importante para acessar o Light2D do URP

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _playerRigidbody2D;
    private Animator _playerAnimator;

    // Váriaveis de configuração de velocidade
    [Header("Configurações de Velocidade")]
    public float _playerRunSpeed = 8f; 	  
    public float _playerNormalSpeed = 5f; 
    public float _playerSlowSpeed = 2f; 	 
    private float _currentSpeed; 		 
    private Vector2 _rawInput;
    
    // Variáveis para persistência da direção
    private float _lastMoveX;
    private float _lastMoveY;

    // Configuração de teclas
    [Header("Configuração de Teclas")]
    [Tooltip("Tecla ou botão para andar mais devagar (modo Stealth)")]
    public KeyCode _slowMoveKey = KeyCode.LeftControl; 
    [Tooltip("Tecla ou botão para Correr (Run)")]
    public KeyCode _runKey = KeyCode.LeftShift;
    
    // --- NOVO: Configuração da Lanterna ---
    [Header("Lanterna do Camaleão")]
    [Tooltip("Arraste o componente Light2D (luz do jogador) aqui.")]
    public Light2D playerLight;
    [Tooltip("Tecla para ligar/desligar a luz.")]
    public KeyCode _lightToggleKey = KeyCode.Q;


    void Start()
    {
        _playerRigidbody2D = GetComponent<Rigidbody2D>();
        _playerAnimator = GetComponent<Animator>();

        _currentSpeed = _playerNormalSpeed; 

        // Adiciona uma verificação para garantir que o Light2D está atribuído
        if (playerLight == null)
        {
            Debug.LogWarning("O componente Light2D não foi atribuído ao PlayerController!");
        }
    }

    void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        _rawInput = new Vector2(horizontalInput, verticalInput).normalized;

        ControlPlayerSpeed();
        
        // --- NOVO: Lógica da Lanterna ---
        HandleLightInput();
    }

    void FixedUpdate()
    {
        Vector2 movementVector = _rawInput;

        // Lógica de Movimento
        if (movementVector.sqrMagnitude > 0.01f) 
        {
            MovePlayer(movementVector);

            _playerAnimator.SetFloat("AxisX", movementVector.x);
            _playerAnimator.SetFloat("AxisY", movementVector.y);
            
            _lastMoveX = movementVector.x;
            _lastMoveY = movementVector.y;
            
            _playerAnimator.SetInteger("Movimento", 1); 
        }
        else
        {
            _playerAnimator.SetInteger("Movimento", 0); 
            
            _playerAnimator.SetFloat("LastMoveX", _lastMoveX);
            _playerAnimator.SetFloat("LastMoveY", _lastMoveY);
        }
    }

    // Controla a velocidade do jogador com base na entrada do usuário
    void ControlPlayerSpeed()
    {
        if (Input.GetKey(_slowMoveKey))
        {
            _currentSpeed = _playerSlowSpeed;
        }
        else if (Input.GetKey(_runKey))
        {
            _currentSpeed = _playerRunSpeed;
        }
        else
        {
            _currentSpeed = _playerNormalSpeed;
        }
    }

    // Move o jogador com base na direção e velocidade atuais
    void MovePlayer(Vector2 direction)
    {
        _playerRigidbody2D.MovePosition(_playerRigidbody2D.position + direction * _currentSpeed * Time.fixedDeltaTime);
    }
    
    // --- NOVO MÉTODO: Lógica de Input da Lanterna ---
    void HandleLightInput()
    {
        // Usa GetKeyDown para detectar apenas o momento em que a tecla é pressionada
        if (Input.GetKeyDown(_lightToggleKey))
        {
            ToggleLight();
        }
    }

    // --- NOVO MÉTODO: Ligar/Desligar a Luz ---
    void ToggleLight()
    {
        if (playerLight != null)
        {
            // O componente Light2D usa a propriedade 'enabled' para ligar/desligar
            playerLight.enabled = !playerLight.enabled;
            
            // Dica: Adicione um SFX de clique/ligar aqui!
            // Ex: AudioManager.instance.PlaySFX("Click");
        }
    }
}