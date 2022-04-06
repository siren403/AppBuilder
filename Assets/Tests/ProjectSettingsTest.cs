using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

public class ProjectSettingsTest
{
    // A Test behaves as an ordinary method
    [Test]
    public void ProjectSettingsTestSimplePasses()
    {
        var asset = AssetDatabase.LoadAssetAtPath<PlayerSettings>("ProjectSettings/ProjectSettings.asset");

        var path = Application.dataPath;
        var parent = Directory.GetParent(path);
        var parent2 = Directory.GetCurrentDirectory();

        var parent3 = Path.GetFileName(parent2);

        var playerSettings = AssetDatabase.LoadAssetAtPath<PlayerSettings>("ProjectSettings/ProjectSettings.asset");
        var preset = AssetDatabase.LoadAssetAtPath<Preset>("Assets/PlayerSettings.preset");
        preset.ApplyTo(playerSettings);
    }
}