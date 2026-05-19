using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace ConectaSTI.Api.Extensoes;

public class PlainTextInputFormatter : TextInputFormatter
{
    public PlainTextInputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/plain"));
        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

    protected override bool CanReadType(Type type)
    {
        return type == typeof(string);
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
    {
        var request = context.HttpContext.Request;

        using var reader = new StreamReader(request.Body, encoding);
        var content = await reader.ReadToEndAsync();

        return await InputFormatterResult.SuccessAsync(content);
    }
}
