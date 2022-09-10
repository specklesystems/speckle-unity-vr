using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sentry;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using UnityEngine;

#nullable enable
namespace VRSample.SpeckleUtils
{
    
    public class SpeckleReceiver : IDisposable
    {
        protected ITransport LocalCache;
        
        protected List<IDisposable?> activeTransports;

        public SpeckleReceiver()
        {
            activeTransports = new List<IDisposable?>();
            
            #if UNITY_STANDALONE
            SQLiteTransport SQLiteCache = new();
            LocalCache = SQLiteCache;
            activeTransports.Add(SQLiteCache);
            #else
            LocalCache = new MemoryTransport();
            #endif
        }

        public async Task<Base> ReceiveCommit(Client client, string streamId,
            Commit commit,
            CancellationToken cancellationToken,
            Action<ConcurrentDictionary<string, int>>? onProgressAction = null,
            Action<string, Exception>? onErrorAction = null,
            Action<int>? onTotalChildrenCountKnown = null)
        {
            var Base = await ReceiveObject(client.Account, streamId, commit.referencedObject, cancellationToken, onProgressAction, onErrorAction, onTotalChildrenCountKnown);
                
            try //Read receipt
            {
                await client.CommitReceived(cancellationToken, new CommitReceivedInput
                {
                    streamId = streamId,
                    commitId = commit.id,
                    message = "received commit from " + VersionedHostApplications.Unity,
                    sourceApplication = VersionedHostApplications.Unity
                });
            }
            catch(Exception e)
            {
                // Do nothing!
                Debug.LogWarning(e);
            }

            return Base;
        }
        
        public async Task<Base> ReceiveObject(Account account, string streamId,
            string objectId,
            CancellationToken cancellationToken,
            Action<ConcurrentDictionary<string, int>>? onProgressAction = null,
            Action<string, Exception>? onErrorAction = null,
            Action<int>? onTotalChildrenCountKnown = null)
        {
            ServerTransport? transport = new (account, streamId);
            activeTransports.Add(transport);
            try
            {

                Base @base = await Operations.Receive(
                    objectId,
                    remoteTransport: transport,
                    localTransport: LocalCache,
                    onErrorAction: onErrorAction,
                    onProgressAction: onProgressAction,
                    onTotalChildrenCountKnown: onTotalChildrenCountKnown,
                    cancellationToken: cancellationToken);
                return @base;
            }
            catch (Exception e)
            {
                throw new SpeckleException(e.Message, e, true, SentryLevel.Error);
            }
            finally
            {
                activeTransports.Remove(transport);
                transport?.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (IDisposable? transport in activeTransports)
            {
                transport?.Dispose();
            }
            activeTransports.Clear();
        }
    }   
}
