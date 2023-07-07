using OpenAI.GPT3.Extensions;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels.ResponseModels;

namespace OpenAI.GPT3.Managers;

public partial class OpenAIService : ICompletionService
{
    public async Task<CompletionCreateResponse> CreateCompletion(CompletionCreateRequest createCompletionRequest, string? engineId)
    {
        return await _httpClient.PostAndReadAsAsync<CompletionCreateResponse>(_endpointProvider.CompletionCreate(ProcessEngineId(engineId)), createCompletionRequest);
    }
    
    public IAsyncEnumerable<CompletionCreateResponse> StreamCompletionAsync(CompletionCreateRequest createCompletionRequest, string? engineId)
    {
        return _httpClient.PostAndReadAsAsyncEnumerable<CompletionCreateResponse>(_endpointProvider.CompletionCreate(ProcessEngineId(engineId)), createCompletionRequest);
    }
}