using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Client
{
    public partial class Form1 : Form
    {
        class Users
        {
            public string username;
            public string name;
            public string surname;
            public string password;


            public Users(string username, string name, string surname, string password)
            {
                this.username = username;
                this.name = name;
                this.surname = surname;
                this.password = password;


            }
        }

        bool terminating = false;
        bool connected = false;
        Socket clientSocket;

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void button_connect_Click(object sender, EventArgs e)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string IP = textBox_ip.Text;

            int portNum;
            if (Int32.TryParse(textBox_port.Text, out portNum))
            {
                try
                {
                    clientSocket.Connect(IP, portNum);
                    button_connect.Enabled = false;
                    textBox_name.Enabled = true;
                    textBox_surname.Enabled = true;
                    textBox_username.Enabled = true;
                    textBox_password.Enabled = true;
                    button_createaccount.Enabled = true;
                    button_disconnect.Enabled = true;
                    connected = true;
                    clientlogs.AppendText("You are connected!\n");

                    Thread receiveThread = new Thread(Receive);
                    receiveThread.Start();

       

                }
                catch
                {
                    clientlogs.AppendText("Could not connect to the server!\n");
                }
            }
            else
            {
                clientlogs.AppendText("Check the port number\n");
            }

        }
        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connected = false;
            terminating = true;
            if (button_disconnect.Enabled == true)
            {
                Byte[] buffer = Encoding.Default.GetBytes("DISCONNECT");
                clientSocket.Send(buffer);
            }
            Environment.Exit(0);
            
        }

        private void button_createaccount_Click(object sender, EventArgs e)
        {
          
            string username = textBox_username.Text;
            string name = textBox_name.Text;
            string surname = textBox_surname.Text;
            string password = textBox_password.Text;

            Users user = new Users(username,name, surname, password);
            

            if(name =="")
            {
                clientlogs.AppendText("Please enter a name\n");
            }
            if (surname == "")
            {
                clientlogs.AppendText("Please enter a surname\n");
            }
            if (username== "")
            {
                clientlogs.AppendText("Please enter an username\n");
            }
            if (password == "")
            {
                clientlogs.AppendText("Please enter a password\n");
            }
           
            if (name!=""&& surname !=""&& password!=""&& username != "" && username.Length <= 64)
            {
                Byte[] buffer = Encoding.Default.GetBytes(username);
                clientSocket.Send(buffer);
                check_file("../../../../database.txt", user);
               
               
            }
        }

        private void button_disconnect_Click(object sender, EventArgs e)
        {
            string message = "DISCONNECT";
            Byte[] buffer = Encoding.Default.GetBytes(message);
            clientSocket.Send(buffer);
            button_disconnect.Enabled = false;
            button_connect.Enabled = true;
            button_createaccount.Enabled = false;
            textBox_name.Enabled = false;
            textBox_surname.Enabled = false;
            textBox_username.Enabled = false;
            textBox_password.Enabled = false;
            connected = false;
            clientSocket.Disconnect(false);
            
         
            if (clientSocket.Connected)
                clientlogs.AppendText(" still connnected\n");
            else
                clientlogs.AppendText(" Succesfully disconnected\n");
            
        }
        private void check_file(string filename,  Users user )
        {
            string line;

           
            bool Flag = false;
            System.IO.StreamReader file2 = new System.IO.StreamReader(filename);
            while ((line = file2.ReadLine()) != null)
            {
                var uname = line.Split(' ');
                if (uname[0] == user.username)
                {
                    clientlogs.AppendText("There is already an account with this username!\n");
                    Flag = true;
                }
            }
            file2.Close();
            
            if (Flag == false)
            {
                using (StreamWriter file = new StreamWriter(filename, append: true))
                {
                    file.WriteLine(user.username+" "+user.name+" "+user.surname+" "+user.password);
                    file.Close();
                    clientlogs.AppendText("You have created a new account!\n");
                   
                    

                }
               

            }
        }

        private void Receive()
        {
            while (connected)
            {
                Byte[] buffer = new Byte[64];
                clientSocket.Receive(buffer);
                string msg = Encoding.Default.GetString(buffer);
                msg = msg.Substring(0, msg.IndexOf("\0"));
                if (msg == "DISCONNECT")
                {
                    clientlogs.AppendText("The server has disconnected!\n");
                    clientSocket.Close();
                }
            }
            
        }
    }
}
