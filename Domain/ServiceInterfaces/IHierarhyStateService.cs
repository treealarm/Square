﻿using Domain.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IHierarhyStateService
  {
    Task Init();
    Task OnStatesChanged(List<ObjectStateDTO> newObjs);
  }
}