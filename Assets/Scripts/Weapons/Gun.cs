using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Gun : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset playerControls;
    
    public GunData gunData;
    public Transform gunMuzzle;

    public GameObject bulletHolePrefab;
    public GameObject bulletHitParticlePrefab;
    
    [HideInInspector] public FirstPersonController playerController;
    [HideInInspector] public Transform cameraTransform;
    [HideInInspector] public bool isShooting;

    protected InputAction ShootAction, ReloadAction;
    private float _currentAmmo, _nextTimeToFire = 0f;
    private bool _isReloading;
    private WeaponAnimation _weaponAnimation;
    private CinemachineImpulseSource _recoilShakeSource;
    
    private void Awake()
    {
        _currentAmmo = gunData.magSize;
        playerController = transform.root.GetComponent<FirstPersonController>();
        cameraTransform = playerController.cinemachineCamera.transform;
        _weaponAnimation = GetComponent<WeaponAnimation>();
        _recoilShakeSource = GetComponent<CinemachineImpulseSource>();
        
        ShootAction = playerControls.FindActionMap("Player").FindAction("Attack");
        ReloadAction = playerControls.FindActionMap("Player").FindAction("Reload");
    }
    
    private void OnEnable()
    {
        ShootAction.Enable();
    }

    private void OnDisable()
    {
        ShootAction.Disable();
    }

    public virtual void Update()
    {
        playerController.ResetRecoil(gunData);
    }

    public void TryReload()
    {
        if (!_isReloading && _currentAmmo < gunData.magSize)
        {
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        _isReloading = true;

        Debug.Log(gunData.gunName + " is reloading...");
        
        yield return new WaitForSeconds(gunData.reloadTime);

        _currentAmmo = gunData.magSize;
        _isReloading = false;
        
        Debug.Log(gunData.gunName + " is reloaded.");
    }

    public void TryShoot()
    {
        if (_isReloading)
        {
            Debug.Log(gunData.gunName + " is reloading...");
            return;
        }

        if (_currentAmmo <= 0f)
        {
            Debug.Log(gunData.gunName + " has no bullets left, Please reload.");
            return;
        }

        if (Time.time >= _nextTimeToFire)
        {
            _nextTimeToFire = Time.time + (1 / gunData.fireRate);
            HandleShoot();
        }
    }

    private void HandleShoot()
    {
        isShooting = true;
        
        _currentAmmo--;
        Debug.Log(gunData.gunName + " has shot!, Bullets left: " + _currentAmmo);
        Shoot();
        
        playerController.ApplyRecoil(gunData);
        _weaponAnimation.isRecoiling = true;
        _recoilShakeSource.GenerateImpulse();
    }

    public abstract void Shoot();
}