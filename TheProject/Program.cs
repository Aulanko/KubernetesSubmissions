

using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;



namespace TheProject
{
    class Program
    {
        
        
        private static TcpListener myListener;

        private static int port = 3001;



        private static IPAddress localAddress = IPAddress.Any;

        //web server path

        private static string WebServerPath = @"WebServer";


        static void Main(string[] args)
        {
            myListener = new TcpListener(localAddress, port);
                myListener.Start();
            Console.WriteLine($"Web Server Running on localhost:{port}, which is the same as {localAddress.ToString()}:{port} which uses port {port}.. Press ctrl+C to stop..");
                Thread th = new Thread(new ThreadStart(StartListen));
                th.Start();

        }

        static void StartListen()
{
    while (true)
    {
        TcpClient client = myListener.AcceptTcpClient();
        NetworkStream stream = client.GetStream();

        // 1. Read the browser's HTTP request (optional, but good for debugging)
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string requestText = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine($"\n--- Received Request ---\n{requestText}");

        // 2. Prepare the HTML body
        string htmlBody = "<html><body><h1>Hello World from my C# Server!</h1></body></html>";
        byte[] htmlBytes = System.Text.Encoding.UTF8.GetBytes(htmlBody);

        // 3. Build a valid HTTP response header
        // Browsers need to know it's a "200 OK" success, and how long the content is.
        string httpHeader = "HTTP/1.1 200 OK\r\n" +
                            "Content-Type: text/html; charset=UTF-8\r\n" +
                            $"Content-Length: {htmlBytes.Length}\r\n" +
                            "Connection: close\r\n\r\n"; // Note the extra \r\n\r\n at the end!
        byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(httpHeader);

        // 4. Send the header followed by the HTML body back to the browser
        stream.Write(headerBytes, 0, headerBytes.Length);
        stream.Write(htmlBytes, 0, htmlBytes.Length);

        // 5. Clean up
        stream.Flush();
        client.Close();
    }
}

  }


}
