using UnityEditor;
using UnityEngine;
using System.IO;

namespace T3GIV.Editor
{
    public class AudioPreloadEditor : AssetPostprocessor
    {
        // 1. Automatically set "Preload Audio Data" for any NEW audio files imported
        private void OnPreprocessAudio()
        {
            AudioImporter audioImporter = (AudioImporter)assetImporter;
            AudioImporterSampleSettings settings = audioImporter.defaultSampleSettings;
            
            if (!settings.preloadAudioData)
            {
                settings.preloadAudioData = true;
                audioImporter.defaultSampleSettings = settings;
                Debug.Log($"[AudioPreloadEditor] Automatically enabled 'Preload Audio Data' for: {assetPath}");
            }
        }

        // 2. Menu item to batch-process all EXISTING audio files in the project
        [MenuItem("Tools/Audio/Preload All Audio Clips")]
        public static void PreloadAllAudioClips()
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip");
            int count = 0;
            int total = guids.Length;

            try
            {
                for (int i = 0; i < total; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    
                    // Show progress bar
                    if (EditorUtility.DisplayCancelableProgressBar("Preloading Audio Clips", $"Processing {Path.GetFileName(path)}", (float)i / total))
                    {
                        Debug.Log("[AudioPreloadEditor] Batch processing cancelled by user.");
                        break;
                    }

                    AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
                    if (audioImporter != null)
                    {
                        AudioImporterSampleSettings settings = audioImporter.defaultSampleSettings;
                        if (!settings.preloadAudioData)
                        {
                            settings.preloadAudioData = true;
                            audioImporter.defaultSampleSettings = settings;
                            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                            count++;
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[AudioPreloadEditor] Batch processing complete. Updated {count} audio clips to 'Preload Audio Data'.");
            EditorUtility.DisplayDialog("Audio Preload Fixer", $"Successfully updated {count} audio clips to 'Preload Audio Data'.", "OK");
        }
    }
}
