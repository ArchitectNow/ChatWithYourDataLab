using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace ChatBotApi;

public class SearchService
{
    private readonly SearchClient _searchClient;

    public SearchService(SearchClient searchClient)                 
    {
        _searchClient = searchClient;
    }

    public async Task<string> SearchDocumentsAsync(string query)
    {
        var options = new SearchOptions
        {
            QueryType = SearchQueryType.Semantic,
            Size = 2,
            IncludeTotalCount = false,
            SemanticSearch = new SemanticSearchOptions
            {
                SemanticConfigurationName = "SemanticConfig",
            }
        };
        var response = await _searchClient.SearchAsync<SearchDocument>(query, options);
 
        var content = string.Empty;
 
        foreach (var result in response.Value.GetResults())
        {
            if (result.Score > 5) {
                content += Environment.NewLine +  result.Document["content"].ToString();
            }
        }
 
        return content;
    }
}