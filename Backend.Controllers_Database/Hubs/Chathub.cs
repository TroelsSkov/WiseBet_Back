using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using WiseBet.backend.Data;
using WiseBet.backend.IRepository;
using WiseBet.backend.DTOs;


namespace WiseBet.backend.Hubs;

[Authorize]

public class Chathub : Hub
{
    private ChatsRepository _chatRepo;
    public Chathub(ChatsRepository chatRepo)
    {
        _chatRepo = chatRepo;
    }
    public async Task OnSendChatAsync(string chat)
    {
        var userString = this.Context.User?.FindFirst("UserRepoConnect")?.Value;
        if (userString == null || !Guid.TryParse(userString, out var userId))
            return;

        var username = this.Context.User?.FindFirst("Username")?.Value;

        if (string.IsNullOrWhiteSpace(chat) || chat.Length > 500)
            return;


        var dto = new ChatDto
        {
            ID = Guid.NewGuid(),
            UserId = userId,
            Message = chat,
            TimeOfChat = DateTime.Now,
            UserName = username
        };
        try
        {
            await _chatRepo.PostAsync(dto);
            await Clients.Caller.SendAsync("SendChat", dto);
        }
        catch (Exception)
        {
            await Clients.Caller.SendAsync("Error", "Beskeden kunne ikke sendes");
        }
    }

    public async Task OnChatLoadAsync()
    {
        try
        {
            var chats = await _chatRepo.GetAllAsync();
            await Clients.All.SendAsync("Chats", chats);
        }
        catch (Exception)
        {
            await Clients.Caller.SendAsync("Error", "Beskeder kunne ikke indlæses");
        }
    }
}
