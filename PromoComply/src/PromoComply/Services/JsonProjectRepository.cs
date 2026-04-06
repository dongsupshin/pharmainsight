using System.IO;
using System.Text.Json;
using PromoComply.Models;

namespace PromoComply.Services;

public class JsonProjectRepository : IProjectRepository
{
    private readonly string _dataDirectory;

    public JsonProjectRepository()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _dataDirectory = Path.Combine(appDataPath, "PromoComply", "Reviews");

        if (!Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
        }
    }

    public async Task SaveReviewSessionAsync(ReviewSession session)
    {
        var filePath = GetFilePath(session.Id);
        var json = JsonSerializer.Serialize(session, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<ReviewSession?> LoadReviewSessionAsync(Guid sessionId)
    {
        var filePath = GetFilePath(sessionId);

        if (!File.Exists(filePath))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<ReviewSession>(json);
    }

    public async Task<List<ReviewSession>> LoadAllReviewSessionsAsync()
    {
        var sessions = new List<ReviewSession>();

        if (!Directory.Exists(_dataDirectory))
        {
            return sessions;
        }

        var files = Directory.GetFiles(_dataDirectory, "*.json");

        foreach (var file in files)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var session = JsonSerializer.Deserialize<ReviewSession>(json);
                if (session != null)
                {
                    sessions.Add(session);
                }
            }
            catch (Exception)
            {
                // Skip malformed files
            }
        }

        return sessions;
    }

    public async Task DeleteReviewSessionAsync(Guid sessionId)
    {
        var filePath = GetFilePath(sessionId);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        await Task.CompletedTask;
    }

    private string GetFilePath(Guid sessionId)
    {
        return Path.Combine(_dataDirectory, $"{sessionId:N}.json");
    }
}
