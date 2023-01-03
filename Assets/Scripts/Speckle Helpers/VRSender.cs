using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Speckle.ConnectorUnity.Components;
using Speckle.Core.Api;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using UnityEngine;

namespace VRSample.Speckle_Helpers
{
    
    /// <summary>
    /// A custom speckle sender that adds sending as a coroutine
    /// This functionality might be added to SpeckleSender sometime.
    /// </summary>
    [RequireComponent(typeof(RecursiveConverter))]
    public class VRSender : MonoBehaviour
    {
        public GameObject environment;
        private RecursiveConverter converter;
        private CancellationTokenSource tokenSource;
        void Awake()
        {
            converter = GetComponent<RecursiveConverter>();
        }
        
        public IEnumerator ConvertAndSend(Client client, Stream stream, Branch branch)
        {
            Base b = converter.RecursivelyConvertToSpeckle(environment, _ => true);
            yield return null;

            Task.Run(async () =>
            {
                string commitId = await SendDataAsync(b, client, stream, branch);
            });
        }
        
        
        public async Task<string> SendDataAsync(Base data, Client client, Stream stream, Branch branch)
        {
            tokenSource?.Cancel();
            tokenSource?.Dispose();
            tokenSource = new CancellationTokenSource();
                
            ServerTransport transport = new ServerTransport(client.Account, stream.id);
            transport.CancellationToken = tokenSource.Token;
                
            return await SpeckleSender.SendDataAsync(tokenSource.Token,
                remoteTransport: transport,
                data: data,
                client: client,
                branchName: branch.name,
                createCommit: true,
                onProgressAction: dict => _ = 0,
                onErrorAction: (m, e) => Debug.LogError($"{m}\n{e}", this)
            );
        }
        
        
        public void OnDestroy()
        {
            tokenSource?.Cancel();
            tokenSource?.Dispose();
        }
    }
}
