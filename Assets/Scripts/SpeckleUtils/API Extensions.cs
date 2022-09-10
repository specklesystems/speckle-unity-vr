using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using Speckle.Core.Api;
using Speckle.Core.Logging;

namespace VRSample.SpeckleUtils
{
    public static class APIExtensions
    {
        public static async Task<List<Stream>> GetStreamWithBranches(this Client client,
            CancellationToken cancellationToken,
            int streamLimit = 10,
            int branchLimit = 10,
            int commitLimit = 1)
        {
            List<Stream> items;
            try
            {
                GraphQLRequest request = new()
                {
                    Query = StreamWithBranchesQuery,
                    Variables = new {sLimit = streamLimit, bLimit = branchLimit, cLimit = commitLimit}
                };
                GraphQLResponse<UserData> graphQlResponse = await client.GQLClient
                    .SendMutationAsync<UserData>(request, cancellationToken).ConfigureAwait(false);

                if (graphQlResponse.Errors != null)
                    throw new SpeckleException("Could not get streams", graphQlResponse.Errors);
                items = graphQlResponse.Data.user.streams.items;
            }
            catch (Exception ex)
            {
                throw new SpeckleException(ex.Message, ex);
            }

            return items;
        }

        private const string StreamWithBranchesQuery =
@"query streamFetch($sLimit: Int!, $bLimit: Int!, $cLimit: Int!) {
  user {
    id
    name
    streams(limit: $sLimit){
      items {
        id
        name
        description
        isPublic
        role
        createdAt
        updatedAt
        favoritedDate
        branches(limit: $bLimit) {
          totalCount
          items{
            id
            name
            description
            commits(limit: $cLimit) {
              totalCount
              items{
                id
                message
                branchName
                authorName
                authorId
                createdAt
                sourceApplication
                referencedObject
              }
            }
          }
        }
      }
    }
  }
}";
    }
}
