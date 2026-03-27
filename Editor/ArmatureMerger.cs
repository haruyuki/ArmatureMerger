#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace blumewmew.Tools
{
    public class ArmatureMerger : EditorWindow
    {
        private GameObject avatarObject;
        private GameObject clothingObject;

        private Transform avatarRoot;
        private Transform clothingRoot;

        private bool autoDetectSuffix = true;
        private string clothingSuffix = "";
        private bool renameMergedBones = true;

        private bool unpackPrefabIfNeeded = true;

        private string statusMessage = "";
        private bool configurationValid;

        private bool showAdvancedOptions;

        private Regex suffixRegex;
        private string cachedSuffixForRegex;

        [MenuItem("Tools/Armature Merger")]
        private static void OpenWindow()
        {
            ArmatureMerger window = GetWindow<ArmatureMerger>();
            window.minSize = new Vector2(500, 450);
            window.Show();
        }

        private void OnEnable()
        {
            statusMessage = "Drag your avatar and clothing objects. The tool will auto-detect their armatures and suffix.";
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Armature Merger", EditorStyles.boldLabel);

            using (EditorGUI.ChangeCheckScope scope = new EditorGUI.ChangeCheckScope())
            {
                DrawMainFields();
                DrawAdvancedOptions();

                if (scope.changed)
                    ValidateConfiguration();
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.HelpBox(statusMessage,
                configurationValid ? MessageType.Info : MessageType.Warning);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reset", GUILayout.Width(80)))
                    ResetFields();

                GUI.enabled = configurationValid;
                if (GUILayout.Button("Merge", GUILayout.Width(80)))
                    ExecuteMerge();
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawMainFields()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Input Objects", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            avatarObject = EditorGUILayout.ObjectField("Avatar Object", avatarObject, typeof(GameObject), true) as GameObject;
            clothingObject = EditorGUILayout.ObjectField("Clothing Object", clothingObject, typeof(GameObject), true) as GameObject;
            if (EditorGUI.EndChangeCheck())
            {
                AutoDetectSuffixFromObjectNames();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Armature Roots (auto-detected, can override)", EditorStyles.boldLabel);

            avatarRoot = EditorGUILayout.ObjectField("Avatar Root", avatarRoot, typeof(Transform), true) as Transform;
            clothingRoot = EditorGUILayout.ObjectField("Clothing Root", clothingRoot, typeof(Transform), true) as Transform;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Naming", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            autoDetectSuffix = EditorGUILayout.ToggleLeft("Auto‑detect suffix", autoDetectSuffix, GUILayout.Width(120));
            if (autoDetectSuffix)
            {
                GUI.enabled = false; // read-only when auto-detect is on
                EditorGUILayout.TextField(clothingSuffix);
                GUI.enabled = true;
            }
            else
            {
                clothingSuffix = EditorGUILayout.TextField(clothingSuffix);
            }
            EditorGUILayout.EndHorizontal();

            renameMergedBones = EditorGUILayout.ToggleLeft("Rename merged bones (append suffix)", renameMergedBones);
        }

        private void DrawAdvancedOptions()
        {
            EditorGUILayout.Space();
            showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Advanced Options", true);
            if (!showAdvancedOptions) return;
            EditorGUI.indentLevel++;
            unpackPrefabIfNeeded = EditorGUILayout.ToggleLeft("Automatically unpack clothing prefab if needed", unpackPrefabIfNeeded);
            EditorGUI.indentLevel--;
        }

        private void ResetFields()
        {
            avatarObject = null;
            clothingObject = null;
            avatarRoot = null;
            clothingRoot = null;
            clothingSuffix = "";
            autoDetectSuffix = true;
            configurationValid = false;
            statusMessage = "Fields reset.";
        }

        private void AutoDetectSuffixFromObjectNames()
        {
            if (avatarObject == null || clothingObject == null)
            {
                autoDetectSuffix = true;
                clothingSuffix = "";
                return;
            }

            string avatarName = avatarObject.name;
            string clothName = clothingObject.name;

            // Case‑insensitive check
            if (clothName.StartsWith(avatarName, System.StringComparison.OrdinalIgnoreCase))
            {
                string remainder = clothName.Substring(avatarName.Length);
                // Remove leading separators (underscore, hyphen, whitespace)
                remainder = remainder.TrimStart(' ', '_', '-');
                if (!string.IsNullOrEmpty(remainder))
                {
                    clothingSuffix = remainder;
                    autoDetectSuffix = true;
                    return;
                }
            }

            // No valid suffix detected
            clothingSuffix = "";
            autoDetectSuffix = false;
        }

        private void ValidateConfiguration()
        {
            if (avatarObject == null || clothingObject == null)
            {
                configurationValid = false;
                statusMessage = "Please assign both Avatar Object and Clothing Object.";
                return;
            }

            if (avatarRoot == null)
                avatarRoot = FindArmatureRoot(avatarObject);
            if (clothingRoot == null)
                clothingRoot = FindArmatureRoot(clothingObject);

            if (avatarRoot == null)
            {
                configurationValid = false;
                statusMessage = "Could not find an armature root in the Avatar Object. Please assign manually.";
                return;
            }
            if (clothingRoot == null)
            {
                configurationValid = false;
                statusMessage = "Could not find an armature root in the Clothing Object. Please assign manually.";
                return;
            }

            if (avatarRoot == clothingRoot ||
                clothingRoot.IsChildOf(avatarRoot) ||
                avatarRoot.IsChildOf(clothingRoot))
            {
                configurationValid = false;
                statusMessage = "Avatar and clothing armatures must be separate hierarchies.";
                return;
            }

            // Validate suffix
            if (autoDetectSuffix)
            {
                if (string.IsNullOrEmpty(clothingSuffix))
                {
                    autoDetectSuffix = false;
                    configurationValid = false;
                    statusMessage = "Auto-detection could not find a suffix. Please enter one manually or adjust object names.";
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(clothingSuffix))
                {
                    configurationValid = false;
                    statusMessage = "Please enter a suffix (e.g., 'Cloth', 'Dress') for the clothing.";
                    return;
                }
            }

            configurationValid = true;
            statusMessage = "Configuration valid. Press 'Merge' to perform the operation.";
        }

        private Transform FindArmatureRoot(GameObject obj)
        {
            if (obj == null) return null;

            string[] commonNames = { "Armature", "Hips", "Root", "Skeleton" };
            Transform best = null;
            int maxChildren = 0;

            foreach (Transform t in obj.GetComponentsInChildren<Transform>(true))
            {
                if (commonNames.Any(boneName => string.Equals(t.name, boneName, System.StringComparison.OrdinalIgnoreCase)))
                {
                    return t;
                }

                if (t.childCount <= maxChildren) continue;
                maxChildren = t.childCount;
                best = t;
            }
            return best;
        }

        // --------------------------------------------------------------------
        // Core merge logic
        // --------------------------------------------------------------------
        private void ExecuteMerge()
        {
            if (unpackPrefabIfNeeded && PrefabUtility.IsPartOfPrefabInstance(clothingRoot))
            {
                GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(clothingRoot);
                if (!EditorUtility.DisplayDialog("Unpack Prefab?",
                        "The clothing object is part of a prefab instance. Modifying its hierarchy requires unpacking.\n\nUnpack now?",
                        "Unpack", "Cancel"))
                {
                    return;
                }

                Undo.RegisterFullObjectHierarchyUndo(root, "Unpack Prefab");
                PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.UserAction);
            }

            Undo.RegisterFullObjectHierarchyUndo(clothingRoot.gameObject, "Armature Merge");
            Undo.RegisterFullObjectHierarchyUndo(avatarRoot.gameObject, "Armature Merge (avatar)");

            RebuildSuffixRegex();

            MergeBone(clothingRoot, avatarRoot);

            Debug.Log("Armature merge completed.");
        }

        private void RebuildSuffixRegex()
        {
            if (string.IsNullOrEmpty(clothingSuffix))
            {
                suffixRegex = null;
                cachedSuffixForRegex = null;
                return;
            }

            if (cachedSuffixForRegex == clothingSuffix) return;
            suffixRegex = new Regex(@"[_-]?" + Regex.Escape(clothingSuffix) + "$", RegexOptions.Compiled);
            cachedSuffixForRegex = clothingSuffix;
        }

        private void MergeBone(Transform clothingBone, Transform avatarParent)
        {

            // Snapshot children BEFORE reparenting so the list isn't affected by hierarchy changes
            List<Transform> children = new List<Transform>(clothingBone.childCount);
            for (int i = 0; i < clothingBone.childCount; i++)
                children.Add(clothingBone.GetChild(i));

            // Rename if desired
            if (renameMergedBones)
            {
                string baseName = StripSuffix(clothingBone.name);
                if (!string.IsNullOrEmpty(baseName))
                {
                    Undo.RecordObject(clothingBone, "Rename Bone");
                    clothingBone.name = baseName + (string.IsNullOrEmpty(clothingSuffix) ? "" : "_" + clothingSuffix);
                }
            }

            Undo.RecordObject(clothingBone, "Reparent Bone");
            clothingBone.SetParent(avatarParent, true);

            foreach (Transform child in children)
            {
                Transform avatarMatch = FindMatchingAvatarBone(child, avatarParent);
                if (avatarMatch != null)
                {
                    MergeBone(child, avatarMatch);
                }
                else
                {
                    Debug.LogWarning($"No matching avatar bone for '{child.name}' under '{avatarParent.name}'. Bone will remain under its current parent.");
                }
            }
        }

        private Transform FindMatchingAvatarBone(Transform clothingBone, Transform avatarParent)
        {
            string baseName = StripSuffix(clothingBone.name);
            if (string.IsNullOrEmpty(baseName))
                baseName = clothingBone.name;

            Transform match = avatarParent.Find(baseName);
            return match != null ? match :
                // Fallback to exact name
                avatarParent.Find(clothingBone.name);
        }

        private string StripSuffix(string clothingName)
        {
            return suffixRegex == null ? clothingName : suffixRegex.Replace(clothingName, "");
        }
    }
}
#endif