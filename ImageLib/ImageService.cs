using System.Net.NetworkInformation;

namespace ImageLib
{
 using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

  public class ImageService
  {
    static public byte[] GenerateJpegImage()
    {
      var width = 640;
      var height = 480;

      using (var image = new Image<Rgba32>(width, height))
      {
        // Заливаем фон белым цветом
        image.Mutate(x => x.Fill(Color.White));

        // Создаём красный Pen (перо) для рисования
        var pen = new SolidPen(Color.Red, 5);  // Красный цвет и толщина линии 5 пикселей

        // Рисуем красный круг
        var ellipse = new EllipsePolygon(width/2,height/2,height/4);
       
        image.Mutate(ctx => ctx.Draw(pen, ellipse));

        // Сохраняем изображение в байтовый массив в формате JPEG
        using (var ms = new MemoryStream())
        {
          image.SaveAsJpeg(ms);
          return ms.ToArray();
        }
      }
    }

    static public bool IsValidImage(byte[] imageBytes)
    {
      try
      {
        using (var ms = new MemoryStream(imageBytes))
        {
          var image = Image.Load(ms); 
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        return false;
      }
      return true;
    }
  }
}
