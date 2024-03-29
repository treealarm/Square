﻿using Domain.ServiceInterfaces;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  public class UtilService: IUtilService
  {
    public int Compare(string id1, string id2)
    {
      return new ObjectId(id1).CompareTo(new ObjectId(id2));
    }
  }
}
