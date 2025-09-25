#if UNITY_EDITOR
/// <summary>
/// This script belongs to cowsins� as a part of the cowsins� FPS Engine. All rights reserved. 
/// </summary>
using UnityEditor;
using UnityEngine;

namespace cowsins
{
    [System.Serializable]
    [CustomEditor(typeof(WeaponController))]
    public class WeaponControllerEditor : Editor
    {
        private string[] tabs = { "Inventory", "References", "Variables", "Secondary Attack", "Effects", "Events" };
        private int currentTab = 0;

        override public void OnInspectorGUI()
        {
            serializedObject.Update();
            WeaponController myScript = target as WeaponController;


            Texture2D myTexture = Resources.Load<Texture2D>("CustomEditor/weaponController_CustomEditor") as Texture2D;
            GUILayout.Label(myTexture);

            EditorGUILayout.BeginVertical();
            currentTab = GUILayout.Toolbar(currentTab, tabs);
            EditorGUILayout.Space(10f);
            EditorGUILayout.EndVertical();

            if (currentTab >= 0 || currentTab < tabs.Length)
            {
                switch (tabs[currentTab])
                {
                    case "Inventory":
                        EditorGUILayout.LabelField("INVENTORY", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("inventorySize"));
                        EditorGUILayout.Space(5);
                        EditorGUILayout.LabelField("Select the weapons you want to spawn with", EditorStyles.helpBox);
                        EditorGUILayout.Space(5);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("initialWeapons"));
                        if (myScript.initialWeapons.Length > myScript.InventorySize) myScript.initialWeapons = new Weapon_SO[myScript.InventorySize];
                        if (myScript.initialWeapons.Length == myScript.InventorySize) EditorGUILayout.LabelField("You can�t add more initial weapons. This array can�t be bigger than the inventory size", EditorStyles.helpBox);
                        break;
                    case "References":
                        EditorGUILayout.LabelField("REFERENCES", EditorStyles.boldLabel);
                        //var weaponProperty = serializedObject.FindProperty("weapon");
                        //EditorGUILayout.PropertyField(weaponProperty);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("mainCamera"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraPivot"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponHolder"));
                        break;
                    case "Variables":
                        EditorGUILayout.Space(10f);
                        EditorGUILayout.LabelField("VARIABLES", EditorStyles.boldLabel);
                        EditorGUILayout.Space(2f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("resizeCrosshair"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("autoReload"));
                        if(myScript.AutoReload)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoReloadDelay")); 
                            EditorGUI.indentLevel--;
                        }
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("allowReloadWhileUnholstering"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("alternateAiming"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("removeCrosshairOnAiming"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("hitLayer"));
                        break;
                    case "Secondary Attack":
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("canMelee"));
                        if (myScript.CanMelee)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeObject"));
                            if(myScript.MeleeObject)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.LabelField("You can leave �meleeHeadBone� unassigned if your camera does not move during your Melee Animations.", EditorStyles.helpBox);
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeHeadBone"));
                                EditorGUI.indentLevel--;
                            }
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeDelay"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeAttackDamage"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeRange"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeCamShakeAmount"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("reEnableMeleeAfterAction"));
                            if (myScript.meleeDelay > 1) myScript.meleeDelay = 0f;
                            EditorGUI.indentLevel--;
                        }
                        break;
                    case "Effects":
                        EditorGUILayout.Space(2f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("effects"));
                        EditorGUILayout.Space(2f);
                        break;
                    case "Events":
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("events"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("customPrimaryShot"));
                        break;
                }
                EditorGUILayout.Space(10f);

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif