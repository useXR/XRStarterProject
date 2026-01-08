using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;

namespace Gallery.Editor
{
    /// <summary>
    /// Editor tool to automatically wire up Gallery demo references.
    /// Run via menu: Tools > Gallery > Wire Up References
    /// </summary>
    public class GallerySetupWizard : EditorWindow
    {
        [MenuItem("Tools/Gallery/Wire Up References")]
        public static void WireUpReferences()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Undo.SetCurrentGroupName("Wire Up Gallery References");
            int undoGroup = Undo.GetCurrentGroup();

            int successCount = 0;
            int failCount = 0;

            // 1. Wire up AudioManager
            if (WireUpAudioManager()) successCount++; else failCount++;

            // 2. Wire up UIManager
            if (WireUpUIManager()) successCount++; else failCount++;

            // 3. Wire up InfoPanel
            if (WireUpInfoPanel()) successCount++; else failCount++;

            // 4. Wire up Pedestals
            var pedestalResults = WireUpPedestals();
            successCount += pedestalResults.success;
            failCount += pedestalResults.fail;

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            // Report results
            if (failCount == 0)
            {
                Debug.Log($"<color=green>[Gallery Setup] All {successCount} references wired successfully!</color>");
            }
            else
            {
                Debug.LogWarning($"[Gallery Setup] Wired {successCount} references, {failCount} failed. Check warnings above.");
            }
        }

        private static bool WireUpAudioManager()
        {
            var audioManager = Object.FindFirstObjectByType<AudioManager>();
            if (audioManager == null)
            {
                Debug.LogWarning("[Gallery Setup] AudioManager not found in scene.");
                return false;
            }

            // Find the SFXLibrary asset
            string[] guids = AssetDatabase.FindAssets("t:SFXLibrary", new[] { "Assets/_Project/ScriptableObjects" });
            if (guids.Length == 0)
            {
                Debug.LogWarning("[Gallery Setup] SFXLibrary asset not found in Assets/_Project/ScriptableObjects.");
                return false;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var sfxLibrary = AssetDatabase.LoadAssetAtPath<SFXLibrary>(path);

            Undo.RecordObject(audioManager, "Set SFXLibrary");
            var serializedObject = new SerializedObject(audioManager);
            var sfxLibraryProp = serializedObject.FindProperty("sfxLibrary");
            sfxLibraryProp.objectReferenceValue = sfxLibrary;
            serializedObject.ApplyModifiedProperties();

            Debug.Log($"[Gallery Setup] AudioManager: Assigned {sfxLibrary.name}");
            return true;
        }

        private static bool WireUpUIManager()
        {
            var uiManager = Object.FindFirstObjectByType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogWarning("[Gallery Setup] UIManager not found in scene.");
                return false;
            }

            var infoPanel = Object.FindFirstObjectByType<InfoPanel>();
            if (infoPanel == null)
            {
                Debug.LogWarning("[Gallery Setup] InfoPanel not found in scene.");
                return false;
            }

            Undo.RecordObject(uiManager, "Set InfoPanel");
            var serializedObject = new SerializedObject(uiManager);
            var infoPanelProp = serializedObject.FindProperty("infoPanel");
            infoPanelProp.objectReferenceValue = infoPanel;
            serializedObject.ApplyModifiedProperties();

            Debug.Log($"[Gallery Setup] UIManager: Assigned {infoPanel.name}");
            return true;
        }

        private static bool WireUpInfoPanel()
        {
            var infoPanel = Object.FindFirstObjectByType<InfoPanel>();
            if (infoPanel == null)
            {
                Debug.LogWarning("[Gallery Setup] InfoPanel not found in scene.");
                return false;
            }

            var serializedObject = new SerializedObject(infoPanel);
            bool success = true;

            // Find TitleText
            var titleText = infoPanel.transform.Find("TitleText")?.GetComponent<TMP_Text>();
            if (titleText != null)
            {
                Undo.RecordObject(infoPanel, "Set TitleText");
                var titleProp = serializedObject.FindProperty("titleText");
                titleProp.objectReferenceValue = titleText;
                Debug.Log("[Gallery Setup] InfoPanel: Assigned TitleText");
            }
            else
            {
                Debug.LogWarning("[Gallery Setup] TitleText not found under InfoPanel.");
                success = false;
            }

            // Find DescriptionText
            var descriptionText = infoPanel.transform.Find("DescriptionText")?.GetComponent<TMP_Text>();
            if (descriptionText != null)
            {
                Undo.RecordObject(infoPanel, "Set DescriptionText");
                var descProp = serializedObject.FindProperty("descriptionText");
                descProp.objectReferenceValue = descriptionText;
                Debug.Log("[Gallery Setup] InfoPanel: Assigned DescriptionText");
            }
            else
            {
                Debug.LogWarning("[Gallery Setup] DescriptionText not found under InfoPanel.");
                success = false;
            }

            serializedObject.ApplyModifiedProperties();
            return success;
        }

        private static (int success, int fail) WireUpPedestals()
        {
            int success = 0;
            int fail = 0;

            // Load all ExhibitData assets
            var exhibitDataMap = new System.Collections.Generic.Dictionary<string, ExhibitData>();
            string[] guids = AssetDatabase.FindAssets("t:ExhibitData", new[] { "Assets/_Project/ScriptableObjects" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<ExhibitData>(path);
                if (data != null)
                {
                    // Map by asset name (e.g., "Exhibit_Cube" -> matches "Pedestal_Cube")
                    string key = data.name.Replace("Exhibit_", "").ToLower();
                    exhibitDataMap[key] = data;
                }
            }

            // Find all pedestals
            var pedestals = Object.FindObjectsByType<ExhibitPedestal>(FindObjectsSortMode.None);

            foreach (var pedestal in pedestals)
            {
                var serializedObject = new SerializedObject(pedestal);
                bool pedestalSuccess = true;

                // Wire up DisplayPoint
                var displayPoint = pedestal.transform.Find("DisplayPoint");
                if (displayPoint != null)
                {
                    Undo.RecordObject(pedestal, "Set DisplayPoint");
                    var displayPointProp = serializedObject.FindProperty("displayPoint");
                    displayPointProp.objectReferenceValue = displayPoint;
                    Debug.Log($"[Gallery Setup] {pedestal.name}: Assigned DisplayPoint");
                }
                else
                {
                    Debug.LogWarning($"[Gallery Setup] {pedestal.name}: DisplayPoint child not found.");
                    pedestalSuccess = false;
                }

                // Wire up ExhibitData based on name
                string pedestalKey = pedestal.name.Replace("Pedestal_", "").ToLower();
                if (exhibitDataMap.TryGetValue(pedestalKey, out var exhibitData))
                {
                    Undo.RecordObject(pedestal, "Set ExhibitData");
                    var exhibitDataProp = serializedObject.FindProperty("exhibitData");
                    exhibitDataProp.objectReferenceValue = exhibitData;
                    Debug.Log($"[Gallery Setup] {pedestal.name}: Assigned {exhibitData.name}");
                }
                else
                {
                    Debug.LogWarning($"[Gallery Setup] {pedestal.name}: No matching ExhibitData found for key '{pedestalKey}'.");
                    pedestalSuccess = false;
                }

                serializedObject.ApplyModifiedProperties();

                if (pedestalSuccess) success++; else fail++;
            }

            return (success, fail);
        }

        [MenuItem("Tools/Gallery/Add Pedestal Visuals")]
        public static void AddPedestalVisuals()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Undo.SetCurrentGroupName("Add Pedestal Visuals");
            int undoGroup = Undo.GetCurrentGroup();

            var pedestals = Object.FindObjectsByType<ExhibitPedestal>(FindObjectsSortMode.None);

            foreach (var pedestal in pedestals)
            {
                // Check if Base already exists
                if (pedestal.transform.Find("Base") != null)
                {
                    Debug.Log($"[Gallery Setup] {pedestal.name}: Base already exists, skipping.");
                    continue;
                }

                // Create cylinder base
                var baseObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                baseObj.name = "Base";
                baseObj.transform.SetParent(pedestal.transform);
                baseObj.transform.localPosition = new Vector3(0, 0.5f, 0);
                baseObj.transform.localRotation = Quaternion.identity;
                baseObj.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);

                Undo.RegisterCreatedObjectUndo(baseObj, "Create Pedestal Base");

                // Remove the collider from the visual (the pedestal root has the interaction collider)
                var collider = baseObj.GetComponent<Collider>();
                if (collider != null)
                {
                    Object.DestroyImmediate(collider);
                }

                Debug.Log($"[Gallery Setup] {pedestal.name}: Added Base visual");
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("<color=green>[Gallery Setup] Pedestal visuals added!</color>");
        }

        [MenuItem("Tools/Gallery/Assign Display Prefabs")]
        public static void AssignDisplayPrefabs()
        {
            // Map exhibit names to display prefab names
            var mapping = new System.Collections.Generic.Dictionary<string, string>
            {
                { "Exhibit_Cube", "Display_Cube" },
                { "Exhibit_Sphere", "Display_Sphere" },
                { "Exhibit_Cylinder", "Display_Cylinder" }
            };

            int assigned = 0;

            foreach (var pair in mapping)
            {
                // Find ExhibitData asset
                string[] exhibitGuids = AssetDatabase.FindAssets($"t:ExhibitData {pair.Key}");
                if (exhibitGuids.Length == 0)
                {
                    Debug.LogWarning($"[Gallery Setup] ExhibitData '{pair.Key}' not found.");
                    continue;
                }

                string exhibitPath = AssetDatabase.GUIDToAssetPath(exhibitGuids[0]);
                var exhibitData = AssetDatabase.LoadAssetAtPath<ExhibitData>(exhibitPath);

                // Find display prefab
                string[] prefabGuids = AssetDatabase.FindAssets($"{pair.Value} t:Prefab",
                    new[] { "Assets/_Project/Prefabs/DisplayObjects" });
                if (prefabGuids.Length == 0)
                {
                    Debug.LogWarning($"[Gallery Setup] Display prefab '{pair.Value}' not found.");
                    continue;
                }

                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[0]);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                // Assign prefab to ExhibitData
                var so = new SerializedObject(exhibitData);
                var displayPrefabProp = so.FindProperty("displayPrefab");
                displayPrefabProp.objectReferenceValue = prefab;
                so.ApplyModifiedProperties();

                Debug.Log($"[Gallery Setup] Assigned '{prefab.name}' to '{exhibitData.name}'");
                assigned++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"<color=green>[Gallery Setup] Assigned {assigned} display prefab(s)!</color>");
        }

        [MenuItem("Tools/Gallery/Fix InfoPanel Layout")]
        public static void FixInfoPanelLayout()
        {
            var infoPanel = Object.FindFirstObjectByType<InfoPanel>();
            if (infoPanel == null)
            {
                Debug.LogError("[Gallery Setup] InfoPanel not found in scene!");
                return;
            }

            Undo.SetCurrentGroupName("Fix InfoPanel Layout");
            int undoGroup = Undo.GetCurrentGroup();

            // Find Background and move it to first sibling
            var background = infoPanel.transform.Find("Background");
            if (background != null)
            {
                Undo.RecordObject(background, "Set Sibling Index");
                background.SetAsFirstSibling();
                Debug.Log("[Gallery Setup] Background moved to first sibling (behind text)");
            }
            else
            {
                Debug.LogWarning("[Gallery Setup] Background not found under InfoPanel");
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("<color=green>[Gallery Setup] InfoPanel layout fixed!</color>");
        }

        [MenuItem("Tools/Gallery/Setup QuickOutline")]
        public static void SetupQuickOutline()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Undo.SetCurrentGroupName("Setup QuickOutline");
            int undoGroup = Undo.GetCurrentGroup();

            var pedestals = Object.FindObjectsByType<ExhibitPedestal>(FindObjectsSortMode.None);
            int setupCount = 0;

            foreach (var pedestal in pedestals)
            {
                // Find the Base child (has the MeshRenderer)
                var baseTransform = pedestal.transform.Find("Base");
                if (baseTransform == null)
                {
                    Debug.LogWarning($"[Gallery Setup] {pedestal.name}: No 'Base' child found. Run 'Add Pedestal Visuals' first.");
                    continue;
                }

                // Check if Outline already exists
                var outline = baseTransform.GetComponent<Outline>();
                if (outline == null)
                {
                    // Add Outline component
                    outline = Undo.AddComponent<Outline>(baseTransform.gameObject);
                    Debug.Log($"[Gallery Setup] {pedestal.name}: Added Outline component to Base");
                }

                // Configure outline settings
                var outlineSO = new SerializedObject(outline);
                outlineSO.FindProperty("outlineMode").enumValueIndex = 0; // OutlineAll
                outlineSO.FindProperty("outlineColor").colorValue = new Color(1f, 0.8f, 0f, 1f); // Gold/yellow
                outlineSO.FindProperty("outlineWidth").floatValue = 4f;
                outlineSO.ApplyModifiedProperties();

                // Disable outline by default (ExhibitPedestal enables it on hover)
                Undo.RecordObject(outline, "Disable Outline");
                outline.enabled = false;

                // Wire up the outline reference on ExhibitPedestal
                var pedestalSO = new SerializedObject(pedestal);
                var outlineProp = pedestalSO.FindProperty("outline");
                outlineProp.objectReferenceValue = outline;
                pedestalSO.ApplyModifiedProperties();

                Debug.Log($"[Gallery Setup] {pedestal.name}: Configured and wired Outline");
                setupCount++;
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            if (setupCount > 0)
            {
                Debug.Log($"<color=green>[Gallery Setup] QuickOutline set up on {setupCount} pedestal(s)!</color>");
            }
            else
            {
                Debug.LogWarning("[Gallery Setup] No pedestals were set up. Make sure pedestals have 'Base' children.");
            }
        }

        [MenuItem("Tools/Gallery/Validate Setup")]
        public static void ValidateSetup()
        {
            Debug.Log("=== Gallery Setup Validation ===");
            int issues = 0;

            // Check AudioManager
            var audioManager = Object.FindFirstObjectByType<AudioManager>();
            if (audioManager == null)
            {
                Debug.LogError("[Validate] AudioManager not found in scene!");
                issues++;
            }
            else
            {
                var so = new SerializedObject(audioManager);
                if (so.FindProperty("sfxLibrary").objectReferenceValue == null)
                {
                    Debug.LogWarning("[Validate] AudioManager: sfxLibrary not assigned");
                    issues++;
                }
            }

            // Check UIManager
            var uiManager = Object.FindFirstObjectByType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogError("[Validate] UIManager not found in scene!");
                issues++;
            }
            else
            {
                var so = new SerializedObject(uiManager);
                if (so.FindProperty("infoPanel").objectReferenceValue == null)
                {
                    Debug.LogWarning("[Validate] UIManager: infoPanel not assigned");
                    issues++;
                }
            }

            // Check InfoPanel
            var infoPanel = Object.FindFirstObjectByType<InfoPanel>();
            if (infoPanel == null)
            {
                Debug.LogError("[Validate] InfoPanel not found in scene!");
                issues++;
            }
            else
            {
                var so = new SerializedObject(infoPanel);
                if (so.FindProperty("titleText").objectReferenceValue == null)
                {
                    Debug.LogWarning("[Validate] InfoPanel: titleText not assigned");
                    issues++;
                }
                if (so.FindProperty("descriptionText").objectReferenceValue == null)
                {
                    Debug.LogWarning("[Validate] InfoPanel: descriptionText not assigned");
                    issues++;
                }
            }

            // Check Pedestals
            var pedestals = Object.FindObjectsByType<ExhibitPedestal>(FindObjectsSortMode.None);
            if (pedestals.Length == 0)
            {
                Debug.LogWarning("[Validate] No ExhibitPedestal components found in scene!");
                issues++;
            }
            else
            {
                foreach (var pedestal in pedestals)
                {
                    var so = new SerializedObject(pedestal);
                    if (so.FindProperty("exhibitData").objectReferenceValue == null)
                    {
                        Debug.LogWarning($"[Validate] {pedestal.name}: exhibitData not assigned");
                        issues++;
                    }
                    if (so.FindProperty("displayPoint").objectReferenceValue == null)
                    {
                        Debug.LogWarning($"[Validate] {pedestal.name}: displayPoint not assigned");
                        issues++;
                    }
                }
            }

            // Check ExhibitManager
            var exhibitManager = Object.FindFirstObjectByType<ExhibitManager>();
            if (exhibitManager == null)
            {
                Debug.LogError("[Validate] ExhibitManager not found in scene!");
                issues++;
            }

            if (issues == 0)
            {
                Debug.Log("<color=green>=== All checks passed! Gallery is ready to use. ===</color>");
            }
            else
            {
                Debug.LogWarning($"=== Found {issues} issue(s). Run 'Tools > Gallery > Wire Up References' to fix. ===");
            }
        }

        [MenuItem("Tools/Gallery/Fix XR Interactable Colliders")]
        public static void FixXRInteractableColliders()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Undo.SetCurrentGroupName("Fix XR Interactable Colliders");
            int undoGroup = Undo.GetCurrentGroup();

            var interactables = Object.FindObjectsByType<XRSimpleInteractable>(FindObjectsSortMode.None);
            int fixedCount = 0;

            foreach (var interactable in interactables)
            {
                var so = new SerializedObject(interactable);
                var collidersProp = so.FindProperty("m_Colliders");

                // Get all colliders on this GameObject
                var colliders = interactable.GetComponents<Collider>();

                if (colliders.Length == 0)
                {
                    Debug.LogWarning($"[Gallery Setup] {interactable.name}: No colliders found on GameObject!");
                    continue;
                }

                // Check if colliders are already assigned
                if (collidersProp.arraySize == colliders.Length)
                {
                    bool allMatch = true;
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        if (collidersProp.GetArrayElementAtIndex(i).objectReferenceValue != colliders[i])
                        {
                            allMatch = false;
                            break;
                        }
                    }
                    if (allMatch)
                    {
                        Debug.Log($"[Gallery Setup] {interactable.name}: Colliders already correctly assigned");
                        continue;
                    }
                }

                // Assign colliders
                Undo.RecordObject(interactable, "Set Colliders");
                collidersProp.arraySize = colliders.Length;
                for (int i = 0; i < colliders.Length; i++)
                {
                    collidersProp.GetArrayElementAtIndex(i).objectReferenceValue = colliders[i];
                }
                so.ApplyModifiedProperties();

                Debug.Log($"[Gallery Setup] {interactable.name}: Assigned {colliders.Length} collider(s)");
                fixedCount++;
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            if (fixedCount > 0)
            {
                Debug.Log($"<color=green>[Gallery Setup] Fixed colliders on {fixedCount} XR Interactable(s)!</color>");
            }
            else
            {
                Debug.Log("[Gallery Setup] No XR Interactables needed fixing.");
            }
        }
    }
}
