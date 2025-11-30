using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _playerRigidbody2D;
    private Animator _playerAnimator;

    [Header("Configurações de Velocidade")]
    public float _playerRunSpeed = 8f;    
    public float _playerNormalSpeed = 5f; 
    public float _playerSlowSpeed = 2f;      
    private float _currentSpeed;         
    private Vector2 _rawInput;
    
    private float _lastMoveX;
    private float _lastMoveY;

    [Header("Configuração de Teclas")]
    public KeyCode _slowMoveKey = KeyCode.LeftControl; 
    public KeyCode _runKey = KeyCode.LeftShift;
    
    [Header("Lanterna do Camaleão")]
    public Light2D playerLight;
    public KeyCode _lightToggleKey = KeyCode.Q;

    void Start()
    {
        _playerRigidbody2D = GetComponent<Rigidbody2D>();
        _playerAnimator = GetComponent<Animator>();
        _currentSpeed = _playerNormalSpeed; 

        if (playerLight == null) Debug.LogWarning("Light2D não atribuído no Player!");
    }

    void Update()
    {
        // Se o script estiver desativado (capturado), não processa input
        if (!this.enabled) return;

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        _rawInput = new Vector2(horizontalInput, verticalInput).normalized;

        ControlPlayerSpeed();
        HandleLightInput();
    }

    void FixedUpdate()
    {
        Vector2 movementVector = _rawInput;

        if (movementVector.sqrMagnitude > 0.01f) 
        {
            MovePlayer(movementVector);

            _playerAnimator.SetFloat("AxisX", movementVector.x);
            _playerAnimator.SetFloat("AxisY", movementVector.y);
            
            _lastMoveX = movementVector.x;
            _lastMoveY = movementVector.y;
            
            _playerAnimator.SetInteger("Movimento", 1); // 1 = Andando
        }
        else
        {
            _playerRigidbody2D.linearVelocity = Vector2.zero; 
            _playerAnimator.SetInteger("Movimento", 0); // 0 = Idle
            
            _playerAnimator.SetFloat("LastMoveX", _lastMoveX);
            _playerAnimator.SetFloat("LastMoveY", _lastMoveY);
        }
    }

    void ControlPlayerSpeed()
    {
        if (Input.GetKey(_slowMoveKey)) _currentSpeed = _playerSlowSpeed;
        else if (Input.GetKey(_runKey)) _currentSpeed = _playerRunSpeed;
        else _currentSpeed = _playerNormalSpeed;
    }

    void MovePlayer(Vector2 direction)
    {
        _playerRigidbody2D.MovePosition(_playerRigidbody2D.position + direction * _currentSpeed * Time.fixedDeltaTime);
    }
    
    void HandleLightInput()
    {
        if (Input.GetKeyDown(_lightToggleKey)) ToggleLight();
    }

    void ToggleLight()
    {
        if (playerLight != null) playerLight.enabled = !playerLight.enabled;
    }

    // --- NOVA FUNÇÃO DE CAPTURA (Action) ---
    public void SerCapturado()
    {
        // 1. Para a física imediatamente
        if (_playerRigidbody2D != null) _playerRigidbody2D.linearVelocity = Vector2.zero;

        // 2. Desativa este script para parar inputs
        this.enabled = false; 

        // 3. Define a animação para ACTION (Movimento = 2)
        if (_playerAnimator != null)
        {
            _playerAnimator.SetInteger("Movimento", 2);
        }
    }
}