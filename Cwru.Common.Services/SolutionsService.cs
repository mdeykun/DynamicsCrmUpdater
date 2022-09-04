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

        public async Task<SolutionDetail> GetSolutionDetailsAsync(EnvironmentConfig environmentConfig)
        {
            return await GetSolutionDetailsAsync(environmentConfig, environmentConfig.SelectedSolutionId);
        }

        public async Task<SolutionDetail> GetSolutionDetailsAsync(EnvironmentConfig environmentConfig, Guid solutionId)
        {
            var solutionDetail = FindSolutionDetails(environmentConfig.Id, solutionId);
            if (solutionDetail != null)
            {
                return solutionDetail;
            }

            await ReloadEnvironmentSolutionsAsync(environmentConfig);
            return FindSolutionDetails(environmentConfig.Id, solutionId);
        }

        public async Task<List<SolutionDetail>> GetSolutionsDetailsAsync(EnvironmentConfig environmentConfig)
        {
            var solutionsDetails = FindSolutionsDetails(environmentConfig.Id);
            if (solutionsDetails.Count > 0)
            {
                return solutionsDetails;
            }

            await ReloadEnvironmentSolutionsAsync(environmentConfig);
            return FindSolutionsDetails(environmentConfig.Id);
        }

        private List<SolutionDetail> FindSolutionsDetails(Guid environmentConfigId)
        {
            return solutionDetails.Where(x => x.EnvironmentId == environmentConfigId).ToList();
        }

        private SolutionDetail FindSolutionDetails(Guid environmentConfigId, Guid solutionId)
        {
            return solutionDetails.FirstOrDefault(x => x.EnvironmentId == environmentConfigId && x.SolutionId == solutionId);
        }

        private async Task ReloadEnvironmentSolutionsAsync(EnvironmentConfig environmentConfig)
        {
            var solutionsResponse = await crmRequestsClient.GetSolutionsListAsync(environmentConfig.ConnectionString.BuildConnectionString());
            if (solutionsResponse.IsSuccessful)
            {
                var solutions = solutionsResponse.Payload;

                foreach (var solution in solutions)
                {
                    solution.EnvironmentId = environmentConfig.Id;
                    if (solutionDetails.Contains(solution))
                    {
                        continue;
                    }

                    solutionDetails.Add(solution);
                }
            }
            else
            {
                throw new Exception($"Failed to retrieve solutions list: {solutionsResponse.Error}");
            }
        }
    }
}

