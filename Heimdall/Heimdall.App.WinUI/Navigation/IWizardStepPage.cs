using System.Threading;
using System.Threading.Tasks;

namespace Heimdall.App.WinUI.Navigation;

public interface IWizardStepPage
{
    Task<WizardStepResult> OnNextAsync(CancellationToken cancellationToken);
}
