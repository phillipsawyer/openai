using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace OpenAI.GPT3.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<T> GetFromJsonAsync<T>(this HttpClient client, string uri)
        {
            var response = await client.GetAsync(uri);
            return await response.Content.ReadFromJsonAsync<T>() ?? throw new InvalidOperationException();
        }

        public static async Task<TResponse> PostAndReadAsAsync<TResponse>(this HttpClient client, string uri, object requestModel)
        {
            var response = await client.PostAsJsonAsync(uri, requestModel, new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            });
            return await response.Content.ReadFromJsonAsync<TResponse>() ?? throw new InvalidOperationException();
        }

        public static async IAsyncEnumerable<TResponse> PostAndReadAsAsyncEnumerable<TResponse>(this HttpClient client, string uri, object requestModel)
        {
            var jsonContent = JsonSerializer.Serialize(requestModel, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault }); //or whenwritingnull
            var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using var req = new HttpRequestMessage(HttpMethod.Post, uri);
            req.Content = stringContent;

            var response = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);

            if (response.IsSuccessStatusCode)
            {
                await using var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream);
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line.StartsWith("data: "))
                        line = line["data: ".Length..]; //remove data: prefix
                    if (line == "[DONE]")
                    {
                        yield break;
                    }

                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var res = JsonConvert.DeserializeObject<TResponse>(line.Trim());
                        yield return res;
                    }
                }
            }
            else
            {
                throw new HttpRequestException("Error calling OpenAi API to get completion.  HTTP status code: " + response.StatusCode.ToString() + ". Request body: " + jsonContent);
            }
        }

        public static async Task<TResponse> PostFileAndReadAsAsync<TResponse>(this HttpClient client, string uri, HttpContent content)
        {
            var response = await client.PostAsync(uri, content);
            return await response.Content.ReadFromJsonAsync<TResponse>() ?? throw new InvalidOperationException();
        }

        public static async Task<TResponse> DeleteAndReadAsAsync<TResponse>(this HttpClient client, string uri)
        {
            var response = await client.DeleteAsync(uri);
            return await response.Content.ReadFromJsonAsync<TResponse>() ?? throw new InvalidOperationException();
        }
    }
}