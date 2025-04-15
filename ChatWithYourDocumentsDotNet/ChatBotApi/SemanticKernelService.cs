using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ChatBotApi;

public class SemanticKernelService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;

    public SemanticKernelService(Kernel kernel)
    {
        _kernel = kernel;
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<string> GenerateResponseAsync(string prompt)
    {
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(prompt);

        var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory);
        return response.Content;
    }
}