using MacroMaster.WinForms.Composition;

namespace MacroMaster.WinForms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        AppCompositionRoot compositionRoot = AppCompositionRoot.Create();
        global::System.Windows.Forms.Application.Run(compositionRoot.CreateMainForm());
    }
}