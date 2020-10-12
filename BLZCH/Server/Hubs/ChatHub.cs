using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLZCH.Shared;
using System.Collections.Concurrent;

namespace BLZCH.Server.Hubs
{
    public class ChatHub: Hub<IChatClient>
    {

        const string IdentifiedUsers = "Identified Users";
        const string ModeratorUsers = "Moderator Users";
        static List<QuestionUsers> QuestionUsers = new List<QuestionUsers>();
        static ConcurrentDictionary<string, ChatUser> Users =
            new ConcurrentDictionary<string, ChatUser>();
        static ConcurrentDictionary<string, ChatUser> Moderators =
            new ConcurrentDictionary<string, ChatUser>();

        public async Task SendQuestion(string message)
        {
            ChatUser User = GetCurrentChatUser();
            if(User != null)
            {
                QuestionUsers.Add(new Shared.QuestionUsers()
                {
                    ChatUser = User,
                    Question = message,
                    IsAnswered = false
                });
                await Clients.Group(ModeratorUsers)
                       .NotifyQuestions();
            }
        }

        public async Task SendMessageModerator(string message)
        {
            ChatUser User = GetCurrentChatUser();
            if (User != null)
            {
                await Clients.Group(IdentifiedUsers)
                    .ReceiveMessage(User.NickName, message);
            }
        }

        public async Task<List<QuestionUsers>> GetQuestions()
        {
            List<QuestionUsers> Questions = new List<QuestionUsers>();
            if (GetCurrentModeratorUser() is ChatUser ChatUser)
            {
                Questions = QuestionUsers.Where
                    (q => q.IsAnswered == false).ToList(); 
            }
            return await Task.FromResult(Questions);
        }

        public async Task<string[]> GetUserList()
        {
            string[] Result;
                if(GetCurrentChatUser() is ChatUser ChatUser)
            {
                Result = Users.Select(u => u.Key).ToArray();
            }
            else
            {
                Result = new string[0];
            }
            return await Task.FromResult(Result);
        }

        public async Task<bool> SingIn(string nickName)
        {
            bool Result = false;
            if(Users.TryAdd(nickName,new 
                ChatUser(nickName,Context.ConnectionId)))
            {
                await Groups.AddToGroupAsync
                    (Context.ConnectionId,IdentifiedUsers);
                await Clients.GroupExcept(IdentifiedUsers, Context.ConnectionId)
                    .UserConnected(nickName);    
                Result = true;
            }
            return Result;
        }

        public async Task<bool> SingInModerator(string nickName)
        {
            bool Result = false;
            if (Moderators.TryAdd(nickName, new
                ChatUser(nickName, Context.ConnectionId)))
            {
                await Groups.AddToGroupAsync
                    (Context.ConnectionId, ModeratorUsers);
                await Clients.GroupExcept(ModeratorUsers, Context.ConnectionId)
                    .UserConnected(nickName);
                Result = true;
            }
            return Result;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if(GetCurrentChatUser() is ChatUser ChatUser)
            {
                Users.TryRemove(ChatUser.NickName, out var value);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId
                    , IdentifiedUsers);
                await Clients.Group(IdentifiedUsers)
                    .UserDisconnected(ChatUser.NickName);
            }
        }

        ChatUser GetCurrentChatUser()
        {
            return Users.Where(
                u => u.Value.ConnectionId == Context.ConnectionId)
                .Select(u => u.Value).FirstOrDefault();
        }

        ChatUser GetCurrentModeratorUser()
        {
            return Moderators.Where(
                u => u.Value.ConnectionId == Context.ConnectionId)
                .Select(u => u.Value).FirstOrDefault();
        }

    }
}
