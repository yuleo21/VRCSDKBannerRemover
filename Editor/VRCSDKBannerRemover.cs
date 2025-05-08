using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

[InitializeOnLoad]
public class SDKBannerRemover
{
    private const string TARGET_FILE_PATH = "Packages/com.vrchat.base/Editor/VRCSDK/Dependencies/VRChat/Resources/VRCSdkPanelStyles.uss";
    private const int TARGET_LINE_NUMBER = 17;
    private const string MENU_PATH_HIDE = "21tools/VRCSDKBannerRemover/Hide VRCSDK Banner";
    private const string MENU_PATH_SHOW = "21tools/VRCSDKBannerRemover/Show VRCSDK Banner";
    private static System.DateTime _lastWriteTime = System.DateTime.MinValue;

    static SDKBannerRemover()
    {
        EditorApplication.delayCall += () =>
        {
            if (IsBannerVisible() && File.Exists(TARGET_FILE_PATH))
            {
                HideBanner();
                Debug.Log("SDK Banner Remover: Banner hidden.");
            }
        };
        
        EditorApplication.update += MonitorStyleFileChanges;
    }
    
    private static void MonitorStyleFileChanges()
    {
        if (!File.Exists(TARGET_FILE_PATH)) return;
        
        try
        {
            var currentWriteTime = File.GetLastWriteTime(TARGET_FILE_PATH);
            
            if (currentWriteTime != _lastWriteTime)
            {
                _lastWriteTime = currentWriteTime;
                RefreshVRCSDKWindows();
            }
        }
        catch (System.Exception)
        {
        }
    }

    [MenuItem(MENU_PATH_HIDE, false)]
    private static void HideBanner()
    {
        ToggleBannerVisibility("0%");
        Menu.SetChecked(MENU_PATH_HIDE, true);
        Menu.SetChecked(MENU_PATH_SHOW, false);
    }

    [MenuItem(MENU_PATH_HIDE, true)]
    private static bool ValidateHideBanner()
    {
        return IsBannerVisible();
    }

    [MenuItem(MENU_PATH_SHOW, false)]
    private static void ShowBanner()
    {
        ToggleBannerVisibility("100%");
        Menu.SetChecked(MENU_PATH_SHOW, true);
        Menu.SetChecked(MENU_PATH_HIDE, false);
    }
    
    [MenuItem(MENU_PATH_SHOW, true)]
    private static bool ValidateShowBanner()
    {
        return !IsBannerVisible();
    }

    private static void ToggleBannerVisibility(string newHeightValue)
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
            
            RefreshVRCSDKWindows();
            EditorUtility.SetDirty(Selection.activeObject);
            
            string action = newHeightValue == "0%" ? "hidden" : "shown";
            NotifyOperationResult($"VRC SDK Banner {action}.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Operation failed: {e.Message}");
        }
    }
    
    private static void RefreshVRCSDKWindows()
    {
        var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
        foreach (var window in windows)
        {
            if (window.GetType().FullName?.Contains("VRC") == true ||
                window.titleContent.text?.Contains("VRC") == true)
            {
                window.Repaint();
            }
        }
        
        AssetDatabase.Refresh();
        
        EditorApplication.delayCall += () => {
            foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                window.Repaint();
            }
        };
    }

    private static bool ValidateFileIntegrity()
    {
        if (File.Exists(TARGET_FILE_PATH)) return true;
        Debug.LogError($"SDK file missing at:\n{TARGET_FILE_PATH}");
        return false;
    }

    private static bool ValidateLineContent(string[] fileLines)
    {
        if (fileLines.Length >= TARGET_LINE_NUMBER) return true;
        Debug.LogError($"Invalid line count in SDK file");
        return false;
    }

    private static void NotifyOperationResult(string message)
    {
        Debug.Log($"VRCSDK Banner Remover: {message}");
    }
    
    private static bool IsBannerVisible()
    {
        try
        {
            if (!ValidateFileIntegrity()) return true;
            var styleLines = File.ReadAllLines(TARGET_FILE_PATH);
            if (!ValidateLineContent(styleLines)) return true;
            
            var match = Regex.Match(styleLines[TARGET_LINE_NUMBER - 1], @"max-height:\s*(\d+)%");
            if (match.Success)
            {
                return match.Groups[1].Value != "0";
            }
            return true;
        }
        catch (System.Exception)
        {
            return true;
        }
    }
}