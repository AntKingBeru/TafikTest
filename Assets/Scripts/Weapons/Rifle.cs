using System.Collections;
using UnityEngine;

public class Rifle : Gun
{
    public override void Update()
    {
        base.Update();

        if (ShootAction.WasPressedThisFrame())
        {
            TryShoot();
        }
        
        if (ShootAction.WasReleasedThisFrame())
        {
            isShooting = false;
        }
        
        if (ReloadAction.WasPressedThisFrame())
        {
            TryReload();
        }
    }
    
    public override void Shoot()
    {
        RaycastHit hit;
        Vector3 target;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, gunData.fireRange, gunData.targetLayerMask))
        {
            Debug.Log(gunData.gunName + " hit" + hit.collider.name);
            target = hit.point;
        }
        else
        {
            target = cameraTransform.position + (cameraTransform.forward * gunData.fireRange);
        }
        
        StartCoroutine(BulletFire(target, hit));
    }

    private IEnumerator BulletFire(Vector3 targetPosition, RaycastHit hit)
    {
        var bulletTrail = Instantiate(gunData.bulletTrailPrefab, gunMuzzle.position, Quaternion.LookRotation(targetPosition - gunMuzzle.position));

        while (bulletTrail && Vector3.Distance(bulletTrail.transform.position, targetPosition) > 0.1f)
        {
            bulletTrail.transform.position = Vector3.MoveTowards(bulletTrail.transform.position, targetPosition,
                Time.deltaTime * gunData.bulletSpeed);
            yield return null;
        }
        
        Destroy(bulletTrail);
        
        if (hit.collider) BulletHitFX(hit);
    }

    private void BulletHitFX(RaycastHit hit)
    {
        Vector3 hitPosition = hit.point + hit.normal * 0.01f;
        
        GameObject bulletHole = Instantiate(bulletHolePrefab, hitPosition, Quaternion.LookRotation(hit.normal));
        GameObject hitParticle = Instantiate(bulletHitParticlePrefab, hit.point, Quaternion.LookRotation(hit.normal));
        
        bulletHole.transform.parent = hit.collider.transform;
        hitParticle.transform.parent = hit.collider.transform;
        
        Destroy(bulletHole, 5f);
        Destroy(hitParticle, 5f);
    }
}
