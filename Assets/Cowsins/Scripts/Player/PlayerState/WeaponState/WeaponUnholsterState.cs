using UnityEngine;

namespace cowsins
{
    // Called when a weapon is unholstered.
    public class WeaponUnholsterState : WeaponBaseState
    {
        private float timer;
        private bool isDefaultReload;

        private IPlayerControlProvider playerControl; // Reference to PlayerControl.cs ( IPlayerControlProvider is implemented in PlayerControl.cs )
        // Weapon FSM ( Finite State Machine ) is dependant on WeaponController.
        private WeaponController controller;

        public WeaponUnholsterState(WeaponStates currentContext, WeaponStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) 
        {
            playerControl = _ctx.PlayerControlProvider;
            controller = _ctx.WeaponController;
        }

        public sealed override void EnterState()
        {
            // Reset timer to ensure the timing is tracked accordingly.
            timer = 0;
            isDefaultReload = controller.Weapon.reloadStyle == ReloadingStyle.defaultReload;
        }

        public sealed override void UpdateState()
        {
            if (timer < .5f) timer += Time.deltaTime;
            CheckSwitchState();

            if (!playerControl.IsControllable)
            {
                controller.StopAim();
                return;
            }
            controller.HandleInventory();
        }

        public sealed override void FixedUpdateState() { }

        public sealed override void ExitState() {}

        public sealed override void CheckSwitchState()
        {
            if (timer >= .5f) SwitchState(_factory.Default());

            if (controller.AllowReloadWhileUnholstering && isDefaultReload && CanSwitchToReload(controller))
                SwitchState(_factory.Reload());
        }

        private bool CanSwitchToReload(WeaponController controller)
        {
            return InputManager.reloading && (int)controller.Weapon.shootStyle != 2 && controller.Id.bulletsLeftInMagazine < controller.Id.magazineSize && controller.Id.totalBullets > 0
                        || controller.Id.bulletsLeftInMagazine <= 0 && controller.AutoReload && (int)controller.Weapon.shootStyle != 2 && controller.Id.bulletsLeftInMagazine < controller.Id.magazineSize && controller.Id.totalBullets > 0;
        }
    }
}