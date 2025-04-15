using Microsoft.AspNetCore.Mvc;
using ChatBotApi;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly SemanticKernelService _semanticKernelService;
    private readonly SearchService _searchService;

    public ChatController(SemanticKernelService semanticKernelService, SearchService searchService)
    {
        _semanticKernelService = semanticKernelService;
        _searchService = searchService;
    }

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        // Step 1: Retrieve document content
        var documentContent = await _searchService.SearchDocumentsAsync(request.Query);

        // Step 2: Generate AI response with document context
        var aiPrompt = $"Use the following document context to answer: {documentContent}\n\nUser: {request.Query}";
        var response = await _semanticKernelService.GenerateResponseAsync(aiPrompt);

        return Ok(new { response });
    }
}

public class ChatRequest
{
    public string Query { get; set; }
}