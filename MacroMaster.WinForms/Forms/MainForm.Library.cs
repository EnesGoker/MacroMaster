using MacroMaster.Application.Abstractions;
using MacroMaster.Domain.Models;
using MacroMaster.WinForms.Controls;

namespace MacroMaster.WinForms.Forms;

public partial class MainForm
{
    private async Task SaveToLibraryAsync()
    {
        MacroSession session = GetRequiredSession();
        EnsureSessionMutationAllowed();

        MacroLibraryFileFormat format = ResolveCurrentLibrarySaveFormat();
        string filePath = await _macroLibraryService.SaveAsync(session, format);
        _lastSessionPath = filePath;
        MarkLibraryFileUsed(filePath);
        await TrySaveMacroLibraryUserStateAsync("Makro kutuphanesi son kullanilan kaydetme");
        await RefreshMacroLibraryAsync();
        RefreshUiState();
    }

    private async Task ImportLibraryMacroAsync()
    {
        EnsureSessionMutationAllowed();

        using var dialog = new OpenFileDialog
        {
            Filter = "Makro dosyalari (*.json;*.xml)|*.json;*.xml|JSON makro (*.json)|*.json|XML makro (*.xml)|*.xml|Tum dosyalar (*.*)|*.*",
            RestoreDirectory = true
        };

        if (ModalDialogOverlay.ShowDialog(this, dialog) != DialogResult.OK)
        {
            return;
        }

        string importedFilePath = await _macroLibraryService.ImportAsync(dialog.FileName);
        MacroSession session = await _macroLibraryService.LoadAsync(importedFilePath);

        AdoptLoadedSession(session, importedFilePath);
        MarkLibraryFileUsed(importedFilePath);
        await TrySaveMacroLibraryUserStateAsync("Makro kutuphanesi son kullanilan kaydetme");
        await RefreshMacroLibraryAsync();
    }

    private async Task LoadLibraryMacroAsync(MacroLibraryEntry item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureSessionMutationAllowed();

        MacroSession session = await _macroLibraryService.LoadAsync(item.FilePath);
        AdoptLoadedSession(session, item.FilePath);
        MarkLibraryFileUsed(item.FilePath);
        await TrySaveMacroLibraryUserStateAsync("Makro kutuphanesi son kullanilan kaydetme");
        await RefreshMacroLibraryAsync();
    }

    private async Task RenameLibraryMacroAsync(MacroLibraryEntry item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureSessionMutationAllowed();

        using var dialog = new MacroNameEditDialog(item.Name);

        if (ModalDialogOverlay.ShowDialog(this, dialog) != DialogResult.OK)
        {
            return;
        }

        string previousFilePath = item.FilePath;
        string renamedFilePath = await _macroLibraryService.RenameAsync(
            previousFilePath,
            dialog.MacroName);

        if (IsSamePath(_lastSessionPath, previousFilePath))
        {
            _lastSessionPath = renamedFilePath;

            if (_activeSession is not null)
            {
                _activeSession.Name = Path.GetFileNameWithoutExtension(renamedFilePath);
            }
        }

        MoveLibraryStatePath(previousFilePath, renamedFilePath);
        await TrySaveMacroLibraryUserStateAsync("Makro kutuphanesi kullanici tercihi tasima");
        await RefreshMacroLibraryAsync();
        RefreshUiState();
    }

    private async Task DeleteLibraryMacroAsync(MacroLibraryEntry item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureSessionMutationAllowed();

        if (!ThemedConfirmationDialog.ConfirmMacroDelete(this, item.Name))
        {
            return;
        }

        await _macroLibraryService.DeleteAsync(item.FilePath);

        if (IsSamePath(_lastSessionPath, item.FilePath))
        {
            _lastSessionPath = null;
        }

        RemoveLibraryStatePath(item.FilePath);
        await TrySaveMacroLibraryUserStateAsync("Makro kutuphanesi kullanici tercihi silme");
        await RefreshMacroLibraryAsync();
        RefreshUiState();
    }

    private async Task ToggleLibraryFavoriteAsync(MacroLibraryEntry item)
    {
        ArgumentNullException.ThrowIfNull(item);

        string normalizedFilePath = NormalizeLibraryStatePath(item.FilePath);

        if (!RemoveFavoritePath(normalizedFilePath))
        {
            _macroLibraryUserState.FavoriteFilePaths.Add(normalizedFilePath);
        }

        await SaveMacroLibraryUserStateAsync();
        await RefreshMacroLibraryAsync();
    }

    private async Task OptimizeLibraryMacroAsync(MacroLibraryEntry item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureSessionMutationAllowed();

        MacroSession session = await _macroLibraryService.LoadAsync(item.FilePath);
        MacroOptimizationPreview preview = _macroOptimizationService.Preview(session);

        if (!preview.HasChanges)
        {
            MacroOptimizationDialog.ShowNoChanges(this);
            return;
        }

        using var dialog = new MacroOptimizationDialog(preview);

        if (ModalDialogOverlay.ShowDialog(this, dialog) != DialogResult.OK)
        {
            return;
        }

        session.ReplaceEvents(preview.OptimizedEvents);
        await SaveOptimizedLibraryMacroAsync(session, item);

        bool optimizedActiveSession = IsSamePath(_lastSessionPath, item.FilePath);

        if (optimizedActiveSession)
        {
            _activeSession = session;
            _playedEventCount = 0;
            _playedDurationMs = 0;
            _activePlaybackSourceIndex = null;
        }

        MarkLibraryFileUsed(item.FilePath);
        await TrySaveMacroLibraryUserStateAsync("Makro kutuphanesi optimizasyon son kullanilan kaydetme");
        await RefreshMacroLibraryAsync();
        RefreshUiState(forceEventListReload: optimizedActiveSession);
    }

    private Task SaveOptimizedLibraryMacroAsync(MacroSession session, MacroLibraryEntry item)
    {
        return item.Format switch
        {
            MacroLibraryFileFormat.Json => _macroStorageService.SaveAsJsonAsync(session, item.FilePath),
            MacroLibraryFileFormat.Xml => _macroStorageService.SaveAsXmlAsync(session, item.FilePath),
            _ => throw new NotSupportedException($"Desteklenmeyen makro dosya formati: {item.FilePath}")
        };
    }

    private async Task RefreshMacroLibraryAsync()
    {
        IReadOnlyList<MacroLibraryEntry> entries = await _macroLibraryService.ListAsync();

        if (PruneMacroLibraryUserState(entries))
        {
            await TrySaveMacroLibraryUserStateAsync(
                "Makro kutuphanesi kullanici tercihleri temizleme");
        }

        _macroLibraryControl.SetItems(BuildMacroLibraryViewItems(entries), _lastSessionPath);
    }

    private async Task LoadMacroLibraryUserStateAsync()
    {
        _macroLibraryUserState = await _macroLibraryUserStateStore.LoadAsync();
    }

    private async Task SaveMacroLibraryUserStateAsync()
    {
        await _macroLibraryUserStateStore.SaveAsync(_macroLibraryUserState);
    }

    private async Task TrySaveMacroLibraryUserStateAsync(string operationName)
    {
        try
        {
            await SaveMacroLibraryUserStateAsync();
        }
        catch (Exception ex)
        {
            _logger.Log(
                AppLogLevel.Warning,
                nameof(MainForm),
                $"{operationName} islemi tamamlanamadi.",
                ex);
        }
    }

    private MacroLibraryViewItem[] BuildMacroLibraryViewItems(
        IReadOnlyList<MacroLibraryEntry> entries)
    {
        return entries
            .Select(entry => new MacroLibraryViewItem(
                entry,
                IsFavoritePath(entry.FilePath),
                GetLastUsedUtc(entry.FilePath)))
            .ToArray();
    }

    private bool PruneMacroLibraryUserState(IReadOnlyList<MacroLibraryEntry> entries)
    {
        var activeFilePaths = entries
            .Select(entry => NormalizeLibraryStatePath(entry.FilePath))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        int originalFavoriteCount = _macroLibraryUserState.FavoriteFilePaths.Count;
        int originalRecentCount = _macroLibraryUserState.LastUsedUtcByFilePath.Count;
        bool changed = false;

        List<string> normalizedFavoriteFilePaths = _macroLibraryUserState.FavoriteFilePaths
            .Select(NormalizeLibraryStatePath)
            .Where(activeFilePaths.Contains)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        changed = changed
            || originalFavoriteCount != normalizedFavoriteFilePaths.Count
            || !_macroLibraryUserState.FavoriteFilePaths.SequenceEqual(
                normalizedFavoriteFilePaths,
                StringComparer.OrdinalIgnoreCase);
        _macroLibraryUserState.FavoriteFilePaths = normalizedFavoriteFilePaths;

        foreach (string filePath in _macroLibraryUserState.LastUsedUtcByFilePath.Keys.ToArray())
        {
            string normalizedFilePath = NormalizeLibraryStatePath(filePath);

            if (!activeFilePaths.Contains(normalizedFilePath))
            {
                _macroLibraryUserState.LastUsedUtcByFilePath.Remove(filePath);
                changed = true;
            }
            else if (!string.Equals(filePath, normalizedFilePath, StringComparison.Ordinal))
            {
                DateTime lastUsedUtc = _macroLibraryUserState.LastUsedUtcByFilePath[filePath];
                _macroLibraryUserState.LastUsedUtcByFilePath.Remove(filePath);
                _macroLibraryUserState.LastUsedUtcByFilePath[normalizedFilePath] = NormalizeUtc(lastUsedUtc);
                changed = true;
            }
        }

        return changed
            || originalRecentCount != _macroLibraryUserState.LastUsedUtcByFilePath.Count;
    }

    private void MarkLibraryFileUsed(string filePath)
    {
        string normalizedFilePath = NormalizeLibraryStatePath(filePath);
        _macroLibraryUserState.LastUsedUtcByFilePath[normalizedFilePath] = DateTime.UtcNow;
    }

    private void MoveLibraryStatePath(
        string previousFilePath,
        string nextFilePath)
    {
        string previousNormalizedFilePath = NormalizeLibraryStatePath(previousFilePath);
        string nextNormalizedFilePath = NormalizeLibraryStatePath(nextFilePath);

        if (string.Equals(
                previousNormalizedFilePath,
                nextNormalizedFilePath,
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        bool wasFavorite = RemoveFavoritePath(previousNormalizedFilePath);
        if (wasFavorite)
        {
            _macroLibraryUserState.FavoriteFilePaths.Add(nextNormalizedFilePath);
        }

        if (_macroLibraryUserState.LastUsedUtcByFilePath.TryGetValue(
                previousNormalizedFilePath,
                out DateTime lastUsedUtc))
        {
            _macroLibraryUserState.LastUsedUtcByFilePath.Remove(previousNormalizedFilePath);
            _macroLibraryUserState.LastUsedUtcByFilePath[nextNormalizedFilePath] = NormalizeUtc(lastUsedUtc);
        }
    }

    private void RemoveLibraryStatePath(string filePath)
    {
        string normalizedFilePath = NormalizeLibraryStatePath(filePath);
        RemoveFavoritePath(normalizedFilePath);
        _macroLibraryUserState.LastUsedUtcByFilePath.Remove(normalizedFilePath);
    }

    private bool IsFavoritePath(string filePath)
    {
        string normalizedFilePath = NormalizeLibraryStatePath(filePath);
        return _macroLibraryUserState.FavoriteFilePaths
            .Any(path => string.Equals(
                NormalizeLibraryStatePath(path),
                normalizedFilePath,
                StringComparison.OrdinalIgnoreCase));
    }

    private DateTime? GetLastUsedUtc(string filePath)
    {
        string normalizedFilePath = NormalizeLibraryStatePath(filePath);
        return _macroLibraryUserState.LastUsedUtcByFilePath.TryGetValue(normalizedFilePath, out DateTime lastUsedUtc)
            ? NormalizeUtc(lastUsedUtc)
            : null;
    }

    private bool RemoveFavoritePath(string normalizedFilePath)
    {
        bool removed = false;

        for (int index = _macroLibraryUserState.FavoriteFilePaths.Count - 1; index >= 0; index--)
        {
            if (string.Equals(
                    NormalizeLibraryStatePath(_macroLibraryUserState.FavoriteFilePaths[index]),
                    normalizedFilePath,
                    StringComparison.OrdinalIgnoreCase))
            {
                _macroLibraryUserState.FavoriteFilePaths.RemoveAt(index);
                removed = true;
            }
        }

        return removed;
    }

    private static string NormalizeLibraryStatePath(string filePath)
    {
        return Path.GetFullPath(filePath);
    }

    private static DateTime NormalizeUtc(DateTime timestamp)
    {
        return timestamp.Kind switch
        {
            DateTimeKind.Local => timestamp.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(timestamp, DateTimeKind.Utc),
            _ => timestamp
        };
    }
}

