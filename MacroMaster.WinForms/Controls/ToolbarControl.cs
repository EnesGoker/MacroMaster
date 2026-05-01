using MacroMaster.WinForms.Theme;

namespace MacroMaster.WinForms.Controls;

public enum PlaybackButtonState
{
    Play,
    Pause,
    Resume
}

public readonly record struct ToolbarButtonState(
    bool CanRecord,
    bool CanStop,
    bool CanPlayback,
    bool CanSave,
    bool CanLoad,
    bool CanEditHotkeys);

public partial class ToolbarControl : UserControl
{
    private string? _recordHotkey = "F8";
    private string? _stopHotkey = "F10";
    private string? _playbackHotkey = "F9";
    private string? _hotkeysHotkey = "F12";
    private string _recordLabel = "Kaydi Baslat";
    private string _playbackLabel = "Oynat";

    public event EventHandler? RecordToggleClicked;
    public event EventHandler? StopClicked;
    public event EventHandler? PlaybackToggleClicked;
    public event EventHandler? SaveClicked;
    public event EventHandler? LoadClicked;
    public event EventHandler? HotkeysClicked;

    public ToolbarControl()
    {
        InitializeComponent();
        ApplyTheme();
        WireEvents();
        SetHotkeyHints("F8", "F10", "F9", "F12");
    }

    public void UpdateRecordButton(bool isRecording)
    {
        _recordLabel = isRecording ? "Kaydi Durdur" : "Kaydi Baslat";
        SetButtonText(recordButton, _recordLabel, _recordHotkey);
        ApplyButtonAccent(recordButton, isRecording ? DesignTokens.AccentRed : DesignTokens.AccentRed);
    }

    public void UpdatePlaybackButton(PlaybackButtonState state)
    {
        _playbackLabel = state switch
        {
            PlaybackButtonState.Pause => "Duraklat",
            PlaybackButtonState.Resume => "Devam Et",
            _ => "Oynat"
        };

        SetButtonText(playbackButton, _playbackLabel, _playbackHotkey);
        ApplyButtonAccent(playbackButton, DesignTokens.Accent);
    }

    public void SetButtonsEnabled(ToolbarButtonState state)
    {
        recordButton.Enabled = state.CanRecord;
        stopButton.Enabled = state.CanStop;
        playbackButton.Enabled = state.CanPlayback;
        saveButton.Enabled = state.CanSave;
        loadButton.Enabled = state.CanLoad;
        hotkeysButton.Enabled = state.CanEditHotkeys;
    }

    public void SetHotkeyHints(string record, string stop, string playback, string hotkeys)
    {
        _recordHotkey = record;
        _stopHotkey = stop;
        _playbackHotkey = playback;
        _hotkeysHotkey = hotkeys;

        SetButtonText(recordButton, _recordLabel, _recordHotkey);
        SetButtonText(stopButton, "Durdur", _stopHotkey);
        SetButtonText(playbackButton, _playbackLabel, _playbackHotkey);
        SetButtonText(hotkeysButton, "Kisayollar", _hotkeysHotkey);
    }

    private void WireEvents()
    {
        recordButton.Click += (_, _) => RecordToggleClicked?.Invoke(this, EventArgs.Empty);
        stopButton.Click += (_, _) => StopClicked?.Invoke(this, EventArgs.Empty);
        playbackButton.Click += (_, _) => PlaybackToggleClicked?.Invoke(this, EventArgs.Empty);
        saveButton.Click += (_, _) => SaveClicked?.Invoke(this, EventArgs.Empty);
        loadButton.Click += (_, _) => LoadClicked?.Invoke(this, EventArgs.Empty);
        hotkeysButton.Click += (_, _) => HotkeysClicked?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyTheme()
    {
        BackColor = DesignTokens.Surface;
        MinimumSize = new Size(0, DesignTokens.ToolbarHeight);
        MaximumSize = new Size(int.MaxValue, DesignTokens.ToolbarHeight);

        foreach (Control control in toolbarLayoutPanel.Controls)
        {
            if (control is Button button)
            {
                ApplyToolbarButtonStyle(button);
            }
        }

        ApplyButtonAccent(recordButton, DesignTokens.AccentRed);
        ApplyButtonAccent(playbackButton, DesignTokens.Accent);
    }

    private static void ApplyToolbarButtonStyle(Button button)
    {
        button.BackColor = DesignTokens.Surface2;
        button.ForeColor = DesignTokens.TextPrimary;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = DesignTokens.BorderBright;
        button.FlatAppearance.BorderSize = 1;
        button.Font = DesignTokens.FontUiBold;
        button.TextAlign = ContentAlignment.MiddleCenter;
        button.UseVisualStyleBackColor = false;
        button.Cursor = Cursors.Hand;
    }

    private static void ApplyButtonAccent(Button button, Color accent)
    {
        button.FlatAppearance.BorderColor = accent;
    }

    private static void SetButtonText(Button button, string label, string? hotkey)
    {
        button.Text = string.IsNullOrWhiteSpace(hotkey)
            ? label
            : $"{label} ({hotkey})";
    }
}
