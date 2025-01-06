using Czat.Shared.DTOs;

namespace Czat.Shared
{
    public interface IBlazingChatHubClient
    {
        Task UserConnected(UserDto user);
        Task OnlineUsersList(IEnumerable<UserDto> users);
        Task UserIsOnline(int userId);

        Task MessageRecieved(MessageDto messageDto);
    }
}
