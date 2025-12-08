using System;
using System.Threading;
using System.Threading.Tasks;

namespace bot.BackgroundTasks
{
    public interface IBackgroundTask
    {
        Task Start(CancellationToken ct);
    }
}
