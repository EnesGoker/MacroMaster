using MacroMaster.Domain.Models;

namespace MacroMaster.WinForms.Forms;

public partial class MainForm
{
    private async Task ToggleRecordingAsync()
    {
        if (_macroPlaybackService.IsPlaying || _macroPlaybackService.IsPaused)
        {
            return;
        }

        if (_macroRecorderService.IsRecording)
        {
            await _macroRecorderService.StopAsync();
            return;
        }

        await _macroRecorderService.StartAsync();
    }

    private async Task TogglePlaybackAsync()
    {
        if (_macroRecorderService.IsRecording)
        {
            return;
        }

        if (_macroPlaybackService.IsPlaying)
        {
            if (_macroPlaybackService.IsPaused)
            {
                await _macroPlaybackService.ResumeAsync();
            }
            else
            {
                await _macroPlaybackService.PauseAsync();
            }

            return;
        }

        if (_activeSession is { Events.Count: > 0 } currentSession)
        {
            await _macroPlaybackService.PlayAsync(currentSession, BuildPlaybackSettings());
        }
    }

    private async Task StopAsync()
    {
        if (_macroRecorderService.IsRecording)
        {
            await _macroRecorderService.StopAsync();
        }

        if (_macroPlaybackService.IsPlaying || _macroPlaybackService.IsPaused)
        {
            await _macroPlaybackService.StopAsync();
        }
    }

    private async Task SaveSessionAsync()
    {
        if (_activeSession is not { Events.Count: > 0 } session)
        {
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Title = "Makro Oturumu Kaydet",
            Filter = "JSON Dosyasi (*.json)|*.json|XML Dosyasi (*.xml)|*.xml",
            FileName = $"{session.Name}.json",
            AddExtension = true,
            OverwritePrompt = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            if (Path.GetExtension(dialog.FileName).Equals(".xml", StringComparison.OrdinalIgnoreCase))
            {
                await _macroStorageService.SaveAsXmlAsync(session, dialog.FileName);
            }
            else
            {
                await _macroStorageService.SaveAsJsonAsync(session, dialog.FileName);
            }
        }
        catch (Exception exception)
        {
            ShowError("Makro kaydedilirken bir hata olustu.", exception);
        }
    }

    private async Task LoadSessionAsync()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Makro Oturumu Yukle",
            Filter = "Makro Dosyalari (*.json;*.xml)|*.json;*.xml|JSON Dosyasi (*.json)|*.json|XML Dosyasi (*.xml)|*.xml",
            Multiselect = false,
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var session = Path.GetExtension(dialog.FileName).Equals(".xml", StringComparison.OrdinalIgnoreCase)
                ? await _macroStorageService.LoadFromXmlAsync(dialog.FileName)
                : await _macroStorageService.LoadFromJsonAsync(dialog.FileName);

            SetActiveSession(session);
        }
        catch (Exception exception)
        {
            ShowError("Makro yuklenirken bir hata olustu.", exception);
        }
    }

    private void ClearCurrentSession()
    {
        if (_macroRecorderService.IsRecording || _macroPlaybackService.IsPlaying || _macroPlaybackService.IsPaused)
        {
            return;
        }

        _macroRecorderService.Clear();
        ResetUiTimeline();
    }

    private void ShowError(string message, Exception exception)
    {
        MessageBox.Show(
            this,
            $"{message}{Environment.NewLine}{Environment.NewLine}{exception.Message}",
            "MacroMaster",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    private PlaybackSettings BuildPlaybackSettings()
    {
        return new PlaybackSettings
        {
            SpeedMultiplier = (double)_speedNumeric.Value,
            RepeatCount = (int)_repeatCountNumeric.Value,
            InitialDelayMs = (int)_initialDelayNumeric.Value,
            LoopIndefinitely = _loopPlaybackCheckBox.Checked,
            UseRelativeCoordinates = _relativeCoordinatesCheckBox.Checked,
            StopOnError = _stopOnErrorCheckBox.Checked,
            PreserveOriginalTiming = _preserveTimingCheckBox.Checked
        };
    }
}
