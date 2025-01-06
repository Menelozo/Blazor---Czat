using Czat.Shared.DTOs;

namespace Czat.Shared
{
    public interface IBlazingChatHubServer
    {
        Task SetUserOnline(UserDto user);
    }
}
