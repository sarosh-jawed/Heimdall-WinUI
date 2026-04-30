using Heimdall.Application.Errors;

namespace Heimdall.Application.Contracts;

public interface IUserMessageService
{
    UserMessage BuildMessage(
        Exception exception,
        string fallbackTitle,
        string fallbackMessage);
}
