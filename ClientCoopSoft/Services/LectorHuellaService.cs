using ClientCoopSoft.DTO;
using DPUruNet;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

public class LectorHuellaService
{
    private Reader? _reader;
    private TaskCompletionSource<HuellaResultado?>? _tcs;

    public async Task<HuellaResultado?> CapturarHuellaAsync()
    {
        try
        {
            var readers = ReaderCollection.GetReaders();
            if (readers is null || readers.Count == 0)
                throw new Exception("No se detectó ningún lector de huellas.");

            _reader = readers[0];
            var result = _reader.Open(Constants.CapturePriority.DP_PRIORITY_COOPERATIVE);
            if (result != Constants.ResultCode.DP_SUCCESS)
                throw new Exception("No se pudo abrir el lector.");

            _tcs = new TaskCompletionSource<HuellaResultado?>();
            _reader.On_Captured += Reader_OnCaptured;

            var rc = _reader.CaptureAsync(
                Constants.Formats.Fid.ANSI,
                Constants.CaptureProcessing.DP_IMG_PROC_DEFAULT,
                _reader.Capabilities.Resolutions[0]
            );

            if (rc != Constants.ResultCode.DP_SUCCESS)
                throw new Exception($"No se pudo iniciar la captura: {rc}");

            return await _tcs.Task;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al iniciar captura: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }

    private void Reader_OnCaptured(CaptureResult captureResult)
    {
        try
        {
            if (captureResult == null || captureResult.ResultCode != Constants.ResultCode.DP_SUCCESS || captureResult.Data == null)
            {
                _tcs?.TrySetResult(null);
                return;
            }

            var fmdResult = FeatureExtraction.CreateFmdFromFid(captureResult.Data, Constants.Formats.Fmd.ANSI);
            if (fmdResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                throw new Exception("No se pudo crear el template de huella.");

            var imagen = ConvertFidToBitmapImage(captureResult.Data);

            _tcs?.TrySetResult(new HuellaResultado
            {
                TemplateBytes = fmdResult.Data.Bytes,
                ImagenHuella = imagen
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al procesar huella: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            _tcs?.TrySetResult(null);
        }
        finally
        {
            _reader?.CancelCapture();
            if (_reader != null)
            {
                _reader.On_Captured -= Reader_OnCaptured;
                _reader.Dispose();
            }
        }
    }

    public BitmapImage ConvertFidToBitmapImage(Fid fid)
    {
        var view = fid.Views[0];
        int w = view.Width;
        int h = view.Height;
        byte[] raw = view.RawImage;

        // Crear bitmap en System.Drawing
        using var bmp = new System.Drawing.Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

        // Paleta grayscale
        var pal = bmp.Palette;
        for (int i = 0; i < 256; i++)
            pal.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
        bmp.Palette = pal;

        var data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h),
                                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                bmp.PixelFormat);
        for (int y = 0; y < h; y++)
            System.Runtime.InteropServices.Marshal.Copy(raw, y * w, data.Scan0 + y * data.Stride, w);
        bmp.UnlockBits(data);

        // Guardar a MemoryStream como BMP
        using var ms = new MemoryStream();
        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
        ms.Seek(0, SeekOrigin.Begin);

        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = ms;
        image.EndInit();
        image.Freeze(); // <-- Muy importante para usarlo desde cualquier hilo

        return image;
    }
}
