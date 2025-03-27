using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public class SDKBannerRemoverWindow : EditorWindow
{
    private const string TARGET_FILE_PATH = "Packages/com.vrchat.base/Editor/VRCSDK/Dependencies/VRChat/Resources/VRCSdkPanelStyles.uss";
    private const int TARGET_LINE_NUMBER = 17;

    [MenuItem("21tools/VRCSDK Banner Remover")]
    private static void InitializeWindow()
    {
        var removerWindow = GetWindow<SDKBannerRemoverWindow>();
        removerWindow.titleContent = new GUIContent("VRCSDK Banner Remover");
        removerWindow.minSize = new Vector2(30, 150);
        removerWindow.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space(15);

        DrawControlButton("Hide VRCSDK Banner", "0%");
        DrawControlButton("Show VRCSDK Banner", "100%");
    }

    private void DrawControlButton(string buttonLabel, string targetValue)
    {
        var buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 15,
            fixedHeight = 30,
            richText = true
        };

        if (GUILayout.Button($"<b>{buttonLabel}</b>", buttonStyle))
        {
            ToggleBannerVisibility(targetValue);
        }
    }

    private void ToggleBannerVisibility(string newHeightValue)
    {
        try
        {
            if (!ValidateFileIntegrity()) return;

            var styleLines = File.ReadAllLines(TARGET_FILE_PATH);
            if (!ValidateLineContent(styleLines)) return;

            styleLines[TARGET_LINE_NUMBER - 1] = Regex.Replace(
                styleLines[TARGET_LINE_NUMBER - 1],
                @"(max-height:\s*)(\d+%)",
                $"${{1}}{newHeightValue}"
            );

            File.WriteAllLines(TARGET_FILE_PATH, styleLines);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Operation failed: {e.Message}");
        }
    }

    private bool ValidateFileIntegrity()
    {
        if (File.Exists(TARGET_FILE_PATH)) return true;

        Debug.LogError($"SDK file missing at:\n{TARGET_FILE_PATH}");
        return false;
    }

    private bool ValidateLineContent(string[] fileLines)
    {
        if (fileLines.Length >= TARGET_LINE_NUMBER) return true;

        Debug.LogError($"Invalid line count in SDK file");
        return false;
    }

    private void NotifyOperationResult(string message)
    {
        Debug.Log($"SDK Banner Remover: {message}");
        ShowNotification(new GUIContent(message));
    }
}