using UnityEngine;
using UnityEngine.UI;
using Klak.Ndi;

namespace BlenderVirtualMotionCamera
{
    /// <summary>
    /// Receives an NDI stream and displays it on a UI RawImage.
    /// Works with KlakNDI plugin by Keijiro Takahashi.
    /// </summary>
    public class NdiReceiverUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string _ndiSourceName = "Blender"; // Should match your Blender NDI output name
        [SerializeField] private RawImage _targetImage;
        
        private NdiReceiver _receiver;

        void Start()
        {
            if (_targetImage == null)
                _targetImage = GetComponent<RawImage>();

            // Ensure the GameObject has an NdiReceiver component
            _receiver = gameObject.GetComponent<NdiReceiver>();
            if (_receiver == null)
                _receiver = gameObject.AddComponent<NdiReceiver>();

            // Setup receiver
            // Note: KlakNDI might require you to select the source from a list in the Inspector normally,
            // but we can try to find it by name if necessary.
            _receiver.ndiName = _ndiSourceName;
        }

        void Update()
        {
            if (_receiver == null || _targetImage == null) return;

            // NdiReceiverのtexture、または設定されているTarget Textureを取得
            Texture tex = _receiver.targetTexture != null ? (Texture)_receiver.targetTexture : _receiver.texture;

            if (tex != null)
            {
                _targetImage.texture = tex;
                
                // アスペクト比の維持
                float aspect = (float)tex.width / tex.height;
                RectTransform rt = _targetImage.rectTransform;
                rt.sizeDelta = new Vector2(rt.rect.height * aspect, rt.rect.height);
            }
        }
    }
}
