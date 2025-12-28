using System.Threading.Channels;
using BlastTicket.Core.Models;

namespace BlastTicket.Infra.Background
{
    //インターフェース定義
    public interface IOrderQueue
    {
        //データ書き込み
        bool TryWrite(Order order);

        //データ読み出し
        IAsyncEnumerable<Order> ReadAllAsync(CancellationToken ct);
    }

    public class OrderQueue : IOrderQueue
    {
        private readonly Channel<Order> _channel;

        public OrderQueue()
        {
            // BoundedChannelOptions: メモリ溢れを防ぐため、上限を設定（堅牢性）
            // FullMode.DropWrite データが溢れたらエラーを返す
            var options = new BoundedChannelOptions(100_000) // チャンネルの容量を100,000に設定
            {
                FullMode = BoundedChannelFullMode.DropWrite
            };
            _channel = Channel.CreateBounded<Order>(options);
        }

        public bool TryWrite(Order order)
        {
            return _channel.Writer.TryWrite(order);
        }

        public IAsyncEnumerable<Order> ReadAllAsync(CancellationToken ct)
        {
            return _channel.Reader.ReadAllAsync(ct);
        }
    }
}
