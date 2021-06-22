// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

/*
 * This sample uses the Azure Bing Spell Check API to check the spelling of a query.
 * It then offers suggestions for corrections.
 * Bing Spell Check API: 
 * https://docs.microsoft.com/en-us/rest/api/cognitiveservices-bingsearch/bing-spell-check-api-v7-reference
 */

namespace BingSpellCheck
{
    class Program
    {
        // Add your Azure Bing Spell Check key and endpoint to your environment variables.
        static string subscriptionKey = "b41a8b0c9f844927a90797d2db726faa";
        static string endpoint = "https://api.bing.microsoft.com";
        static string path = "/v7.0/spellcheck?";

        // For a list of available markets, go to:
        // https://docs.microsoft.com/rest/api/cognitiveservices/bing-autosuggest-api-v7-reference#market-codes
        static string market = "en-US";
        static string mode = "proof";
        static string query = "Ther is sumthing wrong with this strng!";

        public async static Task Main(string[] args)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            HttpResponseMessage response = new HttpResponseMessage();
            string uri = endpoint + path;

            List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("mkt", market));
            values.Add(new KeyValuePair<string, string>("mode", mode));
            values.Add(new KeyValuePair<string, string>("text", query));

            using (FormUrlEncodedContent content = new FormUrlEncodedContent(values))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                response = await client.PostAsync(uri, content);
            }

            var spellingResult = new SpellingResult();
            if (response.Headers.TryGetValues("X-MSEdge-ClientID", out IEnumerable<string> header_values))
            {
                spellingResult.ClientId = header_values.First();
                Console.WriteLine("Client ID: " + spellingResult.ClientId);
            }
            if (response.Headers.TryGetValues("BingAPIs-SessionId", out IEnumerable<string> session_id))
            {
                spellingResult.TraceId = session_id.First();
                Console.WriteLine("Trace ID: " + spellingResult.TraceId);
            }
            var resultText = await response.Content.ReadAsStringAsync();
            spellingResult.Text = JsonConvert.DeserializeObject<SpellingResponseBody>(resultText);
            Console.WriteLine("");
            Console.WriteLine(resultText);
            spellingResult.CorrectedText = ProcessResults(query, spellingResult.Text.flaggedTokens);
            Console.WriteLine("");
            Console.WriteLine($"Suggested correction: {spellingResult.CorrectedText}");
        }

        static string ProcessResults(string text, List<FlaggedToken> flaggedTokens)
        {
            StringBuilder newTextBuilder = new StringBuilder(text);

            int indexDiff = 0;

            foreach (var token in flaggedTokens)
            {
                if (token.type == "RepeatedToken")
                {
                    newTextBuilder.Remove(token.offset - indexDiff, token.token.Length + 1);
                    indexDiff += token.token.Length + 1;
                }
                else
                {
                    if (token.suggestions.Count > 0)
                    {
                        var suggestedToken = token.suggestions.Where(x => x.score >= 0.7).FirstOrDefault();
                        if (suggestedToken == null)
                            break;

                        // replace the token in the original text

                        newTextBuilder.Remove(token.offset - indexDiff, token.token.Length);
                        newTextBuilder.Insert(token.offset - indexDiff, suggestedToken.suggestion);

                        indexDiff += token.token.Length - suggestedToken.suggestion.Length;
                    }
                }

            }
            return newTextBuilder.ToString();
        }
    }
}