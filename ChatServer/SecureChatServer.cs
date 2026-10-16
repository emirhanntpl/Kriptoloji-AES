using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace DefaultNamespace;

public class SecureChatServer
{
    private static ConcurrentDictionary<string,TcpClient> connectedClients = new ConcurrentDictionary<string,TcpClient>();

    static async Task Main(string[] args)
    {
        int port = 6161;
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        
        Console.WriteLine($"[SUNUCU] Başlatıldı. {port} portu dinleniyor...");
        Console.WriteLine("[SUNUCU] İstemcilerin bağlanması bekleniyor...\n");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();

            _ = HandleClientAsync(client);
        }
    }

    private  static async Task HandleClientAsync(TcpClient client)
    {
        string ClientId = Guid.NewGuid().ToString().Substring(0, 8);
        NetworkStream stream = client.GetStream();
        
        connectedClients.TryAdd(ClientId, client);
        Console.WriteLine($"[BAĞLANTI] Yeni istemci bağlandı. ID: {ClientId} (Toplam İstemci: {connectedClients.Count})");

        try
        {
            byte[] lengthBuffer = new byte[4];

            while (true)
            {
                int bytesRead = await stream.ReadAsync(lengthBuffer, 0, 4);
                if (bytesRead == 4) break;

                int dataLength = BitConverter.ToInt32(lengthBuffer, 0);
                byte[] dataBuffer = new byte[dataLength];

                int totalBytesRead = 0;
                while (totalBytesRead < dataLength)
                {
                    int read = await stream.ReadAsync(dataBuffer, totalBytesRead, dataLength - totalBytesRead);
                    if (read == 0) break;
                    totalBytesRead += read;

                }

                string receivedText = Encoding.UTF8.GetString(dataBuffer);
                Console.WriteLine($"[{ClientId}]'den veri alındı: {receivedText}");

                await BroadcastToOtherClientsAsync(ClientId, dataBuffer);

            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"[HATA] İstemci {ClientId} düştu: {e.Message}");
        }
        finally
        {
            connectedClients.TryRemove(ClientId, out _);
            client.Close();
            Console.WriteLine($"[AYRILMA] İstemci bağlantısı kapatıldı. ID: {ClientId}");
        }
    }

    private static async Task BroadcastToOtherClientsAsync(string senderId, byte[] data)
    {
       
        byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

        foreach (KeyValuePair<string, TcpClient> kvp in connectedClients)
        {
            if (kvp.Key != senderId)
            {
                try
                {
                    NetworkStream otherStream = kvp.Value.GetStream();
                    await otherStream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
                    await otherStream.WriteAsync(data, 0, data.Length);
                    Console.WriteLine($"[YÖNLENDİRME] Veri {senderId} adresinden {kvp.Key} adresine iletildi.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[YÖNLENDİRME HATASI] {kvp.Key} adresine gönderilemedi: {ex.Message}");
                  
                }
            }
            
        }
        
    }
}