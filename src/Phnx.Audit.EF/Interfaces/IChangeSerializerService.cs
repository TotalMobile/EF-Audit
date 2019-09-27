using Newtonsoft.Json;

namespace Phnx.Audit.EF
{
    public interface IChangeSerializerService
    {
        string Serialize(object data);
    }
}