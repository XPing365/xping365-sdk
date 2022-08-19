namespace XPing365.Shared
{
    internal static class Retry
    {
        public static async Task<T> DoAsync<T>(Func<Task<T>> action, TimeSpan retryInterval, int maxAttemptCount = 3)
        {
            maxAttemptCount.RequireCondition((i) => i > 0, nameof(maxAttemptCount), $"{nameof(maxAttemptCount)} has to be greater then 0");

            List<Exception> exceptions = new();

            for (int attempted = 0; attempted < maxAttemptCount; attempted++)
            {
                try
                {
                    if (attempted > 0)
                    {
                        Thread.Sleep(retryInterval);
                    }
                    return await action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            throw new AggregateException(exceptions);
        }
    }
}
