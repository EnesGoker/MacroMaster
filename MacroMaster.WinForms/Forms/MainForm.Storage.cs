using MacroMaster.Domain.Models;
using MacroMaster.WinForms.Reporting;
using MacroMaster.Application.Abstractions;
using System.Text;

namespace MacroMaster.WinForms.Forms;

public partial class MainForm
{
    private async Task SaveJsonAsync()
    {
        MacroSession session = GetRequiredSession();
        EnsureSessionMutationAllowed();

        using var dialog = new SaveFileDialog
        {
            Filter = "JSON makro (*.json)|*.json|Tum dosyalar (*.*)|*.*",
            FileName = BuildDefaultFileName(session.Name, ".json"),
            AddExtension = true,
            DefaultExt = "json",
            RestoreDirectory = true
        };

        if (ModalDialogOverlay.ShowDialog(this, dialog) != DialogResult.OK)
        {
            return;
        }

        await _macroStorageService.SaveAsJsonAsync(session, dialog.FileName);
        _lastSessionPath = dialog.FileName;
        await RefreshMacroLibraryAsync();
        RefreshUiState();
    }

    private async Task SaveXmlAsync()
    {
        MacroSession session = GetRequiredSession();
        EnsureSessionMutationAllowed();

        using var dialog = new SaveFileDialog
        {
            Filter = "XML makro (*.xml)|*.xml|Tum dosyalar (*.*)|*.*",
            FileName = BuildDefaultFileName(session.Name, ".xml"),
            AddExtension = true,
            DefaultExt = "xml",
            RestoreDirectory = true
        };

        if (ModalDialogOverlay.ShowDialog(this, dialog) != DialogResult.OK)
        {
            return;
        }

        await _macroStorageService.SaveAsXmlAsync(session, dialog.FileName);
        _lastSessionPath = dialog.FileName;
        await RefreshMacroLibraryAsync();
        RefreshUiState();
    }

    private async Task LoadJsonAsync()
    {
        EnsureSessionMutationAllowed();

        using var dialog = new OpenFileDialog
        {
            Filter = "JSON makro (*.json)|*.json|Tum dosyalar (*.*)|*.*",
            RestoreDirectory = true
        };

        if (ModalDialogOverlay.ShowDialog(this, dialog) != DialogResult.OK)
        {
            return;
        }

        MacroSession session = await _macroStorageService.LoadFromJsonAsync(dialog.FileName);
        AdoptLoadedSession(session, dialog.FileName);
        await RefreshMacroLibraryAsync();
    }

    private async Task LoadXmlAsync()
    {
        EnsureSessionMutationAllowed();

        using var dialog = new OpenFileDialog
        {
            Filter = "XML makro (*.xml)|*.xml|Tum dosyalar (*.*)|*.*",
            RestoreDirectory = true
        };

        if (ModalDialogOverlay.ShowDialog(this, dialog) != DialogResult.OK)
        {
            return;
        }

        MacroSession session = await _macroStorageService.LoadFromXmlAsync(dialog.FileName);
        AdoptLoadedSession(session, dialog.FileName);
        await RefreshMacroLibraryAsync();
    }

    private async Task SaveHtmlReportAsync()
    {
        await SaveReportAsync(
            "HTML rapor (*.html)|*.html|Tum dosyalar (*.*)|*.*",
            ".html",
            "html",
            MacroReportGenerator.GenerateHtml);
    }

    private async Task SaveTextReportAsync()
    {
        await SaveReportAsync(
            "TXT rapor (*.txt)|*.txt|Tum dosyalar (*.*)|*.*",
            ".txt",
            "txt",
            MacroReportGenerator.GenerateText);
    }

    private async Task SaveReportAsync(
        string filter,
        string extension,
        string defaultExtension,
        Func<MacroSession, string?, string> buildReport)
    {
        MacroSession session = GetRequiredSession();
        EnsureSessionMutationAllowed();

        using var dialog = new SaveFileDialog
        {
            Filter = filter,
            FileName = BuildDefaultFileName(session.Name + "_Rapor", extension),
            AddExtension = true,
            DefaultExt = defaultExtension,
            RestoreDirectory = true
        };

        if (ModalDialogOverlay.ShowDialog(this, dialog) != DialogResult.OK)
        {
            return;
        }

        string reportContent = buildReport(session, _lastSessionPath);
        await File.WriteAllTextAsync(dialog.FileName, reportContent, Encoding.UTF8);
    }

    private static string BuildDefaultFileName(string sessionName, string extension)
    {
        char[] invalidCharacters = Path.GetInvalidFileNameChars();
        string sanitizedName = new string(sessionName
            .Where(character => !invalidCharacters.Contains(character))
            .ToArray());

        if (string.IsNullOrWhiteSpace(sanitizedName))
        {
            sanitizedName = "MakroOturumu";
        }

        return sanitizedName + extension;
    }

    private MacroLibraryFileFormat ResolveCurrentLibrarySaveFormat()
    {
        string extension = string.IsNullOrWhiteSpace(_lastSessionPath)
            ? string.Empty
            : Path.GetExtension(_lastSessionPath);

        return extension.Equals(".xml", StringComparison.OrdinalIgnoreCase)
            ? MacroLibraryFileFormat.Xml
            : MacroLibraryFileFormat.Json;
    }

    private static bool IsSamePath(string? left, string right)
    {
        return !string.IsNullOrWhiteSpace(left)
            && string.Equals(
                Path.GetFullPath(left),
                Path.GetFullPath(right),
                StringComparison.OrdinalIgnoreCase);
    }
}

