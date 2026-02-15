using System.Linq;
using UnityEditor;
using UnityEngine;
#if STARTER_ASSETS_PACKAGES_CHECKED
using Cinemachine;
#endif

namespace StarterAssets
{
    // This class needs to be a scriptable object to support dynamic determination of StarterAssets install path
    public partial class StarterAssetsDeployMenu : ScriptableObject
    {
        public const string MenuRoot = "Tools/Starter Assets";

        // prefab names
        private const string MainCameraPrefabName = "MainCamera";
        private const string PlayerCapsulePrefabName = "PlayerCapsule";

        // names in hierarchy
        private const string CinemachineVirtualCameraName = "PlayerFollowCamera";

        // tags
        private const string PlayerTag = "Player";
        private const string MainCameraTag = "MainCamera";
        private const string CinemachineTargetTag = "CinemachineTarget";

        // Get the path to the template prefabs 
        private static string StarterAssetsPath => PathToThisFile;

        private static UnityEngine.GameObject _cinemachineVirtualCamera;

        /// <summary>
        /// Get the relative root path of the StarterAssets install - works even if user has
        /// moved it within Assets, so long as user does not mess with the internal hierarchy
        /// of the StarterAssets folder
        /// </summary>
        public static string StarterAssetsInstallPath
        {
            get
            {
                string path = PathToThisFile;
                // where this file is relative to install path:
                return path.Substring(0, path.LastIndexOf("StarterAssets"));
            }
        }

        private static string PathToThisFile
        {
            get
            {
                var dummy = CreateInstance<StarterAssetsDeployMenu>();
                string path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(dummy));
                DestroyImmediate(dummy);
                return path.Substring(0, path.LastIndexOf("/Editor/StarterAssetsDeployMenu.cs"));
            }
        }

        /// <summary>
        /// Deletes the scripting define set by the Package Checker.
        /// See Assets/Editor/PackageChecker/PackageChecker.cs for more information
        /// </summary>
        [MenuItem(MenuRoot + "/Reinstall Dependencies", false)]
        static void ResetPackageChecker()
        {
            ScriptingDefineUtils.RemoveScriptingDefine(PackageChecker.PackageCheckerScriptingDefine);
        }

#if STARTER_ASSETS_PACKAGES_CHECKED
        private static void CheckCameras(string prefabPath, Transform targetParent)
        {
            CheckMainCamera(prefabPath);

            UnityEngine.GameObject vcam = UnityEngine.GameObject.Find(CinemachineVirtualCameraName);

            if (!vcam)
            {
                HandleInstantiatingPrefab(StarterAssetsPath + prefabPath,
                    CinemachineVirtualCameraName,
                    out UnityEngine.GameObject vcamPrefab);
                _cinemachineVirtualCamera = vcamPrefab;
            }
            else
            {
                _cinemachineVirtualCamera = vcam;
            }

            UnityEngine.GameObject[] targets = UnityEngine.GameObject.FindGameObjectsWithTag(CinemachineTargetTag);
            UnityEngine.GameObject target = targets.FirstOrDefault(t => t.transform.IsChildOf(targetParent));
            if (target == null)
            {
                target = new UnityEngine.GameObject("PlayerCameraRoot");
                target.transform.SetParent(targetParent);
                target.transform.localPosition = new Vector3(0f, 1.375f, 0f);
                target.tag = CinemachineTargetTag;
                Undo.RegisterCreatedObjectUndo(target, "Created new cinemachine target");
            }
            CheckVirtualCameraFollowReference(target, _cinemachineVirtualCamera);
        }

        private static void CheckMainCamera(string prefabPath)
        {
            UnityEngine.GameObject[] mainCameras = UnityEngine.GameObject.FindGameObjectsWithTag(MainCameraTag);

            if (mainCameras.Length < 1)
            {
                // if there are no MainCameras, add one
                HandleInstantiatingPrefab(StarterAssetsPath + prefabPath, MainCameraPrefabName,
                    out _);
            }
            else
            {
                // make sure the found camera has a cinemachine brain (we only need 1)
                if (!mainCameras[0].TryGetComponent(out CinemachineBrain cinemachineBrain))
                    mainCameras[0].AddComponent<CinemachineBrain>();
            }
        }

        private static void CheckVirtualCameraFollowReference(UnityEngine.GameObject target,
            UnityEngine.GameObject cinemachineVirtualCamera)
        {
            var serializedObject =
                new SerializedObject(cinemachineVirtualCamera.GetComponent<CinemachineVirtualCamera>());
            var serializedProperty = serializedObject.FindProperty("m_Follow");
            serializedProperty.objectReferenceValue = target.transform;
            serializedObject.ApplyModifiedProperties();
        }

        private static void HandleInstantiatingPrefab(string path, string prefabName, out UnityEngine.GameObject prefab)
        {
            prefab = (UnityEngine.GameObject)PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<Object>($"{path}{prefabName}.prefab"));
            Undo.RegisterCreatedObjectUndo(prefab, "Instantiate Starter Asset Prefab");

            prefab.transform.localPosition = Vector3.zero;
            prefab.transform.localEulerAngles = Vector3.zero;
            prefab.transform.localScale = Vector3.one;
        }
#endif
    }
}