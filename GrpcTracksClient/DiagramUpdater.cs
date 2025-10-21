using IntegrationUtilsLib;
using GrpcTracksClient;
using LeafletAlarmsGrpc;

static public class DiagramUpdater
{

  static bool _successfull = false;

  public static async Task UploadDiagramsAsync()
  {
    const string TeslaCar = "Tesla_Car";

    await Task.Delay(1000);

    if (_successfull)
    {
      return;
    }
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
      var client = Utils.ClientBase.Client;

      // Загружаем файл
      await client!.UploadFileAsync(protoUploadFile);
      
      Console.WriteLine("File uploaded.");
      var d_types = new DiagramTypesProto();

      var d_type = new DiagramTypeProto()
      {
        Id = await Utils.GenerateObjectId("car", 1),
        Name = TeslaCar,
        Src = $"{protoUploadFile.MainFolder}/{protoUploadFile.Path}/{protoUploadFile.FileName}",
      };

      var region1 =
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
        };
      region1.Styles.Add("color", "white");
      region1.Styles.Add("fontSize", "16px");

      var region2 = new DiagramTypeRegionProto()
      {
        Id = "temperature",
        Geometry = new DiagramCoordProto()
        {
          Left = 0.1,
          Top = 0.8,
          Height = 0.1,
          Width = 0.2
        }
      };

      region2.Styles.Add("color", "green");
      region2.Styles.Add("fontSize", "16px");

      d_type.Regions.Add(region1);
      d_type.Regions.Add(region2);

      d_types.DiagramTypes.Add(
        d_type
      );

      await client.UpdateDiagramTypesAsync(d_types);

      var diag = new DiagramsProto();

      for (long carId = 1; carId < IMoveObjectService.MaxCars; carId++)
      {
        diag.Diagrams.Add(new DiagramProto()
        {
          DgrType = TeslaCar,
          Id = await Utils.GenerateObjectId("car", carId),
          Geometry = new DiagramCoordProto()
          {
            Left = 50,
            Top = 50,
            Width = 690,
            Height = 400          
          }
        });
      }
      await client.UpdateDiagramsAsync(diag);

    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error UploadDiagrams: {ex.Message}");
      return;
    }
    _successfull = true;
  }
}
