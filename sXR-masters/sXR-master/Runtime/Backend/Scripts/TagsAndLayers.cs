using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace sxr_internal
{
    public class TagsAndLayers : MonoBehaviour
    {
        private static int maxTags = 10000;
        private static int maxLayers = 31;

        // Runtime tag and layer storage
        private static HashSet<string> runtimeTags = new HashSet<string>();
        private static Dictionary<string, int> runtimeLayers = new Dictionary<string, int>();
        private static bool initialized = false;

        private static void InitializeIfNeeded()
        {
            if (!initialized)
            {
                // Initialize with Unity's built-in tags
                runtimeTags.Add("Untagged");
                runtimeTags.Add("Respawn");
                runtimeTags.Add("Finish");
                runtimeTags.Add("EditorOnly");
                runtimeTags.Add("MainCamera");
                runtimeTags.Add("Player");
                runtimeTags.Add("GameController");

                // Initialize with Unity's built-in layers
                runtimeLayers.Add("Default", 0);
                runtimeLayers.Add("TransparentFX", 1);
                runtimeLayers.Add("Ignore Raycast", 2);
                runtimeLayers.Add("Water", 4);
                runtimeLayers.Add("UI", 5);

                initialized = true;
            }
        }

        /// <summary>
        /// Creates a tag (runtime version)
        /// </summary>
        public static bool CreateTag(string tagName)
        {
            InitializeIfNeeded();

            if (string.IsNullOrEmpty(tagName))
                return false;

            if (runtimeTags.Count >= maxTags)
            {
                Debug.Log("No more tags can be added. You have " + runtimeTags.Count + " tags");
                return false;
            }

            if (!runtimeTags.Contains(tagName))
            {
                runtimeTags.Add(tagName);
                Debug.Log("Tag: " + tagName + " has been added");
                return true;
            }

            return false;
        }

        public static string NewTag(string name)
        {
            CreateTag(name);

            if (string.IsNullOrEmpty(name))
            {
                name = "Untagged";
            }

            return name;
        }

        /// <summary>
        /// Removes a tag (runtime version)
        /// </summary>
        public static bool RemoveTag(string tagName)
        {
            InitializeIfNeeded();

            if (runtimeTags.Contains(tagName))
            {
                runtimeTags.Remove(tagName);
                Debug.Log("Tag: " + tagName + " has been removed");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if tag exists (runtime version)
        /// </summary>
        public static bool TagExists(string tagName)
        {
            InitializeIfNeeded();
            return runtimeTags.Contains(tagName);
        }

        /// <summary>
        /// Creates a layer (runtime version)
        /// </summary>
        public static bool CreateLayer(string layerName)
        {
            InitializeIfNeeded();

            if (string.IsNullOrEmpty(layerName))
                return false;

            if (runtimeLayers.ContainsKey(layerName))
                return false;

            // Find next available layer slot (starting from layer 8)
            for (int i = 8; i < maxLayers; i++)
            {
                if (!runtimeLayers.ContainsValue(i))
                {
                    runtimeLayers.Add(layerName, i);
                    Debug.Log("Layer: " + layerName + " has been added at index " + i);
                    return true;
                }
            }

            Debug.Log("All allowed layers have been filled");
            return false;
        }

        public static string NewLayer(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                CreateLayer(name);
            }

            return name;
        }

        /// <summary>
        /// Removes a layer (runtime version)
        /// </summary>
        public static bool RemoveLayer(string layerName)
        {
            InitializeIfNeeded();

            if (runtimeLayers.ContainsKey(layerName))
            {
                int layerIndex = runtimeLayers[layerName];
                runtimeLayers.Remove(layerName);
                Debug.Log("Layer: " + layerName + " has been removed from index " + layerIndex);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if layer exists (runtime version)
        /// </summary>
        public static bool LayerExists(string layerName)
        {
            InitializeIfNeeded();
            return runtimeLayers.ContainsKey(layerName);
        }

        /// <summary>
        /// Gets the layer index for a layer name
        /// </summary>
        public static int GetLayerIndex(string layerName)
        {
            InitializeIfNeeded();
            return runtimeLayers.TryGetValue(layerName, out int index) ? index : -1;
        }

        /// <summary>
        /// Gets all current tags
        /// </summary>
        public static string[] GetAllTags()
        {
            InitializeIfNeeded();
            return runtimeTags.ToArray();
        }

        /// <summary>
        /// Gets all current layers
        /// </summary>
        public static string[] GetAllLayers()
        {
            InitializeIfNeeded();
            return runtimeLayers.Keys.ToArray();
        }

        // Instance methods for backwards compatibility
        public void AddNewTag(string name)
        {
            CreateTag(name);
        }

        public void DeleteTag(string name)
        {
            RemoveTag(name);
        }

        public void AddNewLayer(string name)
        {
            CreateLayer(name);
        }

        public void DeleteLayer(string name)
        {
            RemoveLayer(name);
        }

        public static void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        /// <summary>
        /// Alternative method that uses layer name instead of index
        /// </summary>
        public static void SetLayerRecursively(GameObject obj, string layerName)
        {
            int layerIndex = GetLayerIndex(layerName);
            if (layerIndex != -1)
            {
                SetLayerRecursively(obj, layerIndex);
            }
            else
            {
                Debug.LogWarning("Layer '" + layerName + "' does not exist!");
            }
        }
    }
}