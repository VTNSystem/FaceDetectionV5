namespace SpartonFaceAdmin.Core.Models;

public sealed class FaceRecordSummary
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Fullname { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public float SimilarThreshold { get; set; }
    public bool HasFaceMask { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool HasStoredImage { get; set; }
}

public sealed class FaceRecordImages
{
    public int Id { get; set; }
    public byte[]? FullImageBytes { get; set; }
    public byte[]? FaceImageBytes { get; set; }
}

public sealed class FaceRecordInsertRequest
{
    public string Code { get; set; } = string.Empty;
    public string Fullname { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public int FacePositionXc { get; set; }
    public int FacePositionYc { get; set; }
    public int FacePositionW { get; set; }
    public float FacePositionAngle { get; set; }
    public int Eye1X { get; set; }
    public int Eye1Y { get; set; }
    public int Eye2X { get; set; }
    public int Eye2Y { get; set; }
    public byte[] Template { get; set; } = Array.Empty<byte>();
    public byte[] Image { get; set; } = Array.Empty<byte>();
    public byte[] FaceImage { get; set; } = Array.Empty<byte>();
    public float SimilarThreshold { get; set; }
    public bool HasFaceMask { get; set; }
}
