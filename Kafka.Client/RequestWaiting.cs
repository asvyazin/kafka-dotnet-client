using System.Threading.Tasks;
using Kafka.Client.Messages;

namespace Kafka.Client
{
	public class RequestWaiting
	{
		public TaskCompletionSource<ResponseMessage> TaskCompletionSource { get; set; }
		public ApiKey ApiKey { get; set; }
	}
}