using GrpcDaprLib;
using GrpcTracksClient;
using LeafletAlarmsGrpc;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class DiagramUpdater
{

  public DiagramUpdater()
  {
  }

  public static async Task UploadDiagramsAsync()
  {
    const string TeslaCar = "Tesla_Car";

    await Task.Delay(1000);
    try
    {
      var protoUploadFile = new UploadFileProto()
      {
        MainFolder = "static_files",
        Path = "diagram_types",
        FileName = "tesla.jpeg",        
      };

      protoUploadFile.FileData = Google.Protobuf.ByteString.CopyFrom(
        File.ReadAllBytes($"files/{protoUploadFile.FileName}"));

      // Создаем клиента gRPC и подключаемся
      using var client = new GrpcUpdater();

      // Загружаем файл
      await client.UploadFile(protoUploadFile);
      

      Console.WriteLine("File uploaded.");
      var d_type = new DiagramTypesProto();
      d_type.DiagramTypes.Add(
        new DiagramTypeProto()
        {
          Id = Utils.LongTo24String(1),
          Name = TeslaCar,
          Src = $"{protoUploadFile.MainFolder}/{protoUploadFile.Path}/{protoUploadFile.FileName}",
          Regions =  
          { 
            new DiagramTypeRegionProto()
            {
              Id = "speed",
              Geometry = new DiagramCoordProto()
              {
                Left = 0.1,
                Top = 0.7,
                Height = 0.1,
                Width = 0.2
              }
            },
            new DiagramTypeRegionProto()
            {
              Id = "temperature",
              Geometry = new DiagramCoordProto()
              {
                Left = 0.1,
                Top = 0.9,
                Height = 0.1,
                Width = 0.2
              }
            }
          }
        }
      );

      await client.UpdateDiagramTypes(d_type);

      var diag = new DiagramsProto();

      for (long carId = 1; carId < MoveObject.MaxCars; carId++)
      {
        diag.Diagrams.Add(new DiagramProto()
        {
          DgrType = TeslaCar,
          Id = Utils.LongTo24String(carId),
          Geometry = new DiagramCoordProto()
          {
            Left = 50,
            Top = 50,
            Width = 690,
            Height = 400          
          }
        });
      }
      await client.UpdateDiagrams(diag);

    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error UploadDiagrams: {ex.Message}");
      
    }
  }
}
