using Newtonsoft.Json;
using System.Text;

namespace Orleans.Runtime.Configuration
{

    public interface IMessageSerializationHandler
    {
        byte[] SerializeMessage<T>(T data);
        T DeserializeMessage<T>(byte[] data);
    }
    public class DefaultSerializationHandler : IMessageSerializationHandler
    {
        public T DeserializeMessage<T>(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public byte[] SerializeMessage<T>(T data)
        {
            var json = JsonConvert.SerializeObject(data);
            return Encoding.UTF8.GetBytes(json);

        }
    }

}
