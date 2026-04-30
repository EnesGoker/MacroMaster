using MacroMaster.Application.Abstractions;

namespace MacroMaster.WinForms.Forms;

public partial class MainForm
{
    private async Task ShowHotkeySettingsAsync()
    {
        if (_macroRecorderService.IsRecording || _macroPlaybackService.IsPlaying || _macroPlaybackService.IsPaused)
        {
            MessageBox.Show(
                this,
                "Kisayollar yalnizca uygulama bostayken degistirilebilir.",
                "MacroMaster",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        using var dialog = new HotkeySettingsDialog(
            _hotkeyConfiguration.RecordToggleVirtualKey,
            _hotkeyConfiguration.PlaybackToggleVirtualKey,
            _hotkeyConfiguration.StopVirtualKey);

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        await ApplyHotkeySettingsAsync(
            dialog.RecordToggleVirtualKey,
            dialog.PlaybackToggleVirtualKey,
            dialog.StopVirtualKey);
    }

    private async Task ApplyHotkeySettingsAsync(int recordVirtualKey, int playbackVirtualKey, int stopVirtualKey)
    {
        var currentRecordVirtualKey = _hotkeyConfiguration.RecordToggleVirtualKey;
        var currentPlaybackVirtualKey = _hotkeyConfiguration.PlaybackToggleVirtualKey;
        var currentStopVirtualKey = _hotkeyConfiguration.StopVirtualKey;

        if (currentRecordVirtualKey == recordVirtualKey
            && currentPlaybackVirtualKey == playbackVirtualKey
            && currentStopVirtualKey == stopVirtualKey)
        {
            return;
        }

        var wasRegistered = _hotkeyService.IsRegistered;

        try
        {
            if (wasRegistered)
            {
                await _hotkeyService.UnregisterAsync();
            }

            _hotkeyConfiguration.RecordToggleVirtualKey = recordVirtualKey;
            _hotkeyConfiguration.PlaybackToggleVirtualKey = playbackVirtualKey;
            _hotkeyConfiguration.StopVirtualKey = stopVirtualKey;

            if (wasRegistered)
            {
                await _hotkeyService.RegisterAsync();
            }

            RefreshUiState();
        }
        catch (Exception exception)
        {
            _hotkeyConfiguration.RecordToggleVirtualKey = currentRecordVirtualKey;
            _hotkeyConfiguration.PlaybackToggleVirtualKey = currentPlaybackVirtualKey;
            _hotkeyConfiguration.StopVirtualKey = currentStopVirtualKey;

            try
            {
                if (wasRegistered && !_hotkeyService.IsRegistered)
                {
                    await _hotkeyService.RegisterAsync();
                }
            }
            catch
            {
                // The original error is the one we want to surface to the user.
            }

            ShowError("Kisayollar guncellenirken bir hata olustu.", exception);
        }
    }

    private string GetHotkeySummaryText()
    {
        return string.Join(
            " / ",
            [
                GetHotkeyDisplayText(_hotkeyConfiguration.RecordToggleVirtualKey),
                GetHotkeyDisplayText(_hotkeyConfiguration.PlaybackToggleVirtualKey),
                GetHotkeyDisplayText(_hotkeyConfiguration.StopVirtualKey)
            ]);
    }

    private string GetHotkeyDetailsText()
    {
        return
            $"{GetHotkeyDisplayText(_hotkeyConfiguration.RecordToggleVirtualKey)} kayit  •  " +
            $"{GetHotkeyDisplayText(_hotkeyConfiguration.PlaybackToggleVirtualKey)} oynat  •  " +
            $"{GetHotkeyDisplayText(_hotkeyConfiguration.StopVirtualKey)} durdur";
    }

    internal static string GetHotkeyDisplayText(int virtualKey)
    {
        return virtualKey switch
        {
            >= 0x70 and <= 0x7B => $"F{virtualKey - 0x6F}",
            0x1B => "Esc",
            _ => $"0x{virtualKey:X2}"
        };
    }
}
