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

        public async Task<Response<IEnumerable<SolutionDetail>>> GetSolutionsListAsync(string crmConnection)
        {
            return new Response<IEnumerable<SolutionDetail>>()
            {
                IsSuccessful = true,
                Payload = solutions
            };
        }

        public async Task<Response<ConnectionResult>> ValidateConnectionAsync(string crmConnection)
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

        public async Task<Response<bool>> CreateWebresourceAsync(string crmConnection, WebResource webResource, string solution)
        {
            return new Response<bool>()
            {
                IsSuccessful = true,
                Payload = true
            };
        }
        public async Task<Response<bool>> UploadWebresourceAsync(string crmConnection, WebResource webResource)
        {
            return new Response<bool>()
            {
                IsSuccessful = true,
                Payload = true
            };
        }

        public async Task<Response<IEnumerable<WebResource>>> RetrieveWebResourcesAsync(string crmConnection, Guid solutionId, List<string> webResourceNames)
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

        public async Task<Response<bool>> PublishWebResourcesAsync(string crmConnection, IEnumerable<Guid> webResourcesIds)
        {
            return new Response<bool>()
            {
                IsSuccessful = true,
                Payload = true
            };
        }

        public async Task<Response<bool>> IsWebResourceExistsAsync(string crmConnection, string webResourceName)
        {
            return new Response<bool>()
            {
                IsSuccessful = true,
                Payload = true
            };
        }
    }
}
