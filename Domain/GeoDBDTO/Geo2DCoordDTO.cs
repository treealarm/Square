﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain
{
  // This class is for leaflet, so Y goes first in array.
  public class Geo2DCoordDTO : List<double>
  {
    [JsonIgnore]
    public double Y
    {
      get
      {
        return this[0];
      }
      set
      {
        if (this.Count < 1)
        {
          this.Add(value);
        }
        else
        {
          this[0] = value;
        }
      }
    }

    [JsonIgnore]
    public double X
    {
      get
      {
        return this[1];
      }
      set
      {
        if (this.Count < 1)
        {
          this.Add(0);
        }

        if (this.Count < 2)
        {
          this.Add(value);
        }
        else
        {
          this[1] = value;
        }
      }
    }

    [JsonIgnore]
    public double Lat
    {
      get { return Y; }
      set { Y = value; } 
    }

    [JsonIgnore]
    public double Lon
    {
      get { return X; }
      set { X = value; }
    }
  }
}
