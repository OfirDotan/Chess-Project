using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace FinalProjectChess
{
    public static class ServerCommunication
    {
        private const string serverIP = "10.0.0.60";
        private const int serverPort = 8452;
        public static TcpClient socket;

        public static string authenticate(string username, string password)
        {
            TcpClient client = new TcpClient(serverIP, serverPort);

            //Write username + password to server
            NetworkStream stream = client.GetStream();
            byte[] data = Encoding.ASCII.GetBytes("l%" + username + "#" + password);
            stream.Write(data, 0, data.Length);

            // Reading data
            byte[] buffer = new byte[1024]; // Adjust the buffer size as needed
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);


            if (receivedData == "Server Login Accepted")
            {
                socket = client;

                //Write username + password to server
                stream = client.GetStream();
                data = Encoding.ASCII.GetBytes("Client Login Accepted");
                stream.Write(data, 0, data.Length);


                // Reading data
                buffer = new byte[1024]; // Adjust the buffer size as needed
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                return receivedData;
            }
            else
            {
                socket = null;
                return null;
            }
        }
        public static void closeSocket() {
           if(socket != null)
            {
                //Sends notification
                NetworkStream stream = socket.GetStream();
                byte[] data = Encoding.ASCII.GetBytes("Disconnected");
                stream.Write(data, 0, data.Length);


                // Close the socket
                socket.Close();

                socket = null;
            }
        }
        public static void send(string message)
        {
            if (socket != null)
            {
                NetworkStream stream = socket.GetStream();
                byte[] data = Encoding.ASCII.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }    
        }
        public static string receive(int byteLength, int timeoutInMilliseconds)
        {

            if (socket == null || !socket.Connected)
            {
                return null; // Return null for invalid or disconnected socket
            }
            if (byteLength == -1)
            {
                byteLength = 1024;
            }
            using (var networkStream = new NetworkStream(socket.Client))
            {
                networkStream.ReadTimeout = timeoutInMilliseconds;
                byte[] buffer = new byte[byteLength]; // Adjust the buffer size as needed

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                while (stopwatch.ElapsedMilliseconds <= timeoutInMilliseconds)
                {
                    try
                    {
                        int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            return Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        }
                    }
                    catch (TimeoutException ex)
                    {
                        // Handle timeout specifically
                        return null;
                    }
                    catch (Exception ex)
                    {
                        // Handle other exceptions
                        return null;
                    }
                }

                // No data received within the timeout
                return null;
            }
        }
        public static string sendAndReceive(string message)
        {
            if(socket != null)
            {
                //Write username + password to server
                NetworkStream stream = socket.GetStream();
                byte[] data = Encoding.ASCII.GetBytes(message);
                stream.Write(data, 0, data.Length);

                return receive(1024, 5000);
            }
            return "";
        }

        public static string signUp(string username, string password)
        {
            try
            {
                using (TcpClient client = new TcpClient(serverIP, serverPort))
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] data = Encoding.ASCII.GetBytes("s%" + username + "#" + password);
                    stream.Write(data, 0, data.Length);

                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    if (!receivedData.Equals("SignUpAccepted"))
                    {
                        if (receivedData.Equals("UsernameTaken"))
                        {
                            return "UsernameTaken";
                        }
                        else
                        {
                            return "Error";
                        }
                    }
                    else
                    {
                        return "AccountCreated";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }

        }
    }
}