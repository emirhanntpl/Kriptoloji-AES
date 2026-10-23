using ChatClient.Models;
using ChatClient.Services;

namespace ChatClient;

public partial class MainForm : Form
{
    private readonly NetworkService _networkService;

    private TextBox txtIp = null!; // IP adresi için TextBox
    private TextBox txtPort = null!; // Port numarası için TextBox
    private Button btnConnect = null!; // Bağlan butonu
    private RichTextBox rtbChat = null!; // Sohbet mesajlarını göstermek için RichTextBox
    private TextBox txtMessage = null!; // Mesaj girişi için TextBox
    private Button btnSendText = null!; // Metin gönderme butonu
    private Button btnSendPdf = null!; // PDF dosyası gönderme butonu

    public MainForm()
    {
        _networkService = new NetworkService();
        _networkService.OnLog += Log;
        _networkService.OnPacketReceived += HandlePacketReceived;
        _networkService.OnDisconnected += HandleDisconnected;

        InitializeCustomUI();
    }

    private void InitializeCustomUI()
    {
        Text = "Kriptoloji Güvensiz İstemci (Modüler Mimari)";
        Size = new Size(540, 580);
        StartPosition = FormStartPosition.CenterScreen;

        
        Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 45 };
        topPanel.Controls.Add(new Label { Text = "IP:", Location = new Point(10, 12), AutoSize = true });
        txtIp = new TextBox { Text = "127.0.0.1", Location = new Point(35, 10), Width = 110 };
        topPanel.Controls.Add(txtIp);

        topPanel.Controls.Add(new Label { Text = "Port:", Location = new Point(155, 12), AutoSize = true });
        txtPort = new TextBox { Text = "6161", Location = new Point(190, 10), Width = 55 };
        topPanel.Controls.Add(txtPort);

        btnConnect = new Button { Text = "Bağlan", Location = new Point(260, 8), Width = 85, BackColor = Color.LightGreen };
        btnConnect.Click += async (s, e) => await ConnectAsync();
        topPanel.Controls.Add(btnConnect);

        
        Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 90 };
        txtMessage = new TextBox { Location = new Point(10, 12), Width = 370 };
        txtMessage.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) await SendTextAsync(); };
        bottomPanel.Controls.Add(txtMessage);

        btnSendText = new Button { Text = "Gönder", Location = new Point(390, 10), Width = 120 };
        btnSendText.Click += async (s, e) => await SendTextAsync();
        bottomPanel.Controls.Add(btnSendText);

        btnSendPdf = new Button { Text = " PDF Dosyası Seç ve Gönder", Dock = DockStyle.Bottom, Height = 35, BackColor = Color.LightSkyBlue };
        btnSendPdf.Click += async (s, e) => await SendPdfAsync();
        bottomPanel.Controls.Add(btnSendPdf);

        
        rtbChat = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.White };

        Controls.Add(rtbChat);
        Controls.Add(bottomPanel);
        Controls.Add(topPanel);
    }

    private async Task ConnectAsync()
    {
        try
        {
            btnConnect.Enabled = false;
            await _networkService.ConnectAsync(txtIp.Text.Trim(), int.Parse(txtPort.Text.Trim()));
            btnConnect.Text = "Bağlandı";
            btnConnect.BackColor = Color.LightGray;
        }
        catch (Exception ex)
        {
            btnConnect.Enabled = true;
            MessageBox.Show($"Bağlantı hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task SendTextAsync()
    {
        string text = txtMessage.Text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        var packet = new NetworkPacket { Type = "text", Mode = "unsecure", Content = text };
        await _networkService.SendPacketAsync(packet);
        Log($"Ben (Metin): {text}");
        txtMessage.Clear();
    }

    private async Task SendPdfAsync()
    {
        using OpenFileDialog ofd = new OpenFileDialog { Filter = "PDF Dosyaları (*.pdf)|*.pdf" };
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            var packet = await FileService.PreparePdfPacketAsync(ofd.FileName);
            await _networkService.SendPacketAsync(packet);
            Log($"Ben (Dosya): {packet.FileName} gönderildi.");
        }
    }

    private async void HandlePacketReceived(NetworkPacket packet)
    {
        if (packet.Type == "text")
        {
            Log($"Karşı Taraf (Metin): {packet.Content}");
        }
        else if (packet.Type == "file")
        {
            string savedPath = await FileService.SaveReceivedPdfAsync(packet.FileName ?? "dosya.pdf", packet.Content ?? "");
            Log($"Karşı Taraf (Dosya): {packet.FileName} alındı -> {savedPath}");
        }
    }

    private void HandleDisconnected()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(HandleDisconnected));
            return;
        }
        btnConnect.Enabled = true;
        btnConnect.Text = "Bağlan";
        btnConnect.BackColor = Color.LightGreen;
    }

    private void Log(string message)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => Log(message)));
            return;
        }
        rtbChat.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
        rtbChat.ScrollToCaret();
    }
}