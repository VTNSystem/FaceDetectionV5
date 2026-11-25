using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace SpartonFaceAdmin.Infrastructure.Camera;

public sealed record CameraDeviceInfo(int Index, string Name);

/// <summary>
/// Enumerates DirectShow video input devices in the exact index order expected by OpenCV DSHOW.
/// </summary>
public static class CameraDeviceEnumerator
{
    private static readonly Guid ClsidSystemDeviceEnum = new("62BE5D10-60EB-11d0-BD3B-00A0C911CE86");
    private static readonly Guid ClsidVideoInputDeviceCategory = new("860BB310-5D01-11d0-BD3B-00A0C911CE86");
    private static readonly Guid IidIPropertyBag = new("55272A00-42CB-11CE-8135-00AA004BB851");

    public static IReadOnlyList<CameraDeviceInfo> GetUsbCameras()
    {
        var devices = new List<CameraDeviceInfo>();

        object? devEnumObj = null;
        IEnumMoniker? enumMoniker = null;
        try
        {
            Type? devEnumType = Type.GetTypeFromCLSID(ClsidSystemDeviceEnum);
            if (devEnumType == null)
                return devices;

            devEnumObj = Activator.CreateInstance(devEnumType);
            if (devEnumObj is not ICreateDevEnum devEnum)
                return devices;

            int hr = devEnum.CreateClassEnumerator(ClsidVideoInputDeviceCategory, out enumMoniker, 0);
            if (hr != 0 || enumMoniker == null)
                return devices;

            IMoniker[] monikers = new IMoniker[1];
            int idx = 0;
            while (enumMoniker.Next(1, monikers, IntPtr.Zero) == 0)
            {
                IMoniker moniker = monikers[0];
                try
                {
                    Guid bagId = IidIPropertyBag;
                    moniker.BindToStorage(null!, null!, ref bagId, out object bagObj);
                    if (bagObj is IPropertyBag bag)
                    {
                        try
                        {
                            object? nameVal = null;
                            if (bag.Read("FriendlyName", ref nameVal, IntPtr.Zero) == 0 && nameVal is string friendly && !string.IsNullOrWhiteSpace(friendly))
                            {
                                devices.Add(new CameraDeviceInfo(idx, friendly.Trim()));
                            }
                            else
                            {
                                devices.Add(new CameraDeviceInfo(idx, $"USB Camera {idx}"));
                            }
                        }
                        finally
                        {
                            Marshal.ReleaseComObject(bag);
                        }
                    }
                    idx++;
                }
                finally
                {
                    Marshal.ReleaseComObject(moniker);
                }
            }
        }
        catch
        {
            // Return whatever devices were discovered cleanly.
        }
        finally
        {
            if (enumMoniker != null) Marshal.ReleaseComObject(enumMoniker);
            if (devEnumObj != null) Marshal.ReleaseComObject(devEnumObj);
        }

        return devices;
    }

    [ComImport, Guid("29840822-5B84-11D0-BD3B-00A0C911CE86"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ICreateDevEnum
    {
        [PreserveSig]
        int CreateClassEnumerator([In] ref Guid pType, [Out] out IEnumMoniker? ppEnumMoniker, [In] int dwFlags);
    }

    [ComImport, Guid("55272A00-42CB-11CE-8135-00AA004BB851"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IPropertyBag
    {
        [PreserveSig]
        int Read([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropName, [In, Out, MarshalAs(UnmanagedType.Struct)] ref object? pVar, IntPtr pErrorLog);

        [PreserveSig]
        int Write([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropName, [In, MarshalAs(UnmanagedType.Struct)] ref object pVar);
    }
}
