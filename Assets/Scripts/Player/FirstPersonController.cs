using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 6.0f;
    
    [Header("Jump Parameter")]
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float gravity = 9.81f;

    [Header("Camera Settings")]
    public CinemachineCamera cinemachineCamera;
    [SerializeField] private float mouseSensitivity = 2.0f, verticalRange = 80.0f, bobFrequency = 1.0f, bobAmplitude = 0.2f;
    
    [Header("Recoil")]
    private Vector3 _targetRecoil = Vector3.zero, _currentRecoil = Vector3.zero;
    
    // [Header("Footstep Sounds")]
    // [SerializeField] private AudioSource footstepSource;
    // [SerializeField] private AudioClip[] footstepSounds;
    // [SerializeField] private float walkStepInterval = 0.3f, velocityThreshold = 2.0f;
    
    // [Header("Inputs Customization")]
    // [SerializeField] private string horizontalMoveInput = "Horizontal";
    // [SerializeField] private string verticalMoveInput = "Vertical";
    // [SerializeField] private string mouseXInput = "Mouse X";
    // [SerializeField] private string mouseYInput = "Mouse Y";
    // [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset playerControls;
    [SerializeField] private InputActionReference moveAction, jumpAction, lookAction;
    private Vector2 _moveInput, _lookInput;
    private Vector3 _currentMovement = Vector3.zero;
    private CharacterController _characterController;
    private CinemachineBasicMultiChannelPerlin _bobber;
    // private bool _isMoving;
    // private int _lastPlayedIndex = -1;
    private float _verticalRotation, _nextStepTime;
    
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        _bobber = cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        
        moveAction.action.performed += context => _moveInput = context.ReadValue<Vector2>();
        moveAction.action.canceled += context => _moveInput = Vector2.zero;
        
        lookAction.action.performed += context => _lookInput = context.ReadValue<Vector2>();
        lookAction.action.canceled += context => _lookInput = Vector2.zero;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        lookAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
        lookAction.action.Disable();
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        // HandleFootsteps();
    }

    private void LateUpdate()
    {
        CameraBob();
    }

    private void HandleMovement()
    {
        var verticalSpeed = _moveInput.y * walkSpeed;
        var horizontalSpeed = _moveInput.x * walkSpeed;
        
        var horizontalMovement = new  Vector3(horizontalSpeed, 0, verticalSpeed);
        horizontalMovement = transform.rotation * horizontalMovement;

        HandleGravityAndJumping();
        
        _currentMovement.x = horizontalMovement.x;
        _currentMovement.z = horizontalMovement.z;
        
        _characterController.Move(_currentMovement * Time.deltaTime);
        
        // _isMoving = _moveInput.y != 0 || _moveInput.x != 0;
    }

    private void HandleGravityAndJumping()
    {
        if (_characterController.isGrounded)
        {
            _currentMovement.y = -0.5f;

            if (jumpAction.action.triggered)
            {
                _currentMovement.y = jumpForce;
            }
        }
        else
        {
            _currentMovement.y -= gravity * Time.deltaTime;
        }
    }
    
    private void HandleRotation()
    {
        var mouseXRotation = _lookInput.x * mouseSensitivity;
        transform.Rotate(0, mouseXRotation, 0);
        
        _verticalRotation -= _lookInput.y * mouseSensitivity;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -verticalRange, verticalRange);
        cinemachineCamera.transform.localRotation = Quaternion.Euler(_verticalRotation + _currentRecoil.y, _currentRecoil.x, 0);
    }

    // private void HandleFootsteps()
    // {
    //     if (_characterController.isGrounded && _isMoving && Time.time > _nextStepTime &&
    //         _characterController.velocity.magnitude > velocityThreshold)
    //     {
    //         PlayFootstepsSounds();
    //         _nextStepTime = Time.time + walkStepInterval;
    //     }
    // }
    //
    // private void PlayFootstepsSounds()
    // {
    //     int randomIndex;
    //     if (footstepSounds.Length == 1)
    //     {
    //         randomIndex = 0;
    //     }
    //     else
    //     {
    //         randomIndex = Random.Range(0, footstepSounds.Length - 1);
    //         if (randomIndex >= _lastPlayedIndex)
    //         {
    //             randomIndex++;
    //         }
    //     }
    //     
    //     _lastPlayedIndex = randomIndex;
    //     footstepSource.clip = footstepSounds[randomIndex];
    //     footstepSource.Play();
    // }

    public void ApplyRecoil(GunData gunData)
    {
        var recoilX = Random.Range(-gunData.maxRecoil.x, gunData.maxRecoil.x) * gunData.recoilAmount;
        var recoilY = Random.Range(-gunData.maxRecoil.y, gunData.maxRecoil.y) * gunData.recoilAmount;
        
        _targetRecoil = new Vector3(recoilX, recoilY, 0);
        
        _currentRecoil = Vector3.MoveTowards(_currentRecoil, _targetRecoil, Time.deltaTime * gunData.recoilSpeed);
    }

    public void ResetRecoil(GunData gunData)
    {
        _currentRecoil = Vector3.MoveTowards(_currentRecoil, Vector3.zero, Time.deltaTime * gunData.recoilSpeed);
        _targetRecoil = Vector3.MoveTowards(_targetRecoil, Vector3.zero, Time.deltaTime * gunData.recoilSpeed);
    }

    private void CameraBob()
    {
        if (_characterController.isGrounded && _characterController.velocity.magnitude > 0.1f)
        {
            _bobber.FrequencyGain = bobFrequency;
            _bobber.AmplitudeGain = bobAmplitude;
        }
        else
        {
            _bobber.FrequencyGain = 0;
            _bobber.AmplitudeGain = 0;
        }
    }
}