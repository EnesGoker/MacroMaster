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
        SetHotkeyHints("F8", "F9", "F10", "F12");
    }

    public void UpdateRecordButton(bool isRecording)
    {
        recordButton.Text = isRecording ? "Kaydi Durdur" : "Kaydi Baslat";
        ApplyButtonAccent(recordButton, isRecording ? DesignTokens.AccentRed : DesignTokens.AccentRed);
    }

    public void UpdatePlaybackButton(PlaybackButtonState state)
    {
        playbackButton.Text = state switch
        {
            PlaybackButtonState.Pause => "Duraklat",
            PlaybackButtonState.Resume => "Devam Et",
            _ => "Oynat"
        };

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
        SetButtonText(recordButton, recordButton.Text, record);
        SetButtonText(stopButton, "Durdur", stop);
        SetButtonText(playbackButton, playbackButton.Text, playback);
        SetButtonText(hotkeysButton, "Kisayollar", hotkeys);
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
            : $"{label}   {hotkey}";
    }
}
