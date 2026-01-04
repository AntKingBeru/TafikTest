using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponAnimation : MonoBehaviour
{
    [Header("Sway Config")]
    [SerializeField] private float positionalSway = 0.1f, rotationalSway = 0.1f, swaySmoothness = 1f;
    [SerializeField] private InputActionAsset playerControls;
    
    [Header("Weapon Bobbing")]
    [SerializeField] private float bobbingSpeed = 5f, bobbingAmount = 5f;
    
    [Header("Recoil Animation")]
    [SerializeField] private float recoilAmount = 0.2f, recoilSmoothness = 5f;
    
    private Vector3 _initialPosition = Vector3.zero, _currentRecoil = Vector3.zero;
    private Quaternion _initialRotation = Quaternion.identity;
    private InputAction _moveAction, _lookAction;
    private Vector2 _moveInput, _lookInput;
    private CharacterController _characterController;
    private float _bobberTimer;
    [HideInInspector] public bool isRecoiling;
    
    private void Awake()
    {
        _characterController = transform.root.GetComponent<CharacterController>();
        
        _initialPosition = transform.localPosition;
        _initialRotation = transform.localRotation;
        
        _moveAction = playerControls.FindActionMap("Player").FindAction("Move");
        _lookAction = playerControls.FindActionMap("Player").FindAction("Look");
        
        _moveAction.performed += context => _moveInput = context.ReadValue<Vector2>();
        _moveAction.canceled += context => _moveInput = Vector2.zero;
        
        _lookAction.performed += context => _lookInput = context.ReadValue<Vector2>();
        _lookAction.canceled += context => _lookInput = Vector2.zero;
    }
    
    private void OnEnable()
    {
        _moveAction.Enable();
        _lookAction.Enable();
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _lookAction.Disable();
    }

    private void Update()
    {
        ApplySway();
        ApplyBobbing();
        ApplyRecoil();
    }

    private void ApplySway()
    {
        var look = _lookAction.ReadValue<Vector2>();
        var mouseX = look.x * positionalSway;
        var mouseY = look.y * rotationalSway;
        
        Vector3 positionOffset = new Vector3(mouseX, mouseY, 0) * positionalSway;
        Quaternion rotationalOffset = Quaternion.Euler(new Vector3(-mouseY, -mouseX, mouseX) * rotationalSway);
        
        transform.localPosition = Vector3.Lerp(transform.localPosition, _initialPosition - positionOffset, swaySmoothness * Time.deltaTime);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, _initialRotation * rotationalOffset, swaySmoothness * Time.deltaTime);
    }

    private void ApplyBobbing()
    {
        var moveSpeed = _moveInput.magnitude;
        var bobOffset = 0f;
        
        if (moveSpeed > 0.1f && _characterController.isGrounded)
        {
            _bobberTimer += Time.deltaTime * bobbingSpeed;
            bobOffset = Mathf.Sin(_bobberTimer) * bobbingAmount;
        }
        else
        {
            _bobberTimer = 0f;
            bobOffset = Mathf.Lerp(bobOffset, 0f, Time.deltaTime * swaySmoothness);
        }
        
        transform.localPosition += new Vector3(0, bobOffset, 0);
    }

    private void ApplyRecoil()
    {
        var targetRecoil = Vector3.zero;

        if (isRecoiling)
        {
            targetRecoil = new Vector3(0, 0, -recoilAmount);

            if (Vector3.Distance(_currentRecoil, targetRecoil) < 0.1f)
            {
                isRecoiling = false;
            }
        }
        
        _currentRecoil = Vector3.Lerp(_currentRecoil, targetRecoil, Time.deltaTime * recoilSmoothness);
        transform.localPosition += _currentRecoil;
    }
}