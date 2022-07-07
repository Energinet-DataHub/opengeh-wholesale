using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabricksJobRunner
{
    internal interface IJobRunnerPort : IDisposable
    {
        Task<JobRunId> SubmitJobAsync();
        Task<JobState> GetJobStateAsync(JobRunId jobRunId);
    }
}
