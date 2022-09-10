using System.Threading;
using System.Threading.Tasks;
using Speckle.ConnectorUnity;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Models;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using VRSample.SpeckleUtils;
using VRSample.UI.Components;
using Text = TMPro.TMP_Text;

namespace VRSample.UI.Controllers
{
    [DisallowMultipleComponent, RequireComponent(typeof(SpeckleReceiver), typeof(RecursiveConverter))]
    public class StreamSelectionMenu : MonoBehaviour
    {
        [field: SerializeField, Range(1, 100)]
        public int NumberOfStreams { get; set; } = 10;

        [field: SerializeField]
        public Transform ContentTarget { get; set; }
        
        [field: SerializeField]
        public StreamViewComponent StreamViewPrefab { get; set; }

        [field: SerializeField]
        public GameObject GeometryInteractorPrefab { get; set; }

        public XRInteractionManager XRInteractionManager;

        public bool SendMode;
        
        public SpeckleReceiver Receiver { get; private set; }

        protected Client speckleClient;
        protected CancellationTokenSource cancellationTokenSource;

        private RecursiveConverter rc;
        protected virtual void Awake()
        {
            rc = GetComponent<RecursiveConverter>();
            Receiver = new SpeckleReceiver();
            
            Account acc = AccountManager.GetDefaultAccount();
            Debug.Assert(acc != null, "No Speckle account found!", this);
            Debug.Assert(StreamViewPrefab != null, $"{nameof(StreamViewPrefab)} must be set set!", this);
            speckleClient = new Client(acc);
        }
        

        protected async Task GenerateOptions()
        {
            var streams = speckleClient.GetStreamWithBranches(cancellationTokenSource.Token, NumberOfStreams);
            
            ClearOptions();

            foreach (var stream in await streams)
            {
                Texture previewImage = await GetImage($"{speckleClient.Account.serverInfo.url}/preview/{stream.id}", speckleClient.Account.token, cancellationTokenSource.Token);
                StreamViewComponent streamElement = Instantiate(StreamViewPrefab, ContentTarget);
                streamElement.Initialise(stream, previewImage, OnClick);
            }
        }

        protected void OnClick(string streamId, Commit commit)
        {
            if (SendMode) Send(streamId);
            else Receive(streamId, commit);
        }
        
        protected void Receive(string streamId, Commit commit)
        {
            Task.Run(async () =>
            {
                Base commitObject = await Receiver.ReceiveCommit(speckleClient, streamId, commit, cancellationTokenSource.Token);

                Dispatcher.Instance().Enqueue(() =>
                {
                    //Create parent
                    var parent = Instantiate(GeometryInteractorPrefab);
                    var interactable = parent.GetComponent<XRGrabInteractable>();
                    XRInteractionManager.RegisterInteractable((IXRInteractable)interactable);
                    
                    //convert to native
                    rc.RecursivelyConvertToNative(commitObject, parent.transform);
                    
                    // Add colliders
                    foreach (MeshFilter mesh in parent.GetComponentsInChildren<MeshFilter>())
                    {
                        var col = mesh.gameObject.AddComponent<BoxCollider>();
                        interactable.colliders.Add(col);
                    }
                    
                    //todo remove old
                    
                });
            });
        }

        protected static async Task<Texture2D> GetImage(string url, string authToken, CancellationToken cancellationToken)
        {
            using UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            www.SetRequestHeader("Authorization", $"Bearer {authToken}");
            UnityWebRequestAsyncOperation request = www.SendWebRequest();

            while (!request.isDone)
            {
                if (cancellationToken.IsCancellationRequested) return null;
                
                await Task.Delay(100);
            }
                
            if(www.result != UnityWebRequest.Result.Success )
            {
                Debug.Log( $"Error fetching image from {www.url}: {www.error}" );
                return null;
            }
                
            return DownloadHandlerTexture.GetContent(www);
        }
            
        
        protected void ClearOptions()
        {
            foreach (var child in ContentTarget.GetComponentsInChildren<StreamViewComponent>())
            {
                Destroy(child.gameObject);
            }
        }
        
        protected async void OnEnable()
        {
            cancellationTokenSource = new CancellationTokenSource();
            await GenerateOptions();
        }

        protected void OnDisable()
        {
            cancellationTokenSource.Cancel();
            ClearOptions();
        }
        
        public void Send(string streamId)
        {
            rc.RecursivelyConvertToSpeckle(SceneManager.GetActiveScene().GetRootGameObjects(),
                o => true);
            
            //todo send.
        }
        
        public void Exit()
        {
            Application.Quit();
        }
        
    }
}


