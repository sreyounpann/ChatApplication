using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace ServerApplications
{
    public partial class Form1 : Form
    {
        private bool isDragging = false;
        private Point startPoint;
        private TcpListener serverListener;
        private List<ConnectedClient> connectedClients;
        private const string ClientListPrefix = "[ClientList]";


        public Form1()
        {
            InitializeComponent();
            connectedClients = new List<ConnectedClient>();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StartServer();
        }

        private void StartServer()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                int port = 8888;

                serverListener = new TcpListener(ipAddress, port);
                serverListener.Start();

                Thread acceptThread = new Thread(AcceptClientConnections);
                acceptThread.Start();

                AddMessageToChatBox("Server started. Listening for incoming connections...");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting server: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

         
        }



        private void AcceptClientConnections()
        {
            while (true)
            {
                try
                {
                    TcpClient client = serverListener.AcceptTcpClient();
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error accepting client connection: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



        private void HandleClient(TcpClient client)
        {
            try
            {
                // Create a NetworkStream to send and receive data
                NetworkStream stream = client.GetStream();

                // Receive the username from the client
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string username = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                ConnectedClient connectedClient = new ConnectedClient(client, username);
                connectedClients.Add(connectedClient);

                AddMessageToChatBox("[" + DateTime.Now + "]" + $" Client {connectedClient.Name} connected.");

                // Broadcast the updated client list to all connected clients
                BroadcastClientList();
                Thread clientThread = new Thread(() => HandleClientMessages(connectedClient));
                clientThread.Start();
              
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error handling client: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void HandleClientMessages(ConnectedClient connectedClient)
        {
            try
            {
                NetworkStream stream = connectedClient.Stream;
                byte[] buffer = new byte[1024];
                int bytesRead;

                while (true)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);

                    // If bytesRead is 0, the client  disconnected
                    if (bytesRead == 0)
                    {
                        string currentTime = DateTime.Now.ToString("HH:mm:ss");
                        AddMessageToChatBox($"{currentTime}" + $" Client {connectedClient.Name} disconnected.");

                    
                        string disconnectMessage = $"{connectedClient.Name} has disconnected from the chat.";
                        BroadcastMessageToClients(disconnectMessage, connectedClient.Name);

                        connectedClients.Remove(connectedClient);

                        // Broadcast the updated client list to all connected clients
                        BroadcastClientList();

                        return;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    if (message.ToUpper() == "[DISCONNECT]")
                    {
                        string currentTime = DateTime.Now.ToString("HH:mm:ss");
                        AddMessageToChatBox($"{currentTime}" + $" Client {connectedClient.Name} disconnected.");

                        // inform all clients that the connectedClient has disconnected
                        string disconnectMessage = $"{connectedClient.Name} has been disconnected from the chat.";
                        BroadcastMessageToClients(disconnectMessage, connectedClient.Name);

                        connectedClients.Remove(connectedClient);

                        // Broadcast the updated client list to all connected clients
                        BroadcastClientList();

                        return;
                    }
                    else
                    {
                        AddMessageToChatBox($"{connectedClient.Name}: {message}");

                        // Broadcast the message to all other connected clients
                        BroadcastMessageToClients($"{connectedClient.Name}: {message}", connectedClient.Name);
                    }
                }
            }
            catch (IOException)
            {
                // Handle the IOException when the client disconnects unexpectedly
                AddMessageToChatBox($"Client {connectedClient.Name} disconnected unexpectedly.");
                connectedClients.Remove(connectedClient);

                // Notify all clients that the connectedClient has disconnected
                string disconnectMessage = $"{connectedClient.Name} has disconnected from the chat.";
                BroadcastMessageToClients(disconnectMessage, connectedClient.Name);

                // Broadcast the updated client list to all connected clients
                BroadcastClientList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error handling client messages: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void BroadcastMessageToClients(string message, string senderName)
        {
            try
            {
                foreach (ConnectedClient connectedClient in connectedClients)
                {
                    if (connectedClient.Name != senderName)
                    {
                        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                        connectedClient.Stream.Write(messageBytes, 0, messageBytes.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error broadcasting message to clients: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddMessageToChatBox(string message)
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.Invoke(new MethodInvoker(() => AddMessageToChatBox(message)));
            }
            else
            {
                textBox1.AppendText(message + Environment.NewLine);
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.ScrollToCaret();
            }
        }

        public class ConnectedClient
        {
            private TcpClient client;
            private Thread receiveThread;

            public string Name { get; private set; }
            public NetworkStream Stream { get; private set; }

            public ConnectedClient(TcpClient client, string username)
            {
                this.client = client;
                this.Name = username;
                this.Stream = client.GetStream();
            }
        }

        private void BroadcastClientList()
        {
            try
            {
                List<string> clientNames = connectedClients.Select(client => client.Name).ToList();

      
                string jsonClientList = JsonConvert.SerializeObject(clientNames);

                // Add the ClientListPrefix to the JSON string
                string prefixedJsonClientList = ClientListPrefix + jsonClientList;

                // Convert the JSON string to bytes and send it to all connected clients
                byte[] clientListBytes = Encoding.UTF8.GetBytes(prefixedJsonClientList);
                foreach (ConnectedClient connectedClient in connectedClients)
                {
                    if (connectedClient.Stream.CanWrite)
                    {
                        connectedClient.Stream.Write(clientListBytes, 0, clientListBytes.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error broadcasting client list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point mousePos = MousePosition;
                mousePos.Offset(-startPoint.X, -startPoint.Y);
                Location = mousePos;
            }
        }

        private void labelClose_Click(object sender, EventArgs e)
        {
            Application.Exit(); 
        }
    }
}
