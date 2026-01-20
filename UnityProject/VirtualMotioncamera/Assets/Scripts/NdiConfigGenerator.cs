using System.IO;
using UnityEngine;

namespace BlenderVirtualMotionCamera
{
    public class NdiConfigGenerator : MonoBehaviour
    {
        [Tooltip("The IP address of the NDI sender (Blender PC). Example: 100.x.y.z")]
        public string targetIpAddress = "127.0.0.1";

        [Tooltip("Force regenerate config on every start")]
        public bool forceUpdate = true;

        void Awake()
        {
            GenerateConfig();
        }

        public void GenerateConfig()
        {
            string path = Path.Combine(Application.persistentDataPath, "ndi-config.v1.json");

            if (!forceUpdate && File.Exists(path))
            {
                Debug.Log($"NDI Config already exists at: {path}");
                return;
            }

            // Create simple JSON content
            // Note: NDI expects "ips": "IP1,IP2" format
            string jsonContent = $@"{{
    ""ndi"": {{
        ""networks"": {{
            ""ips"": ""{targetIpAddress}""
        }}
    }}
}}";

            try
            {
                File.WriteAllText(path, jsonContent);
                Debug.Log($"NDI Config generated at: {path}\nContent: {jsonContent}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to write NDI config: {e.Message}");
            }
        }
    }
}
