﻿using Backups.Exceptions;

namespace Backups.Models;

public class BackupObject : IEquatable<BackupObject>
{
    public BackupObject(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new BackupException($"{path} was null or empty");
        }

        Path = path;
    }

    public string Path { get; }

    public override string ToString()
    {
        return $"{nameof(Path)}: {Path}";
    }

    public override int GetHashCode() => Path.GetHashCode();

    public override bool Equals(object obj) => this.Equals(obj as BackupObject);
    public bool Equals(BackupObject other) => other?.Path.Equals(Path) ?? false;
}