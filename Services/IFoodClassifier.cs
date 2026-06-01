namespace TastyMealPlanner.Services;

/// <summary>Single classification prediction with label and confidence score.</summary>
public class ClassificationPrediction
{
    /// <summary>Class label name (e.g. "Salad", "Curry").</summary>
    public string Label { get; set; } = string.Empty;
    /// <summary>Confidence score as a percentage (0-100).</summary>
    public float Confidence { get; set; }
}

/// <summary>Result of running the food image classifier over a photo.</summary>
public class ClassificationResult
{
    /// <summary>Whether classification succeeded.</summary>
    public bool Success { get; set; }
    /// <summary>Top-N predictions ordered by confidence descending.</summary>
    public List<ClassificationPrediction> Predictions { get; set; } = new();
    /// <summary>Human-readable summary for UI display.</summary>
    public string Summary => Success && Predictions.Count > 0
        ? $"Top prediction: {Predictions[0].Label} ({Predictions[0].Confidence:F1}%)"
        : "Classification failed or model not available.";
}

/// <summary>ML-powered food image classifier. Runs an ONNX model exported from
/// Google Teachable Machine to classify food photos into trained categories.
/// Uses the Camera hardware to capture an image, then applies computer vision
/// and machine learning inference to classify what food is in the photo.</summary>
public interface IFoodClassifier
{
    /// <summary>Whether the ONNX model file is available and loaded.</summary>
    bool IsModelLoaded { get; }

    /// <summary>Runs inference on the image at the given path. Returns top-3 predictions.</summary>
    Task<ClassificationResult> ClassifyAsync(string imagePath);
}
