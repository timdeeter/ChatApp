using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace SignalRChat
{
    public class ChatHub : Hub
    {
        public void Send(string name, string message, string connectedKey)
        {
            Clients.OthersInGroup(connectedKey).addChatMessage(name, message);
            //Clients.All.addMessage(name, message);
        }

        public void JoinChat(string connectedKey)
        {
            Groups.Add(Context.ConnectionId, connectedKey);
            Clients.OthersInGroup(connectedKey).addChatMessage("Server", "Someone has joined the server.");

        }
    }
}