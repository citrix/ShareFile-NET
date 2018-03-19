using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Extensions.Tasks
{
    internal static class TaskExtensions
    {
        public static T WaitForTask<T>(this Task<T> task)
        {
            try
            {
                task.Wait();
                return task.Result;
            }
            catch (AggregateException agg)
            {
                throw agg.Unwrap();
            }
        }

        public static void WaitForTask(this Task task)
        {
            try
            {
                task.Wait();
            }
            catch (AggregateException agg)
            {
                throw agg.Unwrap();
            }
        }

        public static Exception Unwrap(this AggregateException agg)
        {
            Exception ex = agg.Flatten();
            while (ex is AggregateException)
                ex = (ex as AggregateException).InnerException;
            return ex;
        }

        public static void Rethrow(this Task task)
        {
            //for continuations
            if (task.Exception != null)
                throw task.Exception.Unwrap();
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

        public static void Forget(this Task task)
        {
            task.ConfigureAwait(false);
        }
    }
}
