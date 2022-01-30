using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmWebResourcesUpdater.Service.Extensions
{
    public static class OrgServiceExtensions
    {
        public static async Task<OrganizationResponse> ExecuteAsync(this OrganizationServiceContext context, OrganizationRequest request)
        {
            var task = Task.Run(() => context.Execute(request));
            return await task;
        }

        public static async Task<EntityCollection> RetrieveMultipleAsync(this IOrganizationService proxy, FetchExpression fetchExpression)
        {
            var task = Task.Run(() => proxy.RetrieveMultiple(fetchExpression));
            return await task;
        }

        public static async Task<EntityCollection> RetrieveMultipleAsync(this IOrganizationService proxy, QueryExpression queryExpression)
        {
            var task = Task.Run(() => proxy.RetrieveMultiple(queryExpression));
            return await task;
        }

        public static async Task UpdateAsync(this IOrganizationService proxy, Entity entity)
        {
            var task = Task.Run(() => proxy.Update(entity));
            await task;
        }

    }
}
