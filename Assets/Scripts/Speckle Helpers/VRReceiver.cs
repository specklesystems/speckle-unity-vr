using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using Objects.BuiltElements;
using Speckle.ConnectorUnity.Components;
using Speckle.Core.Models;
using Speckle.Core.Models.GraphTraversal;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using VRSample.Interactions;
using Debug = UnityEngine.Debug;

namespace VRSample.Speckle_Helpers
{
    [RequireComponent(typeof(SpeckleReceiver))]
    public class VRReceiver : MonoBehaviour
    {
        [field: SerializeField]
        public XRGrabInteractable XRInteractablePrefab { get; set; }
        
        [field: SerializeField]
        public XRInteractionManager XRInteractionManager { get; set; }

        
        [Range(0,100), Min(0)]
        [Tooltip("The target Time(ms) per frame, for this component to performing blocking conversion in a coroutine")]
        public long frameTimeBudget = 14;
        
        [field: SerializeField]
        public SpeckleReceiver Receiver { get; private set; }
        
        [field: SerializeField]
        public Material SelectionMaterial { get; private set; }
        
        
        private void Awake()
        {
            Receiver = GetComponent<SpeckleReceiver>();
        }
        
#nullable enable
        
        public IEnumerator ReceiveRoutine(Transform? parent)
        {
            Task<Base> receiveOperation = Task.Run(async () => await Receiver.ReceiveAsync(default));
            
            yield return new WaitUntil(() => receiveOperation.IsCompleted);

            StartCoroutine(ConvertToXRInteractables(receiveOperation.Result, parent));
        }
        
        /// <summary>
        /// Each coroutine update,
        /// converts all "Level 1" speckle object to native, and parents converted objects to an <see cref="XRInteractablePrefab"/> instance
        /// </summary>
        public IEnumerator ConvertToXRInteractables(Base root, Transform? parent)
        {
            static bool Ignore<T>(TraversalContext tc) => tc.current is not T;

            var timeStamp = Stopwatch.StartNew();
            foreach (var result in Receiver.Converter.RecursivelyConvertToNative_Enumerable(root, null, tc => Ignore<Collection>(tc) && Ignore<Room>(tc)))
            {
                if(!result.WasSuccessful(out var converted, out var ex))
                {
                    Debug.Log($"Failed to convert {result.traversalContext.current}: {ex}", this);
                    continue;
                }

                // Only create interactables for "root" level objects
                if (converted.transform.parent == null)
                    CreateInteractable(converted, parent);

                bool hasReachedBudget = timeStamp.ElapsedMilliseconds >= frameTimeBudget;
                if (!hasReachedBudget) continue;
                
                timeStamp = Stopwatch.StartNew();
                yield return null; //return for 1 frame.
            }
        }

        private void CreateInteractable(GameObject converted, Transform? parent)
        {
            GameObject go = new GameObject("Interactable", 
                typeof (Rigidbody), 
                typeof (XRGrabInteractable), 
                typeof(SelectableInteractable));
            converted.transform.GetLocalPositionAndRotation(out Vector3 pos, out Quaternion rot);
            go.transform.SetLocalPositionAndRotation(pos, rot);
            go.transform.SetParent(parent, true);
                    
            var rb = go.GetComponent<Rigidbody>();
            {
                rb.isKinematic = true;
                rb.drag = 10;
                rb.angularDrag = 10;
                rb.useGravity = false;
            }
                    
            var interactable = go.GetComponent<XRGrabInteractable>();
            {
                interactable.throwOnDetach = false;
                interactable.useDynamicAttach = true;
            }
            
            var selectable = go.GetComponent<SelectableInteractable>();
            {
                selectable.selectedMaterial = SelectionMaterial;
            }


            foreach (var o in converted.GetComponentsInChildren<Transform>())
            {
                if (o.transform.parent == null) 
                    o.transform.SetParent(interactable.transform);
                MeshCollider c = o.gameObject.AddComponent<MeshCollider>();
                c.convex = true;
                interactable.colliders.Add(c);
            }
            XRInteractionManager.UnregisterInteractable((IXRInteractable)interactable);
            XRInteractionManager.RegisterInteractable((IXRInteractable)interactable);
        }

        
    }
}
