using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(World))]
public class WorldEditor : Editor
{
    World world;
    Editor noiseEditor;
    Editor chunkEditor;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Refresh World"))
            world.RefreshWorld();

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            if (check.changed)
            {
                if (world.autoRefresh)
                {
                    world.RefreshWorld();
                }
            }
        }

        DrawNoiseSettingsEditor(world.RefreshWorld, ref world.noiseSettingsFoldout, ref noiseEditor);
    }

    void DrawNoiseSettingsEditor(System.Action onSettingsUpdated, ref bool foldout, ref Editor editor)
    {
        Object settings = world.noiseSettings;
        if (settings == null)
            return;

        foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            if (foldout)
            {
                CreateCachedEditor(settings, null, ref editor);
                editor.OnInspectorGUI();
               

                if (check.changed)
                {
                    if (onSettingsUpdated != null && world.autoRefresh)
                    {
                        onSettingsUpdated();
                    }
                }
            }
        }
    }

    void OnEnable()
    {
        world = (World)target;
    }
}
