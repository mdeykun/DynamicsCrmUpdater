using Cwru.Common.Model;
using Cwru.CrmRequests.Common;
using Cwru.CrmRequests.Common.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cwru.CrmRequests.Client
{
    public class CrmWebResourcesUpdaterClientStub : ICrmRequests
    {
        public CrmWebResourcesUpdaterClientStub()
        {

        }

        private List<SolutionDetail> solutions = new List<SolutionDetail>()
        {
            new SolutionDetail()
            {
                FriendlyName = "Default Solution",
                PublisherPrefix = "new",
                SolutionId = new Guid("{8c2c699b-933f-4aa8-935d-8d3d80a486bd}"),
                UniqueName = "Default Solution"

            },
            new SolutionDetail()
            {
                FriendlyName = "Customizations",
                PublisherPrefix = "cst",
                SolutionId = new Guid("{8c2c699b-933f-4aa8-946d-8d3d80a486bd}"),
                UniqueName = "Customizations"

            }
        };

        public async Task<Response<IEnumerable<SolutionDetail>>> GetSolutionsListAsync(string crmConnectionString)
        {
            return new Response<IEnumerable<SolutionDetail>>()
            {
                IsSuccessful = true,
                Payload = solutions
            };
        }
        public async Task<Response<ConnectionResult>> ValidateConnectionAsync(string crmConnectionString)
        {
            return new Response<ConnectionResult>()
            {
                IsSuccessful = true,
                Payload = new ConnectionResult()
                {
                    IsReady = true,
                    OrganizationUniqueName = "org_100500",
                    OrganizationVersion = "lates_100",
                    LastCrmError = null,
                }
            };
        }
        public async Task<Response<bool>> CreateWebresourceAsync(string crmConnectionString, WebResource webResource, string solution)
        {
            return new Response<bool>()
            {
                IsSuccessful = true,
                Payload = true
            };
        }
        public async Task<Response<bool>> UploadWebresourceAsync(string crmConnectionString, WebResource webResource)
        {
            return new Response<bool>()
            {
                IsSuccessful = true,
                Payload = true
            };
        }
        public async Task<Response<IEnumerable<WebResource>>> RetrieveSolutionWebResourcesAsync(string crmConnectionString, Guid solutionId, IEnumerable<string> webResourceNames)
        {
            return new Response<IEnumerable<WebResource>>()
            {

                IsSuccessful = true,
                Payload = new List<WebResource>()
                {
                    new WebResource()
                    {
                        Content = "qqqqqqq",
                        Description = "Descr",
                        DisplayName = "displayName",
                        Id = solutionId,
                        Name = "new_wr.js"
                    }
                }
            };
        }
        public async Task<Response<IEnumerable<WebResource>>> RetrieveWebResourcesAsync(string crmConnectionString, IEnumerable<string> webResourceNames)
        {
            return new Response<IEnumerable<WebResource>>()
            {
                IsSuccessful = true,
                Payload = new List<WebResource>()
                {
                    new WebResource()
                    {
                        Content = "qqqqqqq",
                        Description = "Descr",
                        DisplayName = "displayName",
                        Name = "new_wr.js"
                    }
                }
            };
        }
        public async Task<Response<bool>> PublishWebResourcesAsync(string crmConnectionString, IEnumerable<Guid> webResourcesIds)
        {
            return new Response<bool>()
            {
                IsSuccessful = true,
                Payload = true
            };
        }
        public async Task<Response<bool>> IsWebResourceExistsAsync(string crmConnectionString, string webResourceName)
        {
            return new Response<bool>()
            {
                IsSuccessful = true,
                Payload = true
            };
        }
    }
}
