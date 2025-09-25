using UnityEngine;
using System.Collections;
using System;

namespace cowsins
{
    /// <summary>
    /// When a weapon is set to Melee Shoot Style, its shootBehaviour in WeaponIdentification is assigned to this MeleeShootStyle.
    /// This is done when the weapon is unholstered.
    /// </summary>
    public class MeleeShootStyle : IShootStyle
    {
        Weapon_SO weapon;
        WeaponIdentification id;
        PlayerDependencies playerDependencies;
        private Transform[] firePoint;
        Camera mainCamera;
        LayerMask hitLayer;

        private bool canShoot = true;

        // Calls events in Weapon Controller when shooting or hitting an enemy
        public event Action onShoot;
        public event Action<int, float, RaycastHit, bool> onHit;

        public MeleeShootStyle(PlayerDependencies playerDependencies, Camera camera, LayerMask hitLayer)
        {
            this.playerDependencies = playerDependencies;
            IWeaponReferenceProvider weaponReference = playerDependencies.WeaponReference;
            this.weapon = weaponReference.Weapon;
            this.id = weaponReference.Id;
            this.firePoint = id.FirePoint;
            this.mainCamera = camera;
            this.hitLayer = hitLayer;
        }

        public void Shoot(float spread, float damageMultiplier, float shakeMultiplier)
        {
            if (canShoot)
                playerDependencies.StartCoroutine(HandleMeleeShotCoroutine(damageMultiplier, shakeMultiplier));
        }

        private IEnumerator HandleMeleeShotCoroutine(float damageMultiplier, float shakeMultiplier)
        {
            canShoot = false;

            int randomIndex = UnityEngine.Random.Range(1, weapon.amountOfShootAnimations + 1);
            // Generate the animation name based on the random index
            string randomAnimation = "shooting" + (randomIndex == 1 ? "" : randomIndex.ToString());
            // Get the Animator component from the current weapon
            Animator animator = id.Animator;
            // Play the selected random animation
            CowsinsUtilities.ForcePlayAnim(randomAnimation, animator);

            SoundManager.Instance.PlaySound(id.GetFireSFX(), 0, weapon.pitchVariationFiringSFX, true);

            if (weapon == null) yield break;

            yield return new WaitForSeconds(weapon.hitDelay);

            Melee(weapon, 0, damageMultiplier);
            playerDependencies.StartCoroutine(AllowShootAfterDelay(weapon.attackRate));

            // OnShoot() inside WeaponController is subscribed to this onShoot method.
            // When onShoot is called, OnShoot() inside WeaponController is called. Mainly used for Camera Effects.
            onShoot?.Invoke();
        }

        private void Melee(Weapon_SO weapon, float spread, float damageMultiplier)
        {
            RaycastHit hit;
            Vector3 basePosition = id != null ? id.transform.position : playerDependencies.transform.position;
            Collider[] col = Physics.OverlapSphere(basePosition + mainCamera.transform.parent.forward * weapon.attackRange / 2, weapon.attackRange, hitLayer);

            float dmg = weapon.damagePerHit * damageMultiplier;

            foreach (var c in col)
            {
                if (c.CompareTag("Critical") || c.CompareTag("BodyShot"))
                {
                    CowsinsUtilities.GatherDamageableParent(c.transform).Damage(dmg, false);
                    break;
                }

                IDamageable damageable = c.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.Damage(dmg, false);
                    break;
                }
            }

            //VISUALS
            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            if (Physics.Raycast(ray, out hit, weapon.attackRange, hitLayer))
            {
                // Hit() inside WeaponController is subscribed to this onHit method.
                // When onHit is called, Hit() inside WeaponController is called.
                onHit?.Invoke(hit.collider.gameObject.layer, 0f, hit, false);
            }
        }

        private IEnumerator AllowShootAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            canShoot = true;
        }

        public void SetOnShootEvent(Action @event) => onShoot = @event;
        public void SetOnHitEvent(Action<int, float, RaycastHit, bool> @event) => onHit = @event;
    }
}