using UnityEngine;

[CreateAssetMenu(fileName = "GunData", menuName = "Scriptable Objects/GunData")]
public class GunData : ScriptableObject
{
    public string gunName;
    public LayerMask targetLayerMask;

    [Header("Fire Config")]
    public float fireRange, fireRate;
    
    [Header("Reload Config")]
    public float magSize, reloadTime;

    [Header("Recoil Config")]
    public Vector2 maxRecoil;
    public float recoilAmount, recoilSpeed, recoilRecoveryTime;

    [Header("VFX")]
    public GameObject bulletTrailPrefab;
    public float bulletSpeed;
}
