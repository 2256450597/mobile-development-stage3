using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;

namespace TastyMealPlanner.Services;

/// <summary>ML-powered food image classifier. Loads an ONNX model (exported from
/// Google Teachable Machine), preprocesses camera-captured photos, runs inference,
/// and returns top-3 food category predictions with confidence scores.</summary>
public class FoodClassifier : IFoodClassifier
{
    private InferenceSession? _session;
    private List<string> _labels = new();
    private const int ImageSize = 224;
    private const string ModelFileName = "food_model.onnx";
    private const string LabelsFileName = "food_labels.txt";

    /// <inheritdoc />
    public bool IsModelLoaded => _session != null && _labels.Count > 0;

    /// <summary>Gets the last error message if model loading or classification failed.</summary>
    public string? LastError { get; private set; }

    /// <summary>Loads the ONNX model and label file from app resources.
    /// Copies from the app package to app data directory on first launch
    /// because ONNX Runtime needs a file-system path (not a stream).</summary>
    public FoodClassifier()
    {
        try
        {
            var modelPath = Path.Combine(FileSystem.AppDataDirectory, ModelFileName);
            var labelsPath = Path.Combine(FileSystem.AppDataDirectory, LabelsFileName);

            // Copy model from app package to data directory if not already there
            if (!File.Exists(modelPath))
                CopyFromPackage(ModelFileName, modelPath);
            if (!File.Exists(labelsPath))
                CopyFromPackage(LabelsFileName, labelsPath);

            if (!File.Exists(modelPath))
            {
                LastError = $"Model file '{ModelFileName}' not found. Add it to Resources/Raw.";
                return;
            }
            if (!File.Exists(labelsPath))
            {
                LastError = $"Labels file '{LabelsFileName}' not found. Add it to Resources/Raw.";
                return;
            }

            _session = new InferenceSession(modelPath);
            _labels = File.ReadAllLines(labelsPath)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrEmpty(l))
                .ToList();

            if (_labels.Count == 0)
            {
                LastError = "Labels file is empty. Ensure food_labels.txt contains at least one label.";
            }
        }
        catch (Exception ex)
        {
            LastError = $"Model loading failed: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Food classifier init failed: {ex.Message}");
        }
    }

    private static void CopyFromPackage(string filename, string destPath)
    {
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync(filename).Result;
            using var file = File.Create(destPath);
            stream.CopyTo(file);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to copy {filename} from package: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public Task<ClassificationResult> ClassifyAsync(string imagePath)
    {
        if (!IsModelLoaded)
            return Task.FromResult(new ClassificationResult
            {
                Success = false,
                Error = LastError ?? "ML model is not loaded. Ensure food_model.onnx and food_labels.txt are in Resources/Raw."
            });

        if (!File.Exists(imagePath))
            return Task.FromResult(new ClassificationResult
            {
                Success = false,
                Error = $"Image file not found: {imagePath}. Please capture a new photo."
            });

        try
        {
            using var bitmap = LoadAndPreprocess(imagePath);
            if (bitmap == null)
                return Task.FromResult(new ClassificationResult
                {
                    Success = false,
                    Error = "Unable to load or resize the image. The file may be corrupted or in an unsupported format."
                });

            // Build input tensor: [1, 3, 224, 224] NCHW format (channels first)
            var inputData = new float[1 * 3 * ImageSize * ImageSize];
            var stride = ImageSize * ImageSize;
            for (var y = 0; y < ImageSize; y++)
            {
                for (var x = 0; x < ImageSize; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    var offset = y * ImageSize + x;
                    inputData[offset] = pixel.Red / 255f;           // R channel
                    inputData[stride + offset] = pixel.Green / 255f; // G channel
                    inputData[2 * stride + offset] = pixel.Blue / 255f; // B channel
                }
            }

            var inputName = _session!.InputMetadata.Keys.First();
            var inputTensor = new DenseTensor<float>(inputData, new[] { 1, 3, ImageSize, ImageSize });
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputName, inputTensor) };

            // Run inference
            using var results = _session.Run(inputs);
            var output = results.First().AsTensor<float>();
            var scores = output.ToArray();

            // Get top-3 predictions
            var predictions = scores
                .Select((score, i) => new ClassificationPrediction
                {
                    Label = i < _labels.Count ? _labels[i] : $"Class {i}",
                    Confidence = score * 100f
                })
                .OrderByDescending(p => p.Confidence)
                .Take(3)
                .ToList();

            return Task.FromResult(new ClassificationResult
            {
                Success = true,
                Predictions = predictions
            });
        }
        catch (Exception ex)
        {
            var errorMsg = $"Classification error: {ex.Message}";
            System.Diagnostics.Debug.WriteLine(errorMsg);
            return Task.FromResult(new ClassificationResult
            {
                Success = false,
                Error = $"Image classification failed. {ex.Message}. Please try with a clearer photo."
            });
        }
    }

    /// <summary>Loads the source image, resizes to 224×224, and normalises pixel values.</summary>
    private static SKBitmap? LoadAndPreprocess(string imagePath)
    {
        try
        {
            using var original = SKBitmap.Decode(imagePath);
            if (original == null) return null;

            // Resize to model input size
            var resized = original.Resize(new SKImageInfo(ImageSize, ImageSize),
                SKFilterQuality.Medium);
            return resized;
        }
        catch
        {
            return null;
        }
    }
}
