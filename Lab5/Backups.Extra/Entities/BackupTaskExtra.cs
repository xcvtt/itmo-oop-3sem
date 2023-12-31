﻿using Backups.Controllers;
using Backups.Entities;
using Backups.Extra.Extensions;
using Backups.Extra.LimitAlgorithms;
using Backups.Extra.Loggers;
using Backups.Interfaces;
using Backups.Models;
using Newtonsoft.Json;

namespace Backups.Extra.Entities;

public class BackupTaskExtra : IBackupTask
{
    [JsonProperty("backupTask")]
    private BackupTask _backupTask;

    public BackupTaskExtra(BackupTask backupTask, ILogger logger, ILimitAlgorithm limitAlgorithm)
    {
        ArgumentNullException.ThrowIfNull(backupTask);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(limitAlgorithm);
        _backupTask = backupTask;
        Logger = logger;
        LimitAlgorithm = limitAlgorithm;
    }

    public ILogger Logger { get; }
    public ILimitAlgorithm LimitAlgorithm { get; }
    public int Id => _backupTask.Id;
    [JsonIgnore]
    public BackupTask BackupTask => _backupTask;

    public void CreateRestorePoint()
    {
        Logger.Log($"Creating restore point number {_backupTask.Backup.RestorePoints.Count()} " +
                   $"with storage algorithm: {_backupTask.Config.StorageAlgorithmType} and repository " +
                   $"{_backupTask.Config.RepositoryType}");
        _backupTask.CreateRestorePoint();
        ApplyLimits();
    }

    public void DeleteRestorePoint(RestorePoint restorePoint)
    {
        Logger.Log($"Deleting restore point number {restorePoint.RestorePointNumber}");
        _backupTask.DeleteRestorePoint(restorePoint);
        string restorePointPath = $"{_backupTask.Config.Repository.RootPath}/" +
                                  $"{_backupTask.TaskName}/{restorePoint.RestorePointNumber}";
        _backupTask.Config.Repository.DeleteDirectory(restorePointPath, true);
    }

    public void TrackObject(BackupObject backupObject)
    {
        Logger.Log($"Now tracking object {backupObject}");
        _backupTask.TrackObject(backupObject);
    }

    public void UntrackObject(BackupObject backupObject)
    {
        Logger.Log($"No longer tracking object {backupObject}");
        _backupTask.UntrackObject(backupObject);
    }

    public void RestoreRestorePoint(RestorePoint restorePoint, string postFix = "")
    {
        Logger.Log($"Restore restore point number {restorePoint.RestorePointNumber} to " +
                   $"original repository {_backupTask.Config.Repository.GetRepositoryType()}");

        string targetLocation = $"{_backupTask.Config.Repository.RootPath}/{postFix}";

        foreach (var zip in restorePoint.Storages)
        {
            _backupTask.Config.Repository.UnzipZipFile(zip.Path, targetLocation);
        }
    }

    public void RestoreRestorePoint(RestorePoint restorePoint, IRepository repository)
    {
        Logger.Log($"Restoring restore point number {restorePoint.RestorePointNumber} to " +
                   $"repository {repository.GetRepositoryType()} at {repository.RootPath}");

        var zipFiles = restorePoint.Storages.Select(s => s.Path);

        _backupTask.Config.Repository.UnzipZipFilesCrossRepository(zipFiles, repository);
    }

    private void ApplyLimits()
    {
        Logger.Log($"Applying limit algorithm {LimitAlgorithm.LimitAlgorithmType} for {_backupTask.Backup.RestorePoints.Count()}" +
                   $"restore points");
        var restorePointsToMerge = LimitAlgorithm.Run(_backupTask.Backup.RestorePoints);
        MergeRestorePoints(restorePointsToMerge);
    }

    private void MergeRestorePoints(IEnumerable<RestorePoint> restorePoints)
    {
        ArgumentNullException.ThrowIfNull(restorePoints);
        var listOfRestorePoints = restorePoints.ToList();
        Logger.Log($"Merging {listOfRestorePoints.Count} restore points");

        if (listOfRestorePoints.Count < 2)
        {
            return;
        }

        string postFix = "tmp_restore";
        string targetLocation = $"{_backupTask.Config.Repository.RootPath}/{postFix}";
        if (!_backupTask.Config.Repository.DirectoryExists(targetLocation))
        {
            _backupTask.Config.Repository.CreateDirectory(targetLocation);
        }

        foreach (var point in listOfRestorePoints)
        {
            RestoreRestorePoint(point, postFix);
        }

        var backupObjects = new List<BackupObject>();
        foreach (var path in _backupTask.Config.Repository.EnumeratePaths(targetLocation))
        {
            backupObjects.Add(new BackupObject(path.FullName));
        }

        var lastRestorePoint = listOfRestorePoints.Last();

        var storages = _backupTask.Config.Algorithm.RunAlgo(
            _backupTask.Config.Repository,
            backupObjects,
            lastRestorePoint.RestorePointNumber,
            _backupTask.TaskName).ToList();

        _backupTask.Config.Repository.DeleteDirectory(targetLocation, true);

        foreach (var point in listOfRestorePoints)
        {
            if (!point.Equals(lastRestorePoint))
            {
                DeleteRestorePoint(point);
            }
        }

        lastRestorePoint.BackupObjects = backupObjects;
        lastRestorePoint.Storages = storages;
    }
}