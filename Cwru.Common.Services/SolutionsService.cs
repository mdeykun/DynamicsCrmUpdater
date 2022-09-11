using Cwru.Common.Config;
using Cwru.Common.Model;
using Cwru.CrmRequests.Common.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cwru.Common.Services
{
    public class SolutionsService
    {
        private readonly ICrmRequests crmRequestsClient;
        private readonly List<SolutionDetail> solutionDetails = new List<SolutionDetail>();

        public SolutionsService(ICrmRequests crmRequestsClient)
        {
            this.crmRequestsClient = crmRequestsClient;
        }

        public async Task<IEnumerable<SolutionDetail>> GetSolutionsDetailsAsync(string connectionString, Guid? environmentId = null, bool updateCache = false)
        {
            if (environmentId == null)
            {
                return await LoadSolutionDetailsAsync(connectionString);
            }

            if (updateCache == true || !IsLoaded(environmentId.Value))
            {
                return await LoadSolutionDetailsAsync(connectionString, environmentId.Value);
            }

            return GetFromCache(environmentId.Value);
        }

        public async Task<SolutionDetail> GetSolutionDetailsAsync(EnvironmentConfig environmentConfig, bool updateCache = false)
        {
            return await GetSolutionDetailsAsync(environmentConfig, environmentConfig.SelectedSolutionId, updateCache);
        }

        private async Task<SolutionDetail> GetSolutionDetailsAsync(EnvironmentConfig environmentConfig, Guid solutionId, bool updateCache = false)
        {
            if (updateCache == true || !IsLoaded(environmentConfig.Id, solutionId))
            {
                await LoadSolutionDetailsAsync(environmentConfig);
            }

            return GetFromCache(environmentConfig.Id, solutionId);
        }

        private bool IsLoaded(Guid environmentId, Guid solutionId)
        {
            return solutionDetails.Any(x => x.EnvironmentId == environmentId && x.SolutionId == solutionId);
        }

        private bool IsLoaded(Guid environmentId)
        {
            return solutionDetails.Any(x => x.EnvironmentId == environmentId);
        }

        private SolutionDetail GetFromCache(Guid environmentId, Guid solutionId)
        {
            return solutionDetails.FirstOrDefault(x => x.EnvironmentId == environmentId && x.SolutionId == solutionId);
        }

        private List<SolutionDetail> GetFromCache(Guid environmentId)
        {
            return solutionDetails.Where(x => x.EnvironmentId == environmentId).ToList();
        }

        private async Task<List<SolutionDetail>> LoadSolutionDetailsAsync(EnvironmentConfig environmentConfig)
        {
            return await LoadSolutionDetailsAsync(environmentConfig.ConnectionString.BuildConnectionString(), environmentConfig.Id);
        }

        private async Task<List<SolutionDetail>> LoadSolutionDetailsAsync(string connectionString, Guid environmentId)
        {
            var solutionDetails = await LoadSolutionDetailsAsync(connectionString);

            foreach (var solution in solutionDetails)
            {
                solution.EnvironmentId = environmentId;
                if (this.solutionDetails.Contains(solution))
                {
                    this.solutionDetails.Remove(solution);
                }

                this.solutionDetails.Add(solution);
            }

            return solutionDetails;
        }

        private async Task<List<SolutionDetail>> LoadSolutionDetailsAsync(string connectionString)
        {
            var solutionsResponse = await crmRequestsClient.GetSolutionsListAsync(connectionString);
            if (solutionsResponse.IsSuccessful)
            {
                return solutionsResponse.Payload.ToList();
            }
            else
            {
                throw new Exception($"Failed to retrieve solutions list: {solutionsResponse.Error}");
            }
        }
    }
}

