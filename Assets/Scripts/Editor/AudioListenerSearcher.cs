using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class AudioListenerSearcher : EditorWindow
{
    private List<AudioListener> _audioListeners = new List<AudioListener>();

    [MenuItem("Tools/Search Audio Listeners")]
    public static void ShowWindow()
    {
        GetWindow<AudioListenerSearcher>("Audio Listeners");
    }

    private void OnGUI()
    {
        GUILayout.Label("Find All Audio Listeners (Active & Inactive)", EditorStyles.boldLabel);

        if (GUILayout.Button("Search"))
        {
            FindAudioListeners();
        }

        GUILayout.Space(10);

        if (_audioListeners != null && _audioListeners.Count > 0)
        {
            GUILayout.Label($"Found {_audioListeners.Count} Audio Listeners:", EditorStyles.boldLabel);
            
            foreach (var listener in _audioListeners)
            {
                if (listener != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    // Display the GameObject with a reference slot
                    EditorGUILayout.ObjectField(listener.gameObject, typeof(GameObject), true);
                    
                    // Button to quickly select the object in the hierarchy
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeGameObject = listener.gameObject;
                        EditorGUIUtility.PingObject(listener.gameObject);
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        else if (_audioListeners != null && _audioListeners.Count == 0)
        {
            GUILayout.Label("No Audio Listeners found in the current scene(s).");
        }
    }

    private void FindAudioListeners()
    {
        _audioListeners.Clear();
        
        // FindObjectsOfType with true argument includes inactive GameObjects
        #if UNITY_2023_1_OR_NEWER
        var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        #else
        var listeners = Object.FindObjectsOfType<AudioListener>(true);
        #endif
        
        _audioListeners.AddRange(listeners);
        
        Debug.Log($"[AudioListenerSearcher] Found {_audioListeners.Count} Audio Listeners.");
        foreach (var listener in listeners)
        {
            Debug.Log($"AudioListener found on: {listener.gameObject.name} (Active: {listener.gameObject.activeInHierarchy})", listener.gameObject);
        }
    }
}
