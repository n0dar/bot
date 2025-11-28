using System.Collections.Generic;
using System.Linq;

namespace bot.Helpers
{
    public static class EnumerableExtension
    {
        public static IEnumerable<int> GetBatchByNumber(int batchSize, int batchNumber)
        {
            return Enumerable.Range(batchNumber * batchSize, batchSize);
        }
    }
}
