using GrpcDaprLib;
using GrpcTracksClient;
using LeafletAlarmsGrpc;

public class DiagramUpdater
{

  public DiagramUpdater()
  {
  }

  public static async Task UploadDiagramsAsync()
  {
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
          Name = "Tesla_Car",
          Src = $"{protoUploadFile.MainFolder}/{protoUploadFile.Path}/{protoUploadFile.FileName}",
          Regions =  
          { 
            new DiagramTypeRegionProto()
            {
              Id = "speed",
              Geometry = new DiagramCoordProto()
              {
                Left = 0,
                Top = 1,
                Height = 0.3,
                Width = 0.5
              }
            },
            new DiagramTypeRegionProto()
            {
              Id = "temperature",
              Geometry = new DiagramCoordProto()
              {
                Left = 0,
                Top = 1.4,
                Height = 0.3,
                Width = 0.5
              }
            }
          }
        }
      );

      await client.UpdateDiagramTypes(d_type);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error UploadDiagrams: {ex.Message}");
      
    }
  }
}
