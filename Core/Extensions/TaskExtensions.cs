using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Extensions.Tasks
{
    internal static class TaskExtensions
    {
        public static T WaitForTask<T>(this Task<T> task)
        {
            try
            {
                if (task.Status == TaskStatus.Created)
                    task.Start();

                task.Wait();
                return task.Result;
            }
            catch (AggregateException agg)
            {
                throw agg.Flatten().InnerExceptions.First();
            }
        }

        public static System.Net.Http.HttpResponseMessage WaitForTask(this Task<System.Net.Http.HttpResponseMessage> task)
        {
            try
            {
                return WaitForTask<System.Net.Http.HttpResponseMessage>(task);
            }
            catch(TaskCanceledException cancel)
            {
                throw new TimeoutException("HttpRequest timed out", cancel);
            }
        }
    }
}
