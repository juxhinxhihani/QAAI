using System.Text;
using System.Text.Json.Nodes;
using Amazon.Bedrock;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime;
using Amazon.Runtime.Internal.Auth;
using Newtonsoft.Json;
using QAAI.Model;

namespace QAAI.Service;
public class TextClassificationService
{
    private readonly S3Service _s3Service;
    private readonly AmazonBedrockRuntimeClient _bedrockClient;
    private const string modelId = "meta.llama3-1-70b-instruct-v1:0"; // Updated model ID

    public TextClassificationService(AmazonBedrockRuntimeClient bedrockClient, S3Service s3Service)
    {
        _bedrockClient = bedrockClient;
        _s3Service = s3Service;
    }

    public async Task<ModelResponse> ClassifyTextAsync(string inputText)
    {
        
        string s3Data = await _s3Service.GetDataAsync(); 

        var formattedInput = $"<|begin_of_text|><|start_header_id|>user<|end_header_id|>\nBazuar ne keto informacione qe po te jap\n\n{s3Data}\nTe lutem pergjigju kesaj:\n{inputText}?\n<|eot_id|>\n<|start_header_id|>assistant<|end_header_id|>\n";

        var nativeRequest = JsonConvert.SerializeObject(new
        {
            prompt = formattedInput,
            max_gen_len = 512,
            temperature = 0.5
        });
        var request = new InvokeModelRequest()
        {
            ModelId = modelId,
            Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(nativeRequest)),
            ContentType = "application/json"
        };
        
        try
        {
            var response = await _bedrockClient.InvokeModelAsync(request);
            using (var reader = new StreamReader(response.Body))
            {
                string responseBody = await reader.ReadToEndAsync();
                ModelResponse modelResponse = JsonConvert.DeserializeObject<ModelResponse>(responseBody);
                return modelResponse;

            }
        }
        catch (AmazonBedrockRuntimeException ex)
        {
            // Handle Bedrock runtime exceptions
            throw new ApplicationException($"Bedrock runtime error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            // Handle other exceptions
            throw new ApplicationException($"An error occurred: {ex.Message}", ex);
        }
    }
}