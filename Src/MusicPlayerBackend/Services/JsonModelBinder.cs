using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MusicPlayerBackend.Services;

public sealed class JsonModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        // Check the value sent in
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None || valueProviderResult.FirstValue == default)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        // Attempt to convert the input value
        var valueAsString = valueProviderResult.FirstValue;
        var result = JsonSerializer.Deserialize(valueAsString, bindingContext.ModelType); // TODO: Import serializer settings
        if (result == null)
            return Task.CompletedTask;

        bindingContext.Result = ModelBindingResult.Success(result);
        return Task.CompletedTask;
    }
}
