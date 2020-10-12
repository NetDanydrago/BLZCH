using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BLZCH.Shared
{
    public interface IChatClient
    {
        Task ReceiveMessage(string user, string message);
        Task UserConnected(string user);
        Task UserDisconnected(string user);
        Task NotifyQuestions();
    }
}
