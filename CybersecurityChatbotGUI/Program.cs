// ============================================================
// Program.cs - This is the ENTRY POINT of the application
// It simply starts Windows Forms and opens our main window
// ============================================================

namespace CybersecurityChatbotGUI;

internal static class Program
{
    [STAThread]  // Required for WinForms - tells Windows this is a single-threaded UI app
    static void Main()
    {
        // Enable modern visual styles (makes buttons/textboxes look nice)
        ApplicationConfiguration.Initialize();

        // Open the NameEntryForm first (where user types their name)
        Application.Run(new NameEntryForm());
    }
}
