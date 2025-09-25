/// <summary>
/// This script belongs to cowsins� as a part of the cowsins� FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace cowsins
{
    public class WeaponEffects : MonoBehaviour
    {
        #region Shared References
        [SerializeField] private Transform weaponEffectsTransform;

        private IPlayerMovementEventsProvider playerEventsProvider; // IPlayerMovementEventsProvider is implemented in PlayerMovement.cs
        private IPlayerMovementStateProvider playerMovementProvider; // IPlayerMovementStateProvider is implemented in PlayerMovement.cs
        private IWeaponBehaviourProvider weaponController; // IWeaponBehaviourProvider is implemented in WeaponController.cs
        private Rigidbody rb;

        private Vector3 basePosition;
        private Quaternion baseRotation;
        private Vector3 currentBobPos, currentJumpPos;
        private Quaternion currentBobRot, currentJumpRot;
        #endregion

        #region Bob Settings
        [Header("Bobbing")]
        [SerializeField] private Vector3 rotationMultiplier;
        [SerializeField] private float translationSpeed;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private Vector3 movementLimit;
        [SerializeField] private Vector3 bobLimit;
        [SerializeField] private float aimingMultiplier = 1f;
        [SerializeField] private float horizontalInclineMultiplier = 1f;
        [SerializeField] private float forwardInclineMultiplier = 1f;

        private float bobSpeed;
        private Vector3 bobPos;
        private Vector3 bobRot;
        #endregion

        #region Jump Motion Settings
        [Header("Jump Motion")]
        [SerializeField] private AnimationCurve jumpMotion;
        [SerializeField] private AnimationCurve groundedMotion;
        [SerializeField] private float jumpTranslationSpeed;
        [SerializeField] private float jumpRotationSpeed;
        [SerializeField] private float jumpMotionDistance = 1f;
        [SerializeField] private float jumpMotionRotationAmount = 10f;
        [SerializeField, Min(1)] private float evaluationSpeed = 5f;

        private Coroutine jumpCoroutine;
        private Vector3 jumpOffsetPosition;
        private Quaternion jumpOffsetRotation;
        #endregion

        private void Start()
        {
            playerEventsProvider = GetComponent<IPlayerMovementEventsProvider>();
            playerMovementProvider = GetComponent<IPlayerMovementStateProvider>();
            weaponController = GetComponent<IWeaponBehaviourProvider>();
            rb = GetComponent<Rigidbody>();

            basePosition = weaponEffectsTransform.localPosition;
            baseRotation = weaponEffectsTransform.localRotation;

            currentBobPos = Vector3.zero;
            currentJumpPos = Vector3.zero;
            currentBobRot = Quaternion.identity;
            currentJumpRot = Quaternion.identity;

            playerEventsProvider.AddJumpListener(OnJump);
            playerEventsProvider.AddLandListener(OnLand);
        }

        private void OnJump()
        {
            if (jumpCoroutine != null) StopCoroutine(jumpCoroutine);
            ResetJumpMotion();
            jumpCoroutine = StartCoroutine(ApplyJumpMotion(jumpMotion));
        }

        private void OnLand()
        {
            if (jumpCoroutine != null) StopCoroutine(jumpCoroutine);
            ResetJumpMotion();
            jumpCoroutine = StartCoroutine(ApplyJumpMotion(groundedMotion));
        }

        private IEnumerator ApplyJumpMotion(AnimationCurve motionCurve)
        {
            float time = 0f;

            yield return null;

            while (time < 1f)
            {
                time += Time.deltaTime * evaluationSpeed;
                float motion = motionCurve.Evaluate(time);

                jumpOffsetPosition = new Vector3(0, motion * jumpMotionDistance, 0);
                jumpOffsetRotation = Quaternion.Euler(motion * jumpMotionRotationAmount, 0, 0);

                yield return null;
            }

            // Reset after motion ends
            jumpOffsetPosition = Vector3.zero;
            jumpOffsetRotation = Quaternion.identity;
        }

        public void ResetJumpMotion()
        {
            jumpOffsetPosition = Vector3.zero;
            jumpOffsetRotation = Quaternion.identity;
        }

        private void Update()
        {
            if (playerMovementProvider.Grounded || playerMovementProvider.WallRunning)
            {
                // Only bob is played when Grounded or Wallruning
                UpdateBobbing();
            }

            ApplyCombinedEffects(); // Always Apply Jump/Bob effects
        }

        private void UpdateBobbing()
        {
            bobSpeed += Time.deltaTime * (playerMovementProvider.Grounded ? rb.linearVelocity.magnitude / 2 : 1) + 0.01f;
            float mult = weaponController.IsAiming ? aimingMultiplier : 1;

            float sin = Mathf.Sin(bobSpeed);
            float cos = Mathf.Cos(bobSpeed);

            bobPos.x = (cos * bobLimit.x * (playerMovementProvider.Grounded || playerMovementProvider.WallRunning ? 1 : 0)) - (InputManager.x * movementLimit.x);
            bobPos.y = sin * bobLimit.y;

            if (playerMovementProvider.Grounded || playerMovementProvider.WallRunning)
            {
                bobPos.y -= rb.linearVelocity.y * movementLimit.y;
            }

            bobPos.z = -InputManager.y * movementLimit.z;

            bobRot.x = InputManager.x != 0 ? rotationMultiplier.x * Mathf.Sin(2 * bobSpeed) : rotationMultiplier.x * Mathf.Sin(2 * bobSpeed) / 2;
            bobRot.y = InputManager.x != 0 ? rotationMultiplier.y * cos : 0;
            bobRot.z = InputManager.x != 0 ? rotationMultiplier.z * cos * InputManager.x : 0;

            if (InputManager.x != 0)
                bobRot.z += horizontalInclineMultiplier * InputManager.x;
            if (InputManager.y != 0)
                bobRot.x += forwardInclineMultiplier * InputManager.y;

            // Apply aiming multiplier
            bobPos *= mult;
            bobRot *= mult;
        }

        private void ApplyCombinedEffects()
        {
            // Bob Effect
            currentBobPos = Vector3.Lerp(currentBobPos, bobPos, Time.deltaTime * translationSpeed);
            currentBobRot = Quaternion.Slerp(currentBobRot, Quaternion.Euler(bobRot), Time.deltaTime * rotationSpeed);

            // Jump Effect
            currentJumpPos = Vector3.Lerp(currentJumpPos, jumpOffsetPosition, Time.deltaTime * jumpTranslationSpeed);
            currentJumpRot = Quaternion.Slerp(currentJumpRot, jumpOffsetRotation, Time.deltaTime * jumpRotationSpeed);

            // Combine both
            Vector3 offsetPos = currentBobPos + currentJumpPos;

            Vector3 finalPos = basePosition + offsetPos;
            Quaternion finalRot = baseRotation * currentBobRot * currentJumpRot;

            weaponEffectsTransform.localPosition = finalPos;
            weaponEffectsTransform.localRotation = finalRot;
        }

    }
#if UNITY_EDITOR
    [CustomEditor(typeof(WeaponEffects))]
    public class WeaponEffectsEditor : Editor
    {
        override public void OnInspectorGUI()
        {
            serializedObject.Update();
            var myScript = target as WeaponEffects;
            
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("SHARED SETTINGS", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponEffectsTransform"));

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("WEAPON BOB SETTINGS", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("translationSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("movementLimit"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bobLimit"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("aimingMultiplier"));

            // Add fields for the new inclination multipliers
            EditorGUILayout.PropertyField(serializedObject.FindProperty("horizontalInclineMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("forwardInclineMultiplier"));

            EditorGUI.indentLevel--;


            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("JUMP MOTION SETTINGS", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpMotion"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("groundedMotion"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpTranslationSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpRotationSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpMotionDistance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpMotionRotationAmount"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("evaluationSpeed"));
            EditorGUILayout.Space(5f);

            EditorGUILayout.LabelField("WEAPON SWAY SETTINGS", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Weapon Sway can be modified in each Weapon Prefab ( Root of the Weapon Prefab )", EditorStyles.helpBox);
            EditorGUILayout.Space(5f);
            if (GUILayout.Button("Go to Weapon Prefabs Folder"))
            {
                string folderPath = "Assets/Cowsins/ScriptableObjects/Weapons";
                EditorUtility.FocusProjectWindow();
                Object folder = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
                if (folder != null)
                {
                    EditorGUIUtility.PingObject(folder);
                    Selection.activeObject = folder;
                    EditorUtility.FocusProjectWindow();
                }
                else
                {
                    Debug.LogError($"Folder is empty or not found: {folderPath}. Did you remove or rename the Weapons Folder?");
                }
            }
            EditorGUILayout.Space(5f);
            serializedObject.ApplyModifiedProperties();

        }
    }
#endif
}
