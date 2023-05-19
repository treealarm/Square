// See https://aka.ms/new-console-template for more information
using Domain;
using GPKGConvertor;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

Console.WriteLine("GPKG Convertor 1.0");

string[] arguments = Environment.GetCommandLineArgs();



try
{
  string sourceDirectory = arguments[1];
  var txtFiles = Directory.EnumerateFiles(sourceDirectory, "*.gpkg", SearchOption.AllDirectories);

  FiguresDTO figDto = new FiguresDTO();
  figDto.figs = new List<FigureGeoDTO>();

  foreach (string currentFile in txtFiles)
  {
    Convertor conv = new Convertor();
    var file = Path.Combine(sourceDirectory, currentFile);
    Console.WriteLine(file);

    if (!conv.Configure(file))
    {
      Console.WriteLine($"failed to configure {currentFile}!");
      return;
    }
    
    var jList = conv.ReadGeometry(currentFile);    
    //File.WriteAllText($"{file}.json", JsonConvert.SerializeObject(jList));

    conv.AddJListToFigs(Path.GetFileNameWithoutExtension(currentFile),jList, figDto);
  }

  File.WriteAllText(
    "D:\\TESTS\\Leaflet\\leaflet_data\\import_data\\states.json",
    //$"{Path.Combine(sourceDirectory, "states.json")}",
    JsonConvert.SerializeObject(figDto));
}
catch (Exception e)
{
  Console.WriteLine(e.Message);
}