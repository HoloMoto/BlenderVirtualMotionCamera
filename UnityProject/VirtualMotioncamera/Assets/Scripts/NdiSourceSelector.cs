using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Klak.Ndi;
using TMPro;

namespace BlenderVirtualMotionCamera
{
    public class NdiSourceSelector : MonoBehaviour
    {
        [SerializeField] NdiReceiver _receiver;
        [SerializeField] TMP_Dropdown _dropdown;
        

        void Start()
        {
            // Start looking for sources
            StartCoroutine(UpdateSourceList());
            
            // Listen for selection changes
            _dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        IEnumerator UpdateSourceList()
        {
            while (true)
            {
                // Get current source names from NdiFinder
                var sources = NdiFinder.sourceNames.ToList();

                // Keep the current selection if possible
                var currentSelection = _dropdown.options.Count > 0 
                    ? _dropdown.options[_dropdown.value].text 
                    : "";

                // Update dropdown options
                _dropdown.ClearOptions();
                
                if (sources.Count == 0)
                {
                    _dropdown.AddOptions(new List<string> { "Scanning..." });
                }
                else
                {
                    _dropdown.AddOptions(sources);
                }

                // Restore selection or select the first one
                int index = sources.IndexOf(currentSelection);
                if (index >= 0)
                {
                    _dropdown.value = index;
                }
                else if (sources.Count > 0) 
                {
                     // If previously selected source is gone, or first run, try to find "Blender" automatically
                     int blenderIndex = sources.FindIndex(s => s.Contains("Blender"));
                     if (blenderIndex >= 0)
                     {
                         _dropdown.value = blenderIndex;
                         // Force update receiver immediately
                         _receiver.ndiName = sources[blenderIndex];
                     }
                }

                yield return new WaitForSeconds(2.0f); // Refresh every 2 seconds
            }
        }

        void OnDropdownValueChanged(int index)
        {
            if (_dropdown.options.Count > index)
            {
                string selectedName = _dropdown.options[index].text;
                if (selectedName != "Scanning...")
                {
                    _receiver.ndiName = selectedName;
                    Debug.Log($"NDI Source changed to: {selectedName}");
                }
            }
        }
    }
}
