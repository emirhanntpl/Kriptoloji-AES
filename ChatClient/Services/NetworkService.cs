using System;
using System.Collections.Generic;
using System.Text;

using System.Net.Sockets;
using System.Text.Json;
using ChatClient.Models;

namespace ChatClient.Services;

public class NetworkService
{
    private TcpClient? _client;
    private NetworkStream? _stream; //veriyi okuyup yazmak için
    private bool _isConnected;

    public bool IsConnected => _isConnected;

    
    public event Action<NetworkPacket>? OnPacketReceived; // Paket alındığında tetiklenecek olay
    public event Action<string>? OnLog; // Log mesajlarını iletmek için tetiklenecek olay
    public event Action? OnDisconnected; // Bağlantı kesildiğinde tetiklenecek olay

    public async Task ConnectAsync(string ip, int port)
    {
        _client = new TcpClient();
        await _client.ConnectAsync(ip, port); // Sunucuya bağlan
        _stream = _client.GetStream(); //veri akışı için stream oluştur
        _isConnected = true; // Bağlantı durumu güncelle

        OnLog?.Invoke($"Sunucuya bağlanıldı: {ip}:{port}"); 

        _ = Task.Run(ReceiveLoopAsync); // Arka planda veri alma döngusunu başlat
    }

    public async Task SendPacketAsync(NetworkPacket packet)
    {
        if (!_isConnected || _stream == null)
            throw new InvalidOperationException("Sunucu bağlantısı aktif değil.");

        string jsonString = JsonSerializer.Serialize(packet);
        byte[] dataBytes = Encoding.UTF8.GetBytes(jsonString); // Veriyi byte dizisine çevir
        byte[] lengthPrefix = BitConverter.GetBytes(dataBytes.Length); 

        
        await _stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);// Önce uzunluğu gönder
        await _stream.WriteAsync(dataBytes, 0, dataBytes.Length);// Sonra veriyi gönder
    }

    private async Task ReceiveLoopAsync()
    {
        byte[] lengthBuffer = new byte[4];

        try
        {
            while (_isConnected && _stream != null)
            {
                // Önce 4 byte uzunluğu oku
                int bytesRead = await _stream.ReadAsync(lengthBuffer, 0, 4);
                if (bytesRead == 0) break;

                //  Uzunluğu integer olarak al
                int dataLength = BitConverter.ToInt32(lengthBuffer, 0);
                byte[] dataBuffer = new byte[dataLength];

                //  Belirtilen uzunluk kadar veriyi eksiksiz oku
                int totalRead = 0;
                while (totalRead < dataLength)
                {
                    int read = await _stream.ReadAsync(dataBuffer, totalRead, dataLength - totalRead);
                    if (read == 0) break;
                    totalRead += read;
                }

                // baytları JSON A çevir 
                string jsonString = Encoding.UTF8.GetString(dataBuffer);
                var packet = JsonSerializer.Deserialize<NetworkPacket>(jsonString); // JSON verisini NetworkPacket nesnesine dönüştür

                if (packet != null)
                {
                    OnPacketReceived?.Invoke(packet); //paketi alındığında tetiklenen olayı çağır
                }
            }
        }
        catch (Exception ex)
        {
            OnLog?.Invoke($"[HATA] Ağ bağlantısı koptu: {ex.Message}");
        }
        finally
        {
            Disconnect();
        }
    }

    public void Disconnect()
    {
        if (!_isConnected) return;
        _isConnected = false;
        _stream?.Close();
        _client?.Close();
        OnDisconnected?.Invoke();
        OnLog?.Invoke("Sunucu bağlantısı sonlandırıldı.");
    }
}
