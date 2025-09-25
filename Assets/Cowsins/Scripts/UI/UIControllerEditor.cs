#if UNITY_EDITOR
using UnityEditor;
using UnityEngine; 

namespace cowsins {
    [System.Serializable]
    [CustomEditor(typeof(UIController))]
    public class UIControllerEditor : Editor
    {
        private string[] tabs = { "Health", "Interaction", "Attachments", "Weapon", "Dashing", "Experience", "Others", "UI Events" };
        private int currentTab = 0;

        override public void OnInspectorGUI()
        {
            serializedObject.Update();
            UIController myScript = target as UIController;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(10f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pauseMenu"));
            EditorGUILayout.Space(10f);
            currentTab = GUILayout.Toolbar(currentTab, tabs);
            EditorGUILayout.Space(10f);
            EditorGUILayout.EndVertical();


            if (currentTab >= 0 || currentTab < tabs.Length)
            {
                switch (tabs[currentTab])
                {
                    case "Health":
                        EditorGUILayout.LabelField("HEALTH AND SHIELD", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("barHealthDisplay"));
                        if (myScript.BarHealthDisplay)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("healthSlider"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("shieldSlider"));
                            EditorGUI.indentLevel--;
                        }
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("numericHealthDisplay"));

                        if (myScript.NumericHealthDisplay)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("healthTextDisplay"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("shieldTextDisplay"));
                            EditorGUI.indentLevel--;
                        }

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("healthStatesEffect"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("damageColor"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("healColor"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("coinCollectColor"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("xpCollectColor"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeOutTime"));

                        break;
                    case "Interaction":

                        EditorGUILayout.LabelField("INTERACTION", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("interactUI"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("allowedInteractionSFX"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("interactUIProgressDisplay"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("forbiddenInteractionUI"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("interactText"));

                        break;
                    case "Attachments":
                        EditorGUILayout.LabelField("ATTACHMENTS", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("inspectionUI"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("inspectionFadeDuration"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponDisplayText_AttachmentsUI"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("attachmentDisplay_UIElement"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("barrels_AttachmentsGroup"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("scopes_AttachmentsGroup"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("stocks_AttachmentsGroup"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("grips_AttachmentsGroup"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("magazines_AttachmentsGroup"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("flashlights_AttachmentsGroup"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("lasers_AttachmentsGroup"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("usingAttachmentColor"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("notUsingAttachmentColor"));
                        break;
                    case "Weapon":

                        EditorGUILayout.LabelField("WEAPON", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletsUI"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("magazineUI"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("overheatUI"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadUI"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("lowAmmoUI"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("currentWeaponDisplay"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("inventoryContainer"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("inventoryUISlot"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("crosshair"));
                        break;
                    case "Dashing":

                        EditorGUILayout.LabelField("DASHING", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashUIContainer"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashUIElement"));
                        EditorGUI.indentLevel--;

                        break;
                    case "Experience":
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("xpImage"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("currentLevel"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("nextLevel"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("lerpXpSpeed"));

                        break;
                    case "Others":
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("coinsUI"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("coinsText"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("hitmarker"));
                        break;
                    case "UI Events":

                        EditorGUILayout.LabelField("EVENTS", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("displayEvents"));
                        if (myScript.DisplayEvents)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("killfeedContainer"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("killfeedObject"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("killfeedMessage"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("damagePopUp"));
                            if(myScript.DamagePopUp)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("xVariation"));
                                EditorGUI.indentLevel--;
                            }
                            EditorGUI.indentLevel--;
                        }

                        break;
                }
            }
            serializedObject.ApplyModifiedProperties();

        }
    }
}
#endif