﻿using Backups.Exceptions;
using Backups.Extra.Entities;
using Newtonsoft.Json;

namespace Backups.Extra.Contexts;

public class BackupTaskContext
{
    private readonly List<BackupTaskExtra> _backupTaskExtras;
    public BackupTaskContext(string backupTasksPath)
    {
        if (string.IsNullOrEmpty(backupTasksPath))
        {
            throw new BackupException($"{nameof(backupTasksPath)} was null or empty");
        }

        _backupTaskExtras = new List<BackupTaskExtra>();
        BackupTasksPath = backupTasksPath;

        if (!Directory.Exists(backupTasksPath))
        {
            Directory.CreateDirectory(backupTasksPath);
        }

        foreach (var backupTask in Directory.EnumerateFiles(backupTasksPath))
        {
            using StreamReader file = File.OpenText(backupTask);
            var serializer = new JsonSerializer();
            serializer.TypeNameHandling = TypeNameHandling.Auto;
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;
            var task = (BackupTaskExtra)serializer.Deserialize(file, typeof(BackupTaskExtra));
            _backupTaskExtras.Add(task);
        }
    }

    public IEnumerable<BackupTaskExtra> BackupTaskExtras => _backupTaskExtras;
    public string BackupTasksPath { get; }

    public BackupTaskExtra GetTask(int taskId)
    {
        var backupTask = BackupTaskExtras.FirstOrDefault(t => t.Id == taskId);
        if (backupTask is null)
        {
            throw new BackupException($"Get failed, can't find backup task with the id {taskId}");
        }

        return backupTask;
    }

    public void AddTask(BackupTaskExtra backupTaskExtra)
    {
        _backupTaskExtras.Add(backupTaskExtra);
        using StreamWriter file = File.CreateText($"{BackupTasksPath}\\{backupTaskExtra.Id}");
        var serializer = new JsonSerializer();
        serializer.TypeNameHandling = TypeNameHandling.Auto;
        serializer.NullValueHandling = NullValueHandling.Ignore;
        serializer.Formatting = Formatting.Indented;
        serializer.Serialize(file, backupTaskExtra);
    }

    public void UpdateTask(BackupTaskExtra backupTaskExtra)
    {
        var backupTask = BackupTaskExtras.FirstOrDefault(t => t.Id == backupTaskExtra.Id);
        if (backupTask is null)
        {
            throw new BackupException($"Update failed, can't find backup task with the id {backupTaskExtra.Id}");
        }

        using StreamWriter file = File.CreateText($"{BackupTasksPath}\\{backupTask.Id}");
        var serializer = new JsonSerializer();
        serializer.TypeNameHandling = TypeNameHandling.Auto;
        serializer.NullValueHandling = NullValueHandling.Ignore;
        serializer.Formatting = Formatting.Indented;
        serializer.Serialize(file, backupTask);
    }

    public void DeleteTask(BackupTaskExtra backupTaskExtra)
    {
        var backupTask = BackupTaskExtras.FirstOrDefault(t => t.Id == backupTaskExtra.Id);
        if (backupTask is null)
        {
            throw new BackupException($"Delete failed, can't find backup task with the id {backupTaskExtra.Id}");
        }

        string filePath = $"{BackupTasksPath}\\{backupTask.Id}";
        File.Delete(filePath);
        _backupTaskExtras.Remove(backupTaskExtra);
    }

    public void DeleteAllTasks()
    {
        _backupTaskExtras.Clear();
        Directory.Delete(BackupTasksPath, true);
        Directory.CreateDirectory(BackupTasksPath);
    }
}