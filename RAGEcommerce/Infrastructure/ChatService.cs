using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace RAGEcommerce.Infrastructure;

public class ChatService(Kernel kernel)
{
    public async Task<string> GetChatResponseAsync(string prompt)
    {
        var chat = kernel.GetRequiredService<IChatCompletionService>();

        var history = new ChatHistory();
        history.AddUserMessage(prompt);

        var answer = new StringBuilder();
        var result = chat.GetStreamingChatMessageContentsAsync(history);
        await foreach (var message in result)
        {
            answer.Append(message.Content);
        }

        history.AddAssistantMessage(answer.ToString());

        return answer.ToString();
    }
}
