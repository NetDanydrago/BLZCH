using System;
using System.Collections.Generic;
using System.Text;

namespace BLZCH.Shared
{
    public class ChatUser
    {
        public ChatUser()
        {

        }

        public ChatUser(string nickName, string connectionId) =>
            (NickName, ConnectionId) = (nickName, connectionId);

        public string NickName { get; set; }
        public string ConnectionId { get; set; }
    }
}
