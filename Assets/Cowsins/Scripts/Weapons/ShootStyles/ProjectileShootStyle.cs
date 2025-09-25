using System;
using System.Collections;
using UnityEngine;

namespace cowsins
{
    /// <summary>
    /// When a weapon is set to Projectile Shoot Style, its shootBehaviour in WeaponIdentification is assigned to this ProjectileShootStyle.
    /// This is done when the weapon is unholstered.
    /// </summary>
    public class ProjectileShootStyle : IShootStyle
    {
        WeaponController weaponController;
        PlayerDependencies playerDependencies;
        WeaponAnimator weaponAnimator;
        PlayerMultipliers multipliers;
        private Transform[] firePoint;
        Weapon_SO weapon;
        Camera mainCamera;

        private bool canShoot = true;
        public Coroutine shootingCoroutine, allowShootCoroutine;

        // Calls events in Weapon Controller when shooting or hitting an enemy ( ProjectileShootStyle does not handle hits so onHit is not used here,
        // but it is still required by IShootStyle.
        public event Action onShoot;
#pragma warning disable CS0067
        public event Action<int, float, RaycastHit, bool> onHit;
#pragma warning restore CS0067

        public ProjectileShootStyle(PlayerDependencies playerDependencies, WeaponController controller, Camera mainCamera)
        {
            this.playerDependencies = playerDependencies;
            this.weaponController = controller;
            this.weapon = weaponController.Weapon;
            this.multipliers = weaponController.GetComponent<PlayerMultipliers>();
            this.weaponAnimator = weaponController.GetComponent<WeaponAnimator>();
            this.mainCamera = mainCamera;
            this.firePoint = weaponController.Id.FirePoint;
        }

        public void Shoot(float spread, float damageMultiplier, float shakeMultiplier)
        {
            if (canShoot)
                HandleHitscanProjectileShot(spread);
        }
        private void HandleHitscanProjectileShot(float spread)
        {
            if (weapon == null) return;

            canShoot = false; // since you have already shot, you will have to wait in order to being able to shoot again

            shootingCoroutine = playerDependencies.StartCoroutine(HandleShooting(spread));
            if (allowShootCoroutine != null)
            {
                playerDependencies.StopCoroutine(allowShootCoroutine);
                allowShootCoroutine = null;
            }
            playerDependencies.StartCoroutine(AllowShootAfterDelay(weapon.fireRate));

            weaponController?.SpawnBulletShells();
        }

        private IEnumerator HandleShooting(float spread)
        {
            weaponAnimator?.StopWalkAndRunMotion();

            if ((int)weapon.shootStyle == 1)
            {
                yield return new WaitForSeconds(weapon.shootDelay);
            }

            if (weapon.timeBetweenShots == 0)
            {
                // Rest the bullets that have just been shot
                weaponController?.ReduceAmmo();
            }

            // Avoid calling the while loop if we only want to shoot one bullet
            if (weapon.bulletsPerFire == 1)
            {
                if (weapon == null) yield break;
                ProjectileShoot(spread);
            }
            else
            {
                int i = 0;
                while (i < weapon.bulletsPerFire)
                {
                    if (weapon == null) yield break;

                    ProjectileShoot(spread);
                    yield return new WaitForSeconds(weapon.timeBetweenShots);
                    i++;
                }
            }

            yield break;
        }

        private void ProjectileShoot(float spread)
        {
            onShoot?.Invoke();

            RaycastHit hit;
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
            Vector3 destination = (Physics.Raycast(ray, out hit) && !hit.transform.CompareTag("Player")) ? destination = hit.point + CowsinsUtilities.GetSpreadDirection(spread, mainCamera) : destination = ray.GetPoint(50f) + CowsinsUtilities.GetSpreadDirection(spread, mainCamera);

            foreach (var p in firePoint)
            {
                GameObject bulletGO = GameObject.Instantiate(weapon.projectile.gameObject, p.position, p.transform.rotation);
                Bullet bullet = bulletGO.GetComponent<Bullet>();
                if (weapon.explosionOnHit) bullet.explosionVFX = weapon.explosionVFX;

                bullet.hurtsPlayer = weapon.hurtsPlayer;
                bullet.explosionOnHit = weapon.explosionOnHit;
                bullet.explosionRadius = weapon.explosionRadius;
                bullet.explosionForce = weapon.explosionForce;

                bullet.criticalMultiplier = weapon.criticalDamageMultiplier;
                bullet.destination = destination;
                bullet.player = weaponController.transform;
                bullet.speed = weapon.speed;
                bullet.GetComponent<Rigidbody>().isKinematic = (!weapon.projectileUsesGravity) ? true : false;
                bullet.damage = weaponController.Id.damage * multipliers.damageMultiplier;
                bullet.duration = weapon.bulletDuration;
            }
        }

        private IEnumerator AllowShootAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            canShoot = true;
        }

        public void SetOnShootEvent(Action @event) => onShoot = @event;
    }
}