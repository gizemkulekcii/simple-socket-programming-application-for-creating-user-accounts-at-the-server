using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Server
{
    public partial class Form1 : Form
    {
        
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        List<Socket> clientSockets = new List<Socket>();
        

        bool terminating=false;
        bool listening=false;

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void button_listen_Click(object sender, EventArgs e)
        {
            int serverPort;
          
            if (Int32.TryParse(textBox_port.Text, out serverPort))
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, serverPort);
                serverSocket.Bind(endPoint);
                serverSocket.Listen(3);

                listening = true;
                button_listen.Enabled = false;
                

                Thread acceptThread = new Thread(Accept);
                acceptThread.Start();

                serverlogs.AppendText("Started listening on port: " + serverPort + "\n");

            }
            else
            {
                serverlogs.AppendText("Check port number \n");
            }
            
        }
        private void Accept()
        {
            while (listening)
            {
                try
                {
                    Socket newClient = serverSocket.Accept();
                    clientSockets.Add(newClient);
                    serverlogs.AppendText("A client is connected.\n");

                    Thread receiveThread = new Thread(() => Receive(newClient)); 
                    receiveThread.Start();
                }
                catch
                {
                    if (terminating)
                    {
                        listening = false;
                    }
                    else
                    {
                        serverlogs.AppendText("The socket stopped working.\n");
                    }

                }
            }
        }
        private void Receive(Socket thisClient) 
        {
            bool connected = true;

            while (connected && !terminating)
            {
                try
                {
                    Byte[] buffer = new Byte[64];
                    thisClient.Receive(buffer);

                    string username = Encoding.Default.GetString(buffer);
                    username = username.Substring(0, username.IndexOf("\0"));
                    if (username == "DISCONNECT")
                    {

                        clientSockets.Remove(thisClient);
                        thisClient.Close();
                        connected = false;
                        serverlogs.AppendText("A client has disconnected!\n");

                    }
                    else
                    {
                        check_file("../../../../database.txt", username);
                    }
                 
                    


                }
                catch(Exception ex)
                {
                    if (!terminating)
                    {
                        serverlogs.AppendText(ex.Message);
                    }
                    thisClient.Close();
                    clientSockets.Remove(thisClient);
                    connected = false;
                }
            }
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listening = false;
            terminating = true;
            Byte[] buffer = Encoding.Default.GetBytes("DISCONNECT");
            foreach (Socket client in clientSockets) 
            {
                client.Send(buffer);
            }
            Environment.Exit(0);
            
        }
        private void check_file(string filename, string username) 
        {
            string line;
           
            System.IO.StreamReader file = new System.IO.StreamReader(filename);
            bool Flag = false;
            
            while ((line = file.ReadLine()) != null)
            {
                var uname = line.Split(' ');

                if (uname[0] == username)
                {
                    serverlogs.AppendText("An account with the " + username + " already exists!\n");
                    Flag = true;
                }
                
            }
            file.Close();
            
            if (Flag == false) 
            {

                serverlogs.AppendText(username + " has created an account!\n");
                  
            }
           
        }

        

       
    }
}
