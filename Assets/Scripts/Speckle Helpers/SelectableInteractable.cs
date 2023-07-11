using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRSample.Interactions
{
    [RequireComponent(typeof(IXRHoverInteractable))]
    public sealed class SelectableInteractable : MonoBehaviour
    {
        public Material selectedMaterial;
    
        private readonly Dictionary<Renderer, Material[]> _childMaterials = new();

        internal void OnEnable()
        {
            var interactable = GetComponent<IXRHoverInteractable>();
            interactable.hoverEntered.AddListener(Select);
            interactable.hoverExited.AddListener(Deselect);
        }
    
        internal void OnDisable()
        {
            var interactable = GetComponent<IXRHoverInteractable>();
            interactable.hoverEntered.RemoveListener(Select);
            interactable.hoverExited.RemoveListener(Deselect);
        }
    

        internal void Start()
        {
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                _childMaterials.Add(r, r.materials.ToArray());
            }
        }

        private void Select(object _)
        {
            foreach (var (r, m) in _childMaterials)
            {
                r.materials = Enumerable.Repeat(selectedMaterial, m.Length).ToArray();
            }
        }
    
        private void Deselect(object _)
        {
            foreach (var (r, m) in _childMaterials)
            {
                r.materials = m;
            }
        }
    }
}
