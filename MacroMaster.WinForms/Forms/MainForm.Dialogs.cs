using MacroMaster.Application.Abstractions;
using MacroMaster.Domain.Models;
using MacroMaster.WinForms.Controls;

namespace MacroMaster.WinForms.Forms;

public partial class MainForm
{
    private void ShowError(string message, Exception exception)
    {
        if (IsDisposed)
        {
            return;
        }

        string detail = string.IsNullOrWhiteSpace(exception.Message)
            ? exception.GetType().Name
            : exception.Message;

        _logger.Log(
            AppLogLevel.Error,
            nameof(MainForm),
            message,
            exception);

        ModalDialogOverlay.ShowMessage(
            this,
            $"{message}{Environment.NewLine}{Environment.NewLine}{detail}",
            "MacroMaster",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    private void sessionSummaryControl_PreviewMapRequested(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;

        if (_shutdownInProgress)
        {
            return;
        }

        MacroSession? displayedSession = GetSessionForPlayback();
        PlaybackSettings playbackSettings = BuildPlaybackSettings();
        string statusText = ResolveStatusText(_applicationStateService.CurrentState, playbackSettings);
        ShowMacroPreviewMapDialog(
            BuildSessionSummaryState(displayedSession, statusText),
            displayedSession?.Events,
            _activePlaybackSourceIndex,
            _sessionSummaryControl.PreviewMapScreenBounds);
    }

    private void ShowMacroPreviewMapDialog(
        SessionSummaryState summaryState,
        IReadOnlyList<MacroEvent>? events,
        int? activeSourceEventIndex,
        Rectangle anchorScreenBounds)
    {
        if (_macroPreviewMapDialog is null || _macroPreviewMapDialog.IsDisposed)
        {
            _macroPreviewMapDialog = new MacroPreviewMapDialog();
            _macroPreviewMapDialog.FormClosed += (_, _) => _macroPreviewMapDialog = null;
        }

        _macroPreviewMapDialog.UpdatePreview(
            summaryState,
            events,
            activeSourceEventIndex);

        if (!_macroPreviewMapDialog.Visible)
        {
            _macroPreviewMapDialog.PositionNear(
                anchorScreenBounds,
                RectangleToScreen(ClientRectangle));
            _macroPreviewMapDialog.Show(this);
        }

        _macroPreviewMapDialog.BringToFront();
        _macroPreviewMapDialog.Activate();
    }
}

