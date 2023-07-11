using UnityEngine;

namespace VRSample.Interactions
{
    public class ObjectPlacer : MonoBehaviour
    {

        [field: SerializeField]
        public GameObject ObjectToPlace { get; set; }

        [field: SerializeField]
        public LayerMask Mask { get; set; }
        public bool IsPlacing => ObjectToPlace != null;

    
        public void Place()
        {
            ObjectToPlace = null;
        }

        public bool CancelPlace()
        {
            if (!IsPlacing) return false;
            Destroy(ObjectToPlace);

            return true;
        }

        protected void UpdateObjectLocation()
        {
            if (!IsPlacing) return;
        
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hit, Mathf.Infinity))
            {
                ObjectToPlace.transform.position = hit.point;
            }
        }

        public void Update()
        {
            UpdateObjectLocation();
        }

    }
}
