using UnityEngine;
using System.Collections.Generic; 

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.Animations;
#endif
using System.IO;

namespace cowsins
{
    public static class CowsinsUtilities
    {
        /// <summary>
        /// Returns a Vector3 that applies spread to the bullets shot
        /// </summary>
        public static Vector3 GetSpreadDirection(float amount, Camera camera)
        {
            float horSpread = UnityEngine.Random.Range(-amount, amount);
            float verSpread = UnityEngine.Random.Range(-amount, amount);
            Vector3 spread = camera.transform.InverseTransformDirection(new Vector3(horSpread, verSpread, 0));
            Vector3 dir = camera.transform.forward + spread;

            return dir;
        }
        public static void PlayAnim(string anim, Animator animator)
        {
            animator.SetTrigger(anim);
        }

        public static void ForcePlayAnim(string anim, Animator animator)
        {
            animator.Play(anim, 0, 0);
        }
        public static void StartAnim(string anim, Animator animated) => animated.SetBool(anim, true);

        public static void StopAnim(string anim, Animator animated) => animated.SetBool(anim, false);
#if UNITY_EDITOR
        public static void SavePreset(UnityEngine.Object source, string name)
        {
            if (EmptyString(name))
            {
                Debug.LogError("ERROR: Do not forget to give your preset a name!");
                return;
            }
            Preset preset = new Preset(source);

            string directoryPath = "Assets/" + "Cowsins/" + "CowsinsPresets/";

            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            string fullPath = directoryPath + name + ".preset";
            AssetDatabase.CreateAsset(preset, fullPath);
            Debug.Log($"Preset successfully saved in {fullPath}");
        }
        public static void ApplyPreset(Preset preset, UnityEngine.Object target)
        {
            preset.ApplyTo(target);
        }

        public static bool IsUsingUnity6()
        {
            string unityVersion = Application.unityVersion;
            return unityVersion.StartsWith("6"); 
        }

#endif
        public static bool EmptyString(string string_)
        {
            if (string_.Length == 0) return true;
            int i = 0;
            while (i < string_.Length)
            {
                if (string_[i].ToString() == " ") return true;
                i++;
            }
            return false;
        }

        public static IDamageable GatherDamageableParent(Transform child)
        {
            for (Transform parent = child.parent; parent != null; parent = parent.parent)
            {
                if (parent.TryGetComponent(out IDamageable component))
                {
                    return component;
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if the attachment is compatible with the current unholstered weapon
        /// </summary>
        /// <param name="weapon">Weapon to check compatibility</param>
        /// <returns></returns>
        public static (bool found, Attachment attachment, int index) CompatibleAttachment(WeaponIdentification weaponIdentification, AttachmentIdentifier_SO identifier)
        {
            if (weaponIdentification == null) return (false, null, -1);

            Weapon_SO weapon = weaponIdentification.weapon;

            if (weapon?.weaponObject == null || identifier == null)
                return (false, null, -1);

            var compatible = weaponIdentification.compatibleAttachments;

            foreach (AttachmentType type in System.Enum.GetValues(typeof(AttachmentType)))
            {
                IReadOnlyList<Attachment> attachments = compatible.GetCompatible(type);

                for (int i = 0; i < attachments.Count; i++)
                {
                    if (attachments[i]?.attachmentIdentifier == identifier)
                        return (true, attachments[i], i);
                }
            }

            return (false, null, -1);
        }


#if UNITY_EDITOR
        public static (bool, float) CheckClipAvailability(Animator animator, string stateName)
        {
            AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
            if (controller == null) return (false, 0f);

            foreach (var layer in controller.layers)
            {
                foreach (var childState in layer.stateMachine.states)
                {
                    if (childState.state.name == stateName)
                    {
                        AnimationClip clip = childState.state.motion as AnimationClip;
                        if (clip != null)
                        {
                            return (true, clip.length);
                        }
                        return (false, 0f);
                    }
                }
            }

            return (false, 0f);
        }
#endif
    }
}
