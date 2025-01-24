using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

struct OmnicatAssetIcon {
    public string Path { get; set; }
    public Texture2D Icon { get; set; }

    public OmnicatAssetIcon(string path) {
        Path = path;
        Icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
}

[InitializeOnLoad]
public static class OmnicatIconDrawer {
    //InitializeOnLoad really doesn't like to initialize members so initialization is done inside the work method.
    private static OmnicatAssetIcon[] AssetIcons;

    //Constructor is called during InitializeOnLoad
    static OmnicatIconDrawer() {
        SetCustomIcon();
    }

    [MenuItem("Assets/Omnicat/Reload Icons")]
    public static void SetCustomIcon() {
        //Add new asset icons here and it will automatically handle the rest
        AssetIcons = new OmnicatAssetIcon[] {
            new OmnicatAssetIcon("Assets/OmnicatLabs/Icons/PoolerIcon.png"),
            new OmnicatAssetIcon("Assets/OmnicatLabs/Icons/ChannelHubIcon.png"),
        };

        OmnicatAssetIcon[] assetIconArray = AssetIcons.Where(icon => {
            if (icon.Icon == null) {
                Debug.LogError($"Could not load icon {icon.Path}");
                return false;
            }
            return true;
        }).ToArray();

        Array.ForEach(assetIconArray, assetIcon => DoAssetAssignment(assetIcon));

        AssetDatabase.Refresh();
    }

    private static void DoAssetAssignment(OmnicatAssetIcon assetIcon) {
        string typeName = ((Func<string>)(() => {
            int lastSlashIndex = assetIcon.Path.LastIndexOf('/');

            int iconIndex = assetIcon.Path.IndexOf("Icon", lastSlashIndex, System.StringComparison.Ordinal);

            if (lastSlashIndex != -1 && iconIndex != -1) {
                return assetIcon.Path.Substring(lastSlashIndex + 1, iconIndex - lastSlashIndex - 1);
            }

            Debug.LogError($"Failed to extrapolate type name from icon path: {assetIcon.Path}. Ensure that all Icon paths are like 'OmnicatLabs/Icons/NewSystemIcon.png'");

            return null;
        }))();
        {
            Array.ForEach(AssetDatabase.FindAssets($"t:{typeName}"), guid => {
                EditorGUIUtility.SetIconForObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guid)), assetIcon.Icon);
            });

        }
    }
}