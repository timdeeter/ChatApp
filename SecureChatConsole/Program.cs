using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;


namespace SecureChatConsole
{
    class Program
    {
        static string url = "http://chatconsolesocket.azurewebsites.net/";
        static string connectedServerKey = "";
        static string username = "AnonymousUser";
        static bool connected = false;
        static HubConnection ConnectionHub;
        static IHubProxy ConnectionProxy;
        
        [STAThread]
        static void Main(string[] args)
        {
            var connectedServerKey = "";
            Format();
            Console.WriteLine("Type 'chat' for a list of commands");
            while (true)
            {
                var response = Console.ReadLine();
                ProcessInput(response);
                
            }
            
        }

        public static void Format()
        {
            Console.Clear();
            Console.WriteLine("Secure Console Chat");
        }

        //[STAThread]
        public static void ProcessInput(string input)
        {
            input = input.ToLower();
            if (input == "chat")
            {
                ListCommands();
            }
            else if (input.Substring(0,11) == "chat create")
            {
                string key = input.Substring(12);
                Format();
                Console.WriteLine("You've created a chat with the key (Case Sensitive): " + key);
                Clipboard.SetText(key);
                Console.WriteLine("The key has been copied to your clipboard. Perform a connection command to join.");
            }
            else if (input.Substring(0,12) == "chat connect")
            {
                Format();
                string key = input.Substring(13);
                Console.WriteLine("Enter a username for this chat:");
                username = Console.ReadLine();
                ChatConnect(key);
                connected = true;
                while (connected)
                {
                    var response = Console.ReadLine();
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    ClearCurrentConsoleLine();
                    SendMessage(response);
                }
            }
            else
            {
                Format();
                Console.WriteLine("You typed '" + input + "'. That is not a valid command.");
                Console.WriteLine("Type 'chat' for a list of commands");
            }
        }

        public static void ListCommands()
        {
            Format();
            Console.WriteLine("chat create [key] - Creates a new chat with a private key.");
            Console.WriteLine("chat connect [key] - joins a chat with the provided key.");
        }

        public static void PostMessage(MessageReceive data)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("<" + data.Username + "> ");
            Console.ResetColor();
            Console.WriteLine(data.Message);

        }

        public static void SendMessage(string message)
        {
            PostMessage(new MessageReceive()
            {
                Message = message,
                Username = username
            });
            ConnectionProxy.Invoke("Send",
            new Object[] {
                username,
                message,
                connectedServerKey
            });
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public static async void ChatConnect(string key)
        {
            connectedServerKey = key;
            ConnectionHub = new HubConnection(url);
            ConnectionProxy = ConnectionHub.CreateHubProxy("ChatHub");
            ConnectionProxy.On<string, string>("addMessage", (name, message) => 
                PostMessage(new MessageReceive()
                {
                    Username = name,
                    Message = message
                })
            );
            long start = DateTime.Now.Ticks;
            await ConnectionHub.Start();

            //await chatHubProxy.Invoke("JoinChat", connectedServerKey);

            await ConnectionProxy.Invoke("Send",
            new Object[] {
                username,
                "Test Message",
                connectedServerKey
            });

            long end = DateTime.Now.Ticks;
            decimal elapsed = (end - start)/10000;
            elapsed = elapsed / 1000;
            Console.WriteLine("Connected to " + url + " in " + elapsed + " seconds." );
            
        }

        public static bool Register()
        {
            return true;
        }
    }
}
