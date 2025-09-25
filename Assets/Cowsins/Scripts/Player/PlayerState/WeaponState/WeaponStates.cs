using UnityEngine;
namespace cowsins
{
    public class WeaponStates : MonoBehaviour
    {
        WeaponBaseState _currentState;
        WeaponStateFactory _states;

        public WeaponBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }
        public WeaponStateFactory _States { get { return _states; } set { _states = value; } }

        // Shared references stored in context to avoid repeated GetComponent calls across states.
        public PlayerDependencies PlayerDependencies { get; private set; }
        public IPlayerControlProvider PlayerControlProvider { get; private set; } // IPlayerControlProvider is implemented in PlayerControl.cs
        public IPlayerMovementStateProvider PlayerMovement { get; private set; } // IPlayerMovementStateProvider is implemented in PlayerMovement.cs
        public WeaponController WeaponController { get; private set; }
        public WeaponAnimator WeaponAnimator { get; private set; }
        public InteractManager InteractManager { get; private set; }

        public bool holding = false;

        private void Start()
        {
            GetContextReferences();

            _states = new WeaponStateFactory(this);
            _currentState = _states.Default();
            _currentState.EnterState();

            InputManager.onStopShoot += TriggerStopHolding;
            InteractManager.events.onDrop.AddListener(OnDropReset);
            WeaponController.events.OnUnholsterWeapon.AddListener(OnUnholster);
        }

        private void OnDisable()
        {
            InputManager.onStopShoot -= TriggerStopHolding;
            InteractManager.events.onDrop.RemoveListener(OnDropReset);
            WeaponController.events.OnUnholsterWeapon.RemoveListener(OnUnholster);
        }

        private void Update()
        {
            _currentState.UpdateState();
        }

        private void FixedUpdate()
        {
            _currentState.FixedUpdateState();
        }

        /// <summary>
        /// Force to change a Weapon state by passing the desired new state.
        /// </summary>
        /// <param name="newState"></param>
        public void ForceChangeState(WeaponBaseState newState)
        {
            _currentState.ExitState();
            _currentState = newState;
            _currentState.EnterState();
        }

        private void GetContextReferences()
        {
            PlayerDependencies = GetComponent<PlayerDependencies>();
            PlayerControlProvider = PlayerDependencies.PlayerControl;
            PlayerMovement = PlayerDependencies.PlayerMovementState;
            WeaponController = GetComponent<WeaponController>();
            WeaponAnimator = GetComponent<WeaponAnimator>();
            InteractManager = GetComponent<InteractManager>();
        }
        private void TriggerStopHolding() => holding = false;

        private void OnDropReset(Pickeable dummyPickeable)
        {
            ForceChangeState(_states.Default());
        }
        private void OnUnholster() => ForceChangeState(_states.Unholster());
    }
}