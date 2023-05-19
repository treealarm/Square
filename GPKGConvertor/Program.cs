// See https://aka.ms/new-console-template for more information
using GPKGConvertor;

Console.WriteLine("GPKG Convertor 1.0");

string[] arguments = Environment.GetCommandLineArgs();



try
{
  string sourceDirectory = arguments[1];
  var txtFiles = Directory.EnumerateFiles(sourceDirectory, "*.gpkg", SearchOption.AllDirectories);

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

    string json = conv.ReadGeometry();
    File.WriteAllText($"{file}.json", json);
  }
}
catch (Exception e)
{
  Console.WriteLine(e.Message);
}