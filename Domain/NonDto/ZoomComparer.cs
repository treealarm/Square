﻿
using System.Collections.Generic;


namespace Domain
{
  public class ZoomComparer : IComparer<FigureZoomedDTO>
  {
    Dictionary<string, LevelDTO> _zoomLevels;
    public ZoomComparer(Dictionary<string, LevelDTO> zoomLevels)
    {
      _zoomLevels = zoomLevels;
    }
    public int Compare(FigureZoomedDTO? x, FigureZoomedDTO? y)
    {
      if (x?.zoom_level == y?.zoom_level)
      {
        return 0;
      }
      if (string.IsNullOrEmpty(x?.zoom_level))
      {
        return -1;
      }
      if (string.IsNullOrEmpty(y?.zoom_level))
      {
        return 1;
      }
      if (_zoomLevels.TryGetValue(x.zoom_level, out LevelDTO? levelX))
      {
        if (_zoomLevels.TryGetValue(y.zoom_level, out LevelDTO? levelY))
        {
          return levelY.zoom_min - levelX.zoom_min;
        }
      }
      return 0;
    }
  }
}
