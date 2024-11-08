using Microsoft.AspNetCore.Mvc;
using QAAI.Model;
using QAAI.Service;

namespace QAAI.Controller;

[ApiController]
[Route("[controller]")]
public class TextClassificationController : ControllerBase
{
    private readonly TextClassificationService _classificationService;

    public TextClassificationController(TextClassificationService classificationService)
    {
        _classificationService = classificationService;
    }

    [HttpPost]
    public async Task<IActionResult> ClassifyText([FromBody] InputRequest inputText)
    {
        if (string.IsNullOrEmpty(inputText.Text))
        {
            return BadRequest("Input text cannot be empty.");
        }
        try
        {
            var result = await _classificationService.ClassifyTextAsync(inputText.Text);
            return Ok(result);
        }
        catch (System.Exception ex)
        {
            // Log exception (not shown here)
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}