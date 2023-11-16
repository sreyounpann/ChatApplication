using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApplication
{
    public partial class Form1 : Form
    {
        private bool isDragging = false;
        private Point startPoint;
        private TcpClient client;
        private NetworkStream stream;
        private string username;
        private const string ClientListPrefix = "[ClientList]";
        private bool isClosing = false;

        public Form1()
        {
            InitializeComponent();
            txtMessage.Enabled = false;
            btnSend.Enabled = false;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                
                username = textBox1.Text.Trim();

                if (!string.IsNullOrEmpty(username))
                {
                    
                    string serverIp = "127.0.0.1"; 
                    int serverPort = 8888; 

                    // Create a TcpClient instance and connect to the server
                    client = new TcpClient();
                    client.Connect(serverIp, serverPort);

                    // Get the network stream from the TcpClient for sending and receiving data
                    stream = client.GetStream();

                    // Send the chosen username to the server
                    byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
                    stream.Write(usernameBytes, 0, usernameBytes.Length);

                    // Start a separate thread to receive messages
                   Task.Run(ReceiveMessages);

                    txtMessage.Enabled = true;
                    btnSend.Enabled = true;

                    
                    btnConnect.Enabled = false;

                    string currentTime = DateTime.Now.ToString("HH:mm:ss");
                   

                    AddMessageToChatBox($" [{currentTime}] Connected to the server as: {username}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to the server: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReceiveMessages()
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead;

                while (!isClosing) 
                {
                    try
                    {
                        bytesRead = stream.Read(buffer, 0, buffer.Length);

                        if (bytesRead == 0)
                        {
                            AddMessageToChatBox("Disconnected from the server.");
                            // close the client-side network stream
                            stream.Close();
                            client.Close();


                            txtMessage.Enabled = false;
                            btnSend.Enabled = false;
                            btnConnect.Enabled = true;
                            return;
                        }

                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);


                        if (message.Contains("has disconnected from the chat."))
                        {
                            // Display the disconnection message in the chatTextBox
                            AddMessageToChatBox(message);
                        }
                        else
                        {
                            // Process regular messages from the server
                            if (message.StartsWith(ClientListPrefix))
                            {
                                string jsonClientList = message.Substring(ClientListPrefix.Length);
                                UpdateContactList(jsonClientList);
                            }
                            else
                            {
                                string[] messageParts = message.Split(new string[] { ": " }, StringSplitOptions.None);
                                if (messageParts.Length == 2)
                                {
                                    string senderName = messageParts[0];
                                    string actualMessage = messageParts[1];

                                    string currentTime = DateTime.Now.ToString("HH:mm:ss");

                                    AddMessageToChatBox($"[{currentTime}] " + $"[ {senderName} ] : "  + $"[{actualMessage}]");
                                }
                                else
                                {
                                    // Process regular messages from the server
                                    AddMessageToChatBox(message);
                                }
                            }
                        }
                    }
                    catch (IOException)
                    {
                      
                        stream.Close();
                        client.Close();
                     

                        txtMessage.Enabled = false;
                        btnSend.Enabled = false;
                        btnConnect.Enabled = true;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // Display any other errors that occur during the receiving process using a MessageBox
                if (!isClosing)
                {
                    isClosing = true; // Set the flag to prevent recursive closing
                    MessageBox.Show($"Error receiving messages: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    isClosing = false; // Reset the flag after displaying the message
                }
            }
        }



        private void AddMessageToChatBox(string message)
        {
            if (!IsDisposed && chatTextBox.InvokeRequired)
            {
                chatTextBox.Invoke(new MethodInvoker(() => AddMessageToChatBox(message)));
            }
            else if (!IsDisposed)
            {
                chatTextBox.AppendText(message + Environment.NewLine);
                chatTextBox.SelectionStart = chatTextBox.Text.Length;
                chatTextBox.ScrollToCaret();
            }
        }


        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            startPoint = new Point(e.X, e.Y);
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

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void UpdateContactList(string jsonClientList)
        {
            try
            {
                List<string> clientNames = JsonConvert.DeserializeObject<List<string>>(jsonClientList);

                if (contactListBox.InvokeRequired)
                {
                    contactListBox.Invoke(new MethodInvoker(() => UpdateContactList(jsonClientList)));
                }
                else
                {
                    contactListBox.Items.Clear();
                    foreach (string clientName in clientNames)
                    {
                        contactListBox.Items.Add(clientName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating contact list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CloseForm()
        {
            try
            {
                // Set the flag to indicate that the client is closing
                isClosing = true;

                // Check if client and stream are not null and connected before attempting to disconnect
                if (client != null && client.Connected && stream != null)
                {
                    // Send a disconnect message to the server
                    string disconnectMessage = "[DISCONNECT]";
                    byte[] disconnectBytes = Encoding.UTF8.GetBytes(disconnectMessage);
                    stream.Write(disconnectBytes, 0, disconnectBytes.Length);

                    // Perform cleanup and close the client-side network stream and TcpClient
                    stream.Close();
                    client.Close();
                }

                // Optionally, display a message to the user indicating disconnection
                AddMessageToChatBox("Disconnected from server.");

            
                this.Close();
            }
            catch (IOException)
            {
               
                // Close the form (application)
                this.Close();
            }
            catch (Exception ex)
            {
              
                if (!isClosing)
                {
                    isClosing = true; // Set the flag to prevent recursive closing
                    MessageBox.Show($"Error disconnecting from the server: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    isClosing = false; // Reset the flag after displaying the message
                }
            }
        }

        private void labelClose_Click_1(object sender, EventArgs e)
        {
            CloseForm();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                string message = txtMessage.Text.Trim();
                if (!string.IsNullOrEmpty(message))
                {
                    // Get the current time 
                    string currentTime = DateTime.Now.ToString("HH:mm:ss");

          
                    AddMessageToChatBox($"[{currentTime}] [You]: {message}");

                    // Send the mmss to the server
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                    stream.Write(messageBytes, 0, messageBytes.Length);
                    txtMessage.Clear();
                }
            }
            catch (Exception ex)
            {
                // Display any errors that happen during the send process using a MessageBox
                MessageBox.Show($"Error sending message: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
