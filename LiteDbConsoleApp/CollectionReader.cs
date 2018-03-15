using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;

namespace LiteDbConsoleApp
{
    public class CollectionReader
    {
        private IEnumerable<BsonDocument> _enumerable;
        BlockingCollection<BsonDocument> _cachedItems = new BlockingCollection<BsonDocument>(5);
        CancellationTokenSource _cts = new CancellationTokenSource();

        public CollectionReader(IEnumerable<BsonDocument> enumerable)
        {
            _enumerable = enumerable;

            StartCaching();
        }

        public IEnumerable<BsonDocument> Read()
        {
            int count = 0;
            try
            {
                foreach (var item in _cachedItems.GetConsumingEnumerable())
                {
                    count++;
                    Console.WriteLine($"\t #{Thread.CurrentThread.ManagedThreadId}: Read ({count}): yield doc {item["IdProp"]}, {_cachedItems.Count} cached");
                    yield return item;
                }
                Console.WriteLine($"\t #{Thread.CurrentThread.ManagedThreadId}: Read after foreach: count {count}, {_cachedItems.Count} cached");
            }
            finally
            {
                Console.WriteLine($"\t #{Thread.CurrentThread.ManagedThreadId}: Read finally - cancel: count {count}, {_cachedItems.Count} cached");
                _cts.Cancel();
            }
        }

        private void StartCaching()
        {
            Task.Run(() =>
            {
                int count = 0;
                try
                {
                    foreach (var item in _enumerable)
                    {
                        count++;
                        Console.WriteLine($"\t #{Thread.CurrentThread.ManagedThreadId}: StartCaching ({count}): before adding doc {item["IdProp"]}, {_cachedItems.Count} cached");
                        _cachedItems.Add(item, _cts.Token);
                    }

                    Console.WriteLine($"\t #{Thread.CurrentThread.ManagedThreadId}: StartCaching CompleteAdding: count {count}, {_cachedItems.Count} cached");
                    _cachedItems.CompleteAdding();

                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine($"\t #{Thread.CurrentThread.ManagedThreadId}: StartCaching OperationCanceledException: count {count}, {_cachedItems.Count} cached");
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\t #{Thread.CurrentThread.ManagedThreadId}: StartCaching {ex.GetType().Name}: {ex.Message}, count {count}, {_cachedItems.Count} cached");
                    throw;
                }
            }, _cts.Token);
        }
    }
}
