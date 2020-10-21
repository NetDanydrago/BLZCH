using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLZCH.Shared;
using System.Collections.Concurrent;
using System.Text;

namespace BLZCH.Server.Hubs
{
    public class ChatHub: Hub<IChatClient>
    {

        const string IdentifiedUsers = "Identified Users";
        const string ModeratorUsers = "Moderator Users";
        static int Aux = 0;
        static ConcurrentDictionary<int, QuestionUsers> QuestionUsers =
            new ConcurrentDictionary<int, QuestionUsers>();
        static ConcurrentDictionary<string, ChatUser> Users =
            new ConcurrentDictionary<string, ChatUser>();
        static ConcurrentDictionary<string, ChatUser> Moderators =
            new ConcurrentDictionary<string, ChatUser>();

        public async Task SendQuestion(string message)
        {
            ChatUser User = GetCurrentChatUser();
            if(User != null)
            {
                Aux++;
                int Id = Aux;
                QuestionUsers.TryAdd(Id,new Shared.QuestionUsers()
                {
                    Id = Id,
                    ChatUser = User,
                    Question = message,
                    IsAnswered = StateAnswer.NotAnswered
                });
                await Clients.Group(ModeratorUsers)
                       .NotifyQuestions();
            }
        }

        public async Task SendMessageModerator(string message,QuestionUsers questionUser)
        {
            ChatUser User = GetCurrentModeratorUser();
            if (User != null)
            {
                QuestionUsers.TryRemove(questionUser.Id, out questionUser);
                var Builder = new StringBuilder();
                Builder.Append($"Pregunta : {questionUser.Question}");
                Builder.Append($"Respuesta: {message}");
                await Clients.Client(questionUser.ChatUser.ConnectionId)
                    .ReceiveMessage(User.NickName, Builder.ToString());
                await Clients.Group(ModeratorUsers).NotifyQuestions();
            }
        }

        public async Task ChangeStateAnswer(QuestionUsers questionUsers, StateAnswer state)
        {
            QuestionUsers.TryGetValue(questionUsers.Id, out Shared.QuestionUsers Question);
            Question.IsAnswered = state;
            await Clients.Group(ModeratorUsers).NotifyQuestions();
        }

        public async Task<List<QuestionUsers>> GetQuestions()
        {
            List<QuestionUsers> Questions = new List<QuestionUsers>();
            if (GetCurrentModeratorUser() is ChatUser)
            {
                 Questions = QuestionUsers.Values.ToList(); 
   
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
            if(GetCurrentModeratorUser() is ChatUser Moderator)
            {
                Moderators.TryRemove(Moderator.NickName, out var value);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId
                    ,IdentifiedUsers);
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
