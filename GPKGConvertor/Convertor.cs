using GeoJSON.Net;
using GeoJSON.Net.Converters;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OSGeo.GDAL;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPKGConvertor
{
  internal class Convertor
  {
    private volatile bool _configured;
    private volatile String _geopackagePath;
    private volatile DataSource _dataSource;

    public bool Configure(String geopackagePath)
    {
      if (_configured)
        return true;

      if (!File.Exists(geopackagePath))
      {
        Console.WriteLine("geopackage path: " + geopackagePath + " does not exist!");
        return false;
      }

      GdalConfiguration.ConfigureGdal();
      GdalConfiguration.ConfigureOgr();

      Gdal.AllRegister();
      Ogr.RegisterAll();

      _geopackagePath = geopackagePath;

      _dataSource = Ogr.Open(_geopackagePath, 0);

      if (_dataSource == null)
      {
        Console.WriteLine("failed to open geopackage file!");
        return false;
      }

      _configured = true;

      return true;
    }

    public string ReadGeometry()
    {
      List<JObject> list = new List<JObject>();

      var layerCount = _dataSource.GetLayerCount();

      for (int l = 0; l < layerCount; l++)
      {
        var resultLayer = _dataSource.GetLayerByIndex(l);

        if (resultLayer == null)
          continue;

        string geoCol = resultLayer.GetGeometryColumn();

        if (string.IsNullOrEmpty(geoCol))
        {
          continue;
        }

        //String sql = $"SELECT * FROM {table}";

        //Layer resultLayer = _dataSource.ExecuteSQL(sql, null, "");
        

        var count = resultLayer.GetFeatureCount(0);

        if (count == 0)
        {
          continue;
        }

        Feature feature = resultLayer.GetNextFeature();


        while (feature != null)
        {
          Geometry geometry = feature.GetGeometryRef();

          var id = feature.GetFieldAsString("id");
          string[] options = new string[1];
          var s = geometry.ExportToJson(options);

          JObject o = JObject.Parse(s);
          o.AddFirst(new JProperty("id", id));

          list.Add(o);
          feature = resultLayer.GetNextFeature();
        }
      }

      return JsonConvert.SerializeObject(list);
    }
  }
}
