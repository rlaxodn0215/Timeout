/// <summary>
/// This script belongs to cowsins� as a part of the cowsins� FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
#if INVENTORY_PRO_ADD_ON
using cowsins.Inventory;
#endif

namespace cowsins
{
    public class InteractManager : MonoBehaviour, IInteractManagerProvider
    {
        [System.Serializable]
        public class Events
        {
            public UnityEvent OnFinishedInteraction;
            public UnityEvent<Pickeable> onDrop;
            public UnityEvent onDropWeapon;
        }

        [Tooltip("Attach your main camera"), SerializeField] private Camera mainCamera;

        [Tooltip("Bitmask that defines the interactable layer"), SerializeField] private LayerMask mask;

        [Tooltip("Enable this toggle if you want to be able to drop your weapons"), SerializeField] private bool canDrop;

        [Tooltip("Attach the generic pickeable object here"), SerializeField] private Pickeable weaponGenericPickeable;

        [Tooltip("Attach the generic pickeable object here"), SerializeField] private Pickeable attachmentGenericPickeable;

        [Tooltip("Distance from the player to detect interactable objects"), SerializeField] private float detectInteractionDistance;

        [Tooltip("Distance from the player where the pickeable will be instantiated"), SerializeField] private float droppingDistance;

        [Tooltip("Randomize drop offset (from -randomDropOffset to +randomDropOffset)"), SerializeField, Range(0f,1f)] private float randomDropOffset = .2f;

        [SerializeField, Tooltip("How much time player has to hold the interact button in order to successfully interact")] private float progressRequiredToInteract;

        [Tooltip("Adjust the interaction interval, the lower, the faster you will be able to interact"), Range(.2f, .7f), SerializeField] private float interactInterval = .4f;

        [Tooltip("When picking up a duplicate weapon, if duplicateWeaponAddsBullets is true, the bullets will be added to the total count of the current weapon instead of creating a new instance of the same weapon. " +
            "This feature is only applicable to weapons with limited magazines."), SerializeField]
        private bool duplicateWeaponAddsBullets;

        [Tooltip("If true, the player will be able to inspect the current weapon."), SerializeField] private bool canInspect;

        [Tooltip("Allows the player to equip and unequip attachments while inspecting. It also displays a custom UI for that."), SerializeField] private bool realtimeAttachmentCustomization;

        [Tooltip("When inspecting, display current attachments only. Otherwise you will be able to see all compatible attachments."), SerializeField] private bool displayCurrentAttachmentsOnly;

        [Tooltip("While Inspecting the weapon, if an attachment is dettached and this field is true, the attachment will be dropped."), SerializeField] private bool dropAttachmentOnDettachUI;

        private bool isForbiddenInteraction = false;
        private float progressElapsed;
        private bool alreadyInteracted = false;
        private bool inspecting = false;

        public float ProgressElapsed => progressElapsed;
        public bool Inspecting => inspecting;
        public bool CanDrop => canDrop;
        public float DroppingDistance => droppingDistance;
        public bool DuplicateWeaponAddsBullets => duplicateWeaponAddsBullets;
        public bool CanInspect => canInspect;   
        public bool RealtimeAttachmentCustomization => realtimeAttachmentCustomization; 
        public bool DisplayCurrentAttachmentsOnly => displayCurrentAttachmentsOnly; 
        public Interactable HighlightedInteractable => highlightedInteractable;


        private PlayerOrientation orientation;

        private PlayerDependencies playerDependencies;
        private IPlayerMovementStateProvider playerMovement; // IPlayerMovementStateProvider is implemented in PlayerMovement.cs
        private IPlayerControlProvider playerControl; // IPlayerControlProvider is implemented in PlayerControl.cs
        private IWeaponBehaviourProvider weaponController; // IWeaponBehaviourProvider is implemented in WeaponController.cs
        private IWeaponReferenceProvider weaponReferences; // IWeaponReferenceProvider is implemented in WeaponController.cs

        private Interactable highlightedInteractable;


        public Events events;

        private void OnEnable()
        {
            // Subscribe to the event
            if(realtimeAttachmentCustomization)
            {
                if (dropAttachmentOnDettachUI) UIEvents.onAttachmentUIElementClicked += DropAttachment;
                else UIEvents.onAttachmentUIElementClicked += DeactivateCurrentAttachment;
            }
        }

        private void Start()
        {
            // Grab main references
            playerDependencies = GetComponent<PlayerDependencies>();
            weaponController = playerDependencies.WeaponBehaviour;
            weaponReferences = playerDependencies.WeaponReference;
            playerMovement = playerDependencies.PlayerMovementState;
            playerControl = playerDependencies.PlayerControl;
            orientation = playerMovement.Orientation;
            mainCamera = weaponReferences.MainCamera;

            // Listen for the drop event from the InputManager
            if(canDrop)
                InputManager.onDrop += HandleDrop;
        }


        private void OnDisable()
        {
            // Unsubscribe to the event
            UIEvents.onAttachmentUIElementClicked -= DropAttachment;
            UIEvents.onAttachmentUIElementClicked -= DeactivateCurrentAttachment;
            if (canDrop) 
                InputManager.onDrop -= HandleDrop;
        }

        private void Update()
        {
            // If we already interacted, or the player is not controllable, return!
            if (alreadyInteracted || !playerControl.IsControllable) return;

            DetectInteractable();
            DetectInput();
        }
        private void DetectInteractable()
        {
            // If we got a hit from the raycast:
            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit interactableHit, detectInteractionDistance, mask))
            {
                if (highlightedInteractable == null || highlightedInteractable?.transform != interactableHit.transform)
                {
                    if (!interactableHit.collider.TryGetComponent(out Interactable interactableTarget) || highlightedInteractable != null)
                    {
                        DisableInteractionUI();
                        return;
                    }
                    // Check if the interaction is forbidden
                    if (interactableTarget.IsForbiddenInteraction(weaponReferences))
                    {
                        // If it is forbidden, call the event
                        UIEvents.forbiddenInteraction?.Invoke();
                        isForbiddenInteraction = true;
                    }
                    else
                    {
                        // If its not, enable interaction UI to display an interaction
                        EnableInteractionUI(interactableTarget);
                        isForbiddenInteraction = false;
                    }
                    highlightedInteractable = interactableTarget;
                }
            }
            else if(highlightedInteractable != null) 
            {
                // If we dont find any interactable, disable interactions UI
                DisableInteractionUI();
            }
        }

        private void EnableInteractionUI(Interactable interactable)
        {
            if (interactable == null)
            {
                DisableInteractionUI();
                return;
            }
            interactable.interactable = true;
            // Current interactable is equal to the passed interactable value
            if (highlightedInteractable == interactable) return;
            highlightedInteractable = interactable;
            interactable.Highlight();
            UIEvents.allowedInteraction?.Invoke(interactable.interactText);
        }

        private void DisableInteractionUI()
        {
            if(highlightedInteractable)
            {
                highlightedInteractable.interactable = false;
                highlightedInteractable.Unhighlight();
            }
            highlightedInteractable = null;
            UIEvents.disableInteractionUI?.Invoke();
        }

        private void DetectInput()
        {
            if (highlightedInteractable == null || isForbiddenInteraction)
            {
                ResetInteractionProgress();
                return;
            }
            // If we dont detect an interactable then dont continue
            // However if we detected an interactable + we pressing the interact button, then: 
            if (InputManager.interacting)
            {
                progressElapsed += Time.deltaTime;
                if (progressRequiredToInteract > 0)
                {
                    UIEvents.onInteractionProgressChanged?.Invoke(progressElapsed / progressRequiredToInteract);
                }

                // Interact
                if (progressElapsed >= progressRequiredToInteract || highlightedInteractable.InstantInteraction && progressElapsed > 0) PerformInteraction();
            }
            else
            {
                ResetInteractionProgress();
                UIEvents.onFinishInteractionProgress?.Invoke();
            }
        }

        private void PerformInteraction()
        {
            ResetInteractionProgress();
            // prevent from spamming
            alreadyInteracted = true;
            // Perform any interaction you may like
            // Please note that classes that inherit from interactable can override the virtual void Interact()
            highlightedInteractable.Interact(this.transform);
            // Prevent from spamming but let the user interact again
            Invoke(nameof(ResetInteractTimer), interactInterval);

            events.OnFinishedInteraction.Invoke(); // Call our event

            // Manage UI
            highlightedInteractable.Unhighlight();
            highlightedInteractable = null;

            UIEvents.disableInteractionUI?.Invoke();
            UIEvents.onFinishInteractionProgress?.Invoke();
        }

        private void ResetInteractionProgress() => progressElapsed = -.01f;

        private void HandleDrop()
        {
            // Handles weapon dropping by pressing the drop button
            if (weaponReferences.Weapon == null || weaponController.Reloading || !weaponController.IsMeleeAvailable || inspecting || !playerControl.IsControllable) return;

            WeaponPickeable pick = Instantiate(weaponGenericPickeable, orientation.Position + orientation.Forward * droppingDistance + transform.right * randomDropOffset, orientation.Rotation) as WeaponPickeable;
            pick.Drop(playerDependencies, orientation);
            WeaponIdentification wp = weaponReferences.Id; 
            pick.SetPickeableAttachments(wp);

            weaponController.ReleaseCurrentWeapon();
            UIController.instance.crosshairShape.ResetCrosshairToDefault();

            events.onDrop?.Invoke(pick);
        }
        private void ResetInteractTimer() => alreadyInteracted = false;

        public void ToggleInspectionState(bool state) => inspecting = state;

        /// <summary>
        /// Drops the current attachment to the ground ( generates a new attachment pickeable )
        /// </summary>
        /// <param name="atc">Attachment to drop </param>
        /// <param name="enableDefault">Enables the default attachment when dropped if true.</param>
        public void DropAttachment(Attachment atc, bool enableDefault)
        {
            if (atc == null) return;

#if INVENTORY_PRO_ADD_ON
            // If Inventory Pro Add-On is installed and the Inventory is available in the scene, try to add to the Inventory
            if (InventoryProManager.instance != null)
            {
                TryAddAttachmentToInventory(atc.attachmentIdentifier);
            }
            else
            {
                InstantiateAttachmentPickeable(atc.attachmentIdentifier);
            }
#else
            InstantiateAttachmentPickeable(atc.attachmentIdentifier);
#endif

            DeactivateCurrentAttachment(atc, enableDefault);

            if (displayCurrentAttachmentsOnly)
                UIController.instance.GenerateInspectionUI(playerDependencies);
        }

        private void DeactivateCurrentAttachment(Attachment atc, bool enableDefault)
        {
            // Grab the current weaponidentification object.
            WeaponIdentification wId = weaponReferences.Id;

            if (wId == null) return;

            Dictionary<AttachmentType, Attachment> attachments = wId.GetCurrentAttachments();

            // Grab what type of attachment it is, returns barrel, Scope, etc...
            AttachmentType attachmentType = atc.attachmentIdentifier.attachmentType;
            // Check if any of the attachments saved in the dictionary is the same type as the attachment to drop type.
            if (attachments.ContainsKey(attachmentType))
            {
                atc.Dettach(wId);

                // Check all the attachment types 
                // This will determine which attachment type matches the dropped attachment
                Attachment defaultAttachment = wId.GetDefaultAttachment(attachmentType);
                wId.DeactivateAttachment(attachmentType);

                // If the default attachment is not null, and we should enable default attachments, assign it and enable it
                if (defaultAttachment != null && enableDefault)
                {
                    attachments[attachmentType] = defaultAttachment;
                    defaultAttachment.gameObject.SetActive(true);
                }
                else
                {
                    // Otherwise do not assign anything
                    attachments[attachmentType] = null;
                }
            }
        }
#if INVENTORY_PRO_ADD_ON
        public void TryAddAttachmentToInventory(AttachmentIdentifier_SO atcIdentifier)
        {
            (bool atcAddedToInv, int amount) = InventoryProManager.instance._GridGenerator.AddItemToInventory(atcIdentifier, 1);
            // If the attachment couldnt be added to the Inventory, drop it.
            if (!atcAddedToInv) InstantiateAttachmentPickeable(atcIdentifier);
        }
#endif
        private void InstantiateAttachmentPickeable(AttachmentIdentifier_SO atcIdentifier)
        {
            // Spawn a new pickeable.
            AttachmentPickeable pick = Instantiate(attachmentGenericPickeable, orientation.Position + orientation.Forward * droppingDistance, orientation.Rotation) as AttachmentPickeable;
            // Assign the appropriate attachment identifier to the spawned pickeable.
            pick.attachmentIdentifier = atcIdentifier;
            // Get visuals
            pick.Drop(playerDependencies, orientation);
        }
    }
}