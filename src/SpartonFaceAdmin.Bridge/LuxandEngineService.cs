using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Luxand;
using SpartonFaceAdmin.Contracts;

namespace SpartonFaceAdmin.Bridge;

internal sealed class LuxandEngineService : IDisposable
{
    private const string LuxandLicense =
        "LImcnZzkzw8RrawhS1F1kirLGcLBEC5xWkpNWBFXCnSiOYEzzcbTSJ0M5NqD7e6Ai7KCJi4g63nKFPVyxhRHq0vwGN/d+bqXummVuocUNIzUO6EWAhlMUv/dtztWSAMNzi0RjrZm0XOJzq0ukos9ZQT4L+aiGm1mKjDEKuPETBk=";

    private readonly object _sync = new();
    private bool _initialized;

    public void Initialize()
    {
        lock (_sync)
        {
            if (_initialized)
                return;

            int act = FSDK.ActivateLibrary(LuxandLicense);
            if (act != FSDK.FSDKE_OK)
                throw new InvalidOperationException($"FSDK.ActivateLibrary failed with code {act}");

            int init = FSDK.InitializeLibrary();
            if (init != FSDK.FSDKE_OK)
                throw new InvalidOperationException($"FSDK.InitializeLibrary failed with code {init}");

            FSDK.SetFaceDetectionThreshold(3);
            FSDK.SetFaceDetectionParameters(true, true, 512);

            int err = 0;
            // Use mask model if deployed alongside binary, otherwise keep permissive uncertain features.
            string maskParam = File.Exists("fd_masks1.bin")
                ? "FaceDetectionModel=fd_masks1.bin;TrimFacesWithUncertainFacialFeatures=false"
                : "TrimFacesWithUncertainFacialFeatures=false";
            FSDK.SetParameters(maskParam, ref err);

            _initialized = true;
        }
    }

    public BridgeResponse Handle(BridgeRequest request)
    {
        lock (_sync)
        {
            try
            {
                if (request.Command == BridgeCommand.Ping)
                {
                    return new BridgeResponse { Success = _initialized };
                }

                if (request.ImageBytes == null || request.ImageBytes.Length == 0)
                {
                    return new BridgeResponse
                    {
                        Success = false,
                        ErrorMessage = "Image payload is empty."
                    };
                }

                FSDK.SetFaceDetectionThreshold(Math.Max(1, Math.Min(10, request.DetectionThreshold)));

                using var ms = new MemoryStream(request.ImageBytes);
                using var bmp = new Bitmap(ms);
                using var image = new FSDK.CImage(bmp);

                var facePos = image.DetectFace();
                if (facePos.w <= 0 || facePos.xc <= 0 || facePos.yc <= 0)
                {
                    return new BridgeResponse
                    {
                        Success = true,
                        FaceDetected = false,
                        ImageWidth = image.Width,
                        ImageHeight = image.Height
                    };
                }

                var response = new BridgeResponse
                {
                    Success = true,
                    FaceDetected = true,
                    ImageWidth = image.Width,
                    ImageHeight = image.Height,
                    FaceBox = new FaceBoxDto
                    {
                        Xc = facePos.xc,
                        Yc = facePos.yc,
                        W = facePos.w,
                        Angle = (float)facePos.angle
                    }
                };

                if (request.Command == BridgeCommand.ExtractTemplate)
                {
                    var features = image.DetectFacialFeaturesInRegion(ref facePos);
                    byte[] template = image.GetFaceTemplateInRegion(ref facePos);

                    int left = Math.Max(0, (int)(facePos.xc - Math.Round(facePos.w * 0.5)));
                    int top = Math.Max(0, (int)(facePos.yc - Math.Round(facePos.w * 0.5)));
                    int right = Math.Min(image.Width, (int)(facePos.xc + Math.Round(facePos.w * 0.5)));
                    int bottom = Math.Min(image.Height, (int)(facePos.yc + Math.Round(facePos.w * 0.5)));

                    using var faceCrop = image.CopyRect(left, top, right, bottom);
                    using Image clrFull = image.ToCLRImage();
                    using Image clrFace = faceCrop.ToCLRImage();

                    response.Eye1 = features != null && features.Length > 0
                        ? new EyePointDto { X = features[0].x, Y = features[0].y }
                        : new EyePointDto();
                    response.Eye2 = features != null && features.Length > 1
                        ? new EyePointDto { X = features[1].x, Y = features[1].y }
                        : new EyePointDto();
                    response.Template = template;
                    response.FullImageJpeg = EncodeJpeg(clrFull);
                    response.FaceImageJpeg = EncodeJpeg(clrFace);
                }

                return response;
            }
            catch (Exception ex)
            {
                return new BridgeResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }

    private static byte[] EncodeJpeg(Image image)
    {
        using var ms = new MemoryStream();
        image.Save(ms, ImageFormat.Jpeg);
        return ms.ToArray();
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_initialized)
            {
                try { FSDK.FinalizeLibrary(); } catch { }
                _initialized = false;
            }
        }
    }
}
