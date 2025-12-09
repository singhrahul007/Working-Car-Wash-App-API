using Microsoft.AspNetCore.Mvc;

namespace CarWash.Api.Controllers;
[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase {
    [HttpPost("presign")]
    public IActionResult Presign([FromBody] dynamic body){
        // In production: generate S3 presigned URL. Here: return a mock URL.
        var fileName = (string?)body?.fileName ?? "image.jpg";
        var mockUrl = $"https://example-bucket.s3.amazonaws.com/{fileName}";
        return Ok(new { uploadUrl = mockUrl, fileUrl = mockUrl });
    }
}
