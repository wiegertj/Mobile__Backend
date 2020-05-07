
using System.Threading.Tasks;

namespace Contracts
{
    public interface IPushSender
    {
        void SendGroupPush(long groupId, long entryId, string text);
        void SendSubGroupPush(long groupId, long entryId, string text);
    }
}
