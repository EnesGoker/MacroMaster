using MacroMaster.Domain.Enums;
using MacroMaster.Application.Abstractions;
using MacroMaster.WinForms.Platform;

namespace MacroMaster.WinForms.Forms;

public partial class MainForm
{
    private async Task InitializeHotkeysAsync()
    {
        _hotkeyService.RecordToggleRequested += OnRecordToggleRequested;
        _hotkeyService.PlaybackToggleRequested += OnPlaybackToggleRequested;
        _hotkeyService.StopRequested += OnStopRequested;
        _hotkeyService.HotkeySettingsRequested += OnHotkeySettingsRequested;

        try
        {
            await _hotkeyService.RegisterAsync();
        }
        catch (Exception ex)
        {
            _logger.Log(
                AppLogLevel.Warning,
                nameof(MainForm),
                "Global kisayollar kaydedilemedi. Uygulama odaktayken yerel kisayol yedegi kullanilacak.",
                ex);
        }
    }

    private void OnRecordToggleRequested()
    {
        _ = ExecuteUiActionAsync(HandleRecordToggleAsync, "Kaydi baslat/durdur");
    }

    private void OnPlaybackToggleRequested()
    {
        _ = ExecuteUiActionAsync(HandlePlaybackToggleAsync, "Oynat/duraklat");
    }

    private void OnStopRequested()
    {
        _ = ExecuteUiActionAsync(HandleStopAsync, "Durdurma istegi");
    }

    private void OnHotkeySettingsRequested()
    {
        _ = ExecuteUiActionAsync(EditHotkeysAsync, "Kisayollari ac");
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (TryHandleLocalHotkeyFallback(keyData))
        {
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private bool TryHandleLocalHotkeyFallback(Keys keyData)
    {
        if (ShouldUseLocalHotkeyFallback(_hotkeyConfiguration.RecordToggleHotkey)
            && MatchesHotkey(keyData, _hotkeyConfiguration.RecordToggleHotkey))
        {
            OnRecordToggleRequested();
            return true;
        }

        if (ShouldUseLocalHotkeyFallback(_hotkeyConfiguration.PlaybackToggleHotkey)
            && MatchesHotkey(keyData, _hotkeyConfiguration.PlaybackToggleHotkey))
        {
            OnPlaybackToggleRequested();
            return true;
        }

        if (ShouldUseLocalHotkeyFallback(_hotkeyConfiguration.StopHotkey)
            && MatchesHotkey(keyData, _hotkeyConfiguration.StopHotkey))
        {
            OnStopRequested();
            return true;
        }

        if (ShouldUseLocalHotkeyFallback(_hotkeyConfiguration.HotkeySettingsHotkey)
            && MatchesHotkey(keyData, _hotkeyConfiguration.HotkeySettingsHotkey))
        {
            OnHotkeySettingsRequested();
            return true;
        }

        return false;
    }

    private bool ShouldUseLocalHotkeyFallback(HotkeyBinding hotkeyBinding)
    {
        return !_hotkeyService.IsHotkeyRegistered(hotkeyBinding);
    }

    private static bool MatchesHotkey(Keys keyData, HotkeyBinding hotkeyBinding)
    {
        return (int)(keyData & Keys.KeyCode) == hotkeyBinding.VirtualKeyCode
            && ResolveKeyDataModifiers(keyData) == hotkeyBinding.Modifiers;
    }

    private static HotkeyModifiers ResolveKeyDataModifiers(Keys keyData)
    {
        HotkeyModifiers modifiers = HotkeyModifiers.None;

        if ((keyData & Keys.Control) == Keys.Control)
        {
            modifiers |= HotkeyModifiers.Control;
        }

        if ((keyData & Keys.Shift) == Keys.Shift)
        {
            modifiers |= HotkeyModifiers.Shift;
        }

        if ((keyData & Keys.Alt) == Keys.Alt)
        {
            modifiers |= HotkeyModifiers.Alt;
        }

        return modifiers;
    }

    private async Task EditHotkeysAsync()
    {
        EnsureHotkeyMutationAllowed();

        bool restorePreviousGlobalHotkeys = await SuspendHotkeysForModalDialogAsync();

        try
        {
            using var dialog = new HotkeySettingsDialog(_hotkeyConfiguration.Snapshot());

            if (ModalDialogOverlay.ShowDialog(this, dialog) != DialogResult.OK)
            {
                return;
            }

            await ApplyHotkeySettingsAsync(
                dialog.SelectedHotkeySettings,
                restorePreviousGlobalHotkeys);
            restorePreviousGlobalHotkeys = false;
        }
        finally
        {
            if (restorePreviousGlobalHotkeys)
            {
                await RestoreHotkeysAfterModalDialogAsync();
            }
        }
    }

    private async Task ApplyHotkeySettingsAsync(
        HotkeySettings hotkeySettings,
        bool restorePreviousRegistrationOnFailure = false)
    {
        ArgumentNullException.ThrowIfNull(hotkeySettings);

        EnsureHotkeyMutationAllowed();
        HotkeySettingsValidator.Validate(hotkeySettings, "Kisayol ayari uygulama");

        HotkeySettings previousSettings = _hotkeyConfiguration.Snapshot();
        bool wasRegistered = _hotkeyService.IsRegistered || restorePreviousRegistrationOnFailure;
        bool shouldRefreshGlobalRegistration = OperatingSystem.IsWindows();

        try
        {
            if (shouldRefreshGlobalRegistration && _hotkeyService.IsRegistered)
            {
                await _hotkeyService.UnregisterAsync();
            }

            _hotkeyConfiguration.Apply(hotkeySettings);

            if (shouldRefreshGlobalRegistration)
            {
                await _hotkeyService.RegisterAsync();
            }

            await _hotkeySettingsStore.SaveAsync(_hotkeyConfiguration.Snapshot());
        }
        catch (Exception applyException)
        {
            List<Exception> recoveryErrors = [];

            try
            {
                if (_hotkeyService.IsRegistered)
                {
                    await _hotkeyService.UnregisterAsync();
                }
            }
            catch (Exception unregisterException)
            {
                recoveryErrors.Add(
                    new InvalidOperationException(
                        "Kismen uygulanan kisayol kaydi temiz bir sekilde geri alinamadi.",
                        unregisterException));
            }

            try
            {
                _hotkeyConfiguration.Apply(previousSettings);
            }
            catch (Exception restoreConfigurationException)
            {
                recoveryErrors.Add(
                    new InvalidOperationException(
                        "Onceki kisayol yapilandirmasi geri yuklenemedi.",
                        restoreConfigurationException));
            }

            if (shouldRefreshGlobalRegistration && wasRegistered)
            {
                try
                {
                    await _hotkeyService.RegisterAsync();
                }
                catch (Exception restoreRegistrationException)
                {
                    recoveryErrors.Add(
                        new InvalidOperationException(
                            "Onceki kisayol kaydi yeniden yuklenemedi.",
                            restoreRegistrationException));
                }
            }

            UpdateHotkeySummary();
            RefreshUiState();

            if (recoveryErrors.Count > 0)
            {
                List<Exception> aggregateErrors = [applyException];
                aggregateErrors.AddRange(recoveryErrors);
                throw new AggregateException(
                    "Kisayol ayari guncellemesi basarisiz oldu ve geri alma islemi tam olarak tamamlanamadi.",
                    aggregateErrors);
            }

            throw;
        }

        UpdateHotkeySummary();
        RefreshUiState();
    }

    private async Task<bool> SuspendHotkeysForModalDialogAsync()
    {
        if (!OperatingSystem.IsWindows() || !_hotkeyService.IsRegistered)
        {
            return false;
        }

        await _hotkeyService.UnregisterAsync();
        return true;
    }

    private async Task RestoreHotkeysAfterModalDialogAsync()
    {
        if (!OperatingSystem.IsWindows() || _hotkeyService.IsRegistered)
        {
            return;
        }

        await _hotkeyService.RegisterAsync();
    }

    private void UpdateHotkeySummary()
    {
        string recordHotkey = FormatHotkey(_hotkeyConfiguration.RecordToggleHotkey);
        string playbackHotkey = FormatHotkey(_hotkeyConfiguration.PlaybackToggleHotkey);
        string stopHotkey = FormatHotkey(_hotkeyConfiguration.StopHotkey);
        string hotkeySettingsHotkey = FormatHotkey(_hotkeyConfiguration.HotkeySettingsHotkey);

        _toolbarControl.SetHotkeyHints(
            recordHotkey,
            stopHotkey,
            playbackHotkey,
            hotkeySettingsHotkey);
    }

    private static string FormatHotkey(HotkeyBinding hotkeyBinding)
    {
        List<string> parts = [];

        if (hotkeyBinding.Modifiers.HasFlag(HotkeyModifiers.Control))
        {
            parts.Add("Ctrl");
        }

        if (hotkeyBinding.Modifiers.HasFlag(HotkeyModifiers.Shift))
        {
            parts.Add("Shift");
        }

        if (hotkeyBinding.Modifiers.HasFlag(HotkeyModifiers.Alt))
        {
            parts.Add("Alt");
        }

        if (hotkeyBinding.Modifiers.HasFlag(HotkeyModifiers.Windows))
        {
            parts.Add("Win");
        }

        parts.Add(VirtualKeyDisplayNameFormatter.Format(hotkeyBinding.VirtualKeyCode));
        return string.Join("+", parts);
    }
}

