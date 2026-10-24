using System;
using System.Collections.Generic;
using System.Text;

using ChatClient.Models;

namespace ChatClient.Services;

public class FileService
{
    public static async Task<NetworkPacket> PreparePdfPacketAsync(string filePath)
    {
        byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
        string base64Content = Convert.ToBase64String(fileBytes); //veriyi base64(metin) formatına çeviriyoruz
        string fileName = Path.GetFileName(filePath);

        return new NetworkPacket
        {
            Type = "file",
            Mode = "unsecure",
            FileName = fileName,
            Content = base64Content
        };
    }

    public static async Task<string> SaveReceivedPdfAsync(string fileName, string base64Content)
    {
        string safeName = string.IsNullOrWhiteSpace(fileName) ? "gelen_belge.pdf" : fileName;
        string downloadDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads");

        Directory.CreateDirectory(downloadDir);
        string fullPath = Path.Combine(downloadDir, $"gelen_{safeName}");

        byte[] fileBytes = Convert.FromBase64String(base64Content);
        await File.WriteAllBytesAsync(fullPath, fileBytes);

        return fullPath;
    }
}
