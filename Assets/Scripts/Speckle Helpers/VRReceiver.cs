using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Speckle.ConnectorUnity.Components;
using Speckle.Core.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRSample.Speckle_Helpers
{
    [RequireComponent(typeof(SpeckleReceiver))]
    public class VRReceiver : MonoBehaviour
    {
        [field: SerializeField]
        public XRGrabInteractable XRInteractablePrefab { get; set; }
        
        [field: SerializeField]
        public XRInteractionManager XRInteractionManager { get; set; }
        
        public SpeckleReceiver Receiver { get; private set; }
        
        private void Awake()
        {
            Receiver = GetComponent<SpeckleReceiver>();
        }
        
#nullable enable
        
        public IEnumerator ReceiveRoutine(Transform? parent)
        {
            Task<Base?> receiveOperation = Task.Run(Receiver.ReceiveAsync);
            
            yield return new WaitUntil(() => receiveOperation.IsCompleted);

            yield return ConvertToXRInteractables(receiveOperation.Result, parent);
        }
        
        /// <summary>
        /// Each coroutine update,
        /// converts one Level 1 speckle object to native, and parents converted objects to a <see cref="XRInteractablePrefab"/> instance
        /// </summary>
        public IEnumerator ConvertToXRInteractables(Base @base, Transform? parent)
        {
            foreach (var rootMember in @base.GetMembers())
            {
                List<Base> objectsToConvertThisFrame = new List<Base>();
                Flatten(rootMember.Value, objectsToConvertThisFrame);
                foreach (var so in objectsToConvertThisFrame)
                {
                    var converted = Receiver.Converter.RecursivelyConvertToNative(so, null);

                    //Skip empties
                    if (converted.Count <= 0) continue;

                    GameObject go = ObjectFactory.CreateGameObject("Interactable", typeof (Rigidbody), typeof (XRGrabInteractable));
                    go.transform.SetParent(parent);
                    IXRInteractable interactable = go.GetComponent<XRGrabInteractable>();
                    XRInteractionManager.RegisterInteractable(interactable);
                    foreach (var o in converted)
                    {
                        if (o.transform.parent == null) 
                            o.transform.SetParent(interactable.transform);
                        Collider c = o.AddComponent<MeshCollider>();
                        interactable.colliders.Add(c);
                    }
                    
                    yield return null;
                }
            }
        }

        private void Flatten(object? o, IList<Base> outSpeckleObjects)
        {
            if(o is Base b) outSpeckleObjects.Add(b);
            if(o is IList l) foreach(object? lo in l) Flatten(lo, outSpeckleObjects);
            if(o is IDictionary d) foreach(var v in d.Values) Flatten(v, outSpeckleObjects);
        }
    }
}
