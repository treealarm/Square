﻿using Google.Protobuf.WellKnownTypes;
using GrpcDaprLib;
using GrpcTracksClient;
using LeafletAlarmsGrpc;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;

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
      var d_types = new DiagramTypesProto();

      var d_type = new DiagramTypeProto()
      {
        Id = Utils.LongTo24String(1),
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

      await client.UpdateDiagramTypes(d_types);

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
