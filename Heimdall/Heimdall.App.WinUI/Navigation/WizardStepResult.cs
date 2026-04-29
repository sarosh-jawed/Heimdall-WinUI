namespace Heimdall.App.WinUI.Navigation;

public sealed record WizardStepResult(
    bool CanContinue,
    string? ErrorTitle = null,
    string? ErrorMessage = null)
{
    public static WizardStepResult Success() => new(true);

    public static WizardStepResult Failure(string title, string message)
    {
        return new WizardStepResult(false, title, message);
    }
}
