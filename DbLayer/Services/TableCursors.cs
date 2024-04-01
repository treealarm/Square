using MongoDB.Driver;
using System;
using System.Collections.Concurrent;


namespace DbLayer.Services
{
  internal class CursorsInfo<T> : IDisposable
  {
    public CursorsInfo(IAsyncCursor<T> cursor) 
    {
      _cursor = cursor;
      RefreshTime();
    }
    private IAsyncCursor<T> _cursor;
    public IAsyncCursor<T> Cursor
    { get { return _cursor; } }
    public int setId { get; set; }
    public void RefreshTime()
    {
      creationTime = DateTime.UtcNow;
    }
    private DateTime creationTime { get; set; }

    public void Dispose()
    {
      if (_cursor != null)
      {
        _cursor.Dispose();
        _cursor = null;
      }
    }
  }
  internal class TableCursors<T> : IDisposable
  {
    private ConcurrentDictionary<string, CursorsInfo<T>> _cursors = new ConcurrentDictionary<string, CursorsInfo<T>>();

    public void Dispose()
    {
      foreach (var kvp in  _cursors)
      {
        kvp.Value.Dispose();
      }
      _cursors.Clear();
    }

    public CursorsInfo<T> Get(string id, int set_id)
    {
      if (string.IsNullOrEmpty(id))
      {
        return null;
      }
      CursorsInfo<T> retVal;

      if (_cursors.TryGetValue(id, out retVal))
      {
        if (retVal.setId == set_id)
        {
          retVal.RefreshTime();
          return retVal;
        }
        _cursors.TryRemove(id, out retVal);
        retVal.Dispose();
      }

      return null;
    }
    public void Remove(string id)
    {
      CursorsInfo<T> retVal;

      if (_cursors.TryRemove(id, out retVal))
      {
        retVal.Dispose();
      }
    }
    public void Add(string id, int set_id, IAsyncCursor<T> cursor)
    {
      var retVal = new CursorsInfo<T>(cursor);
      retVal.setId = set_id;
      _cursors.TryAdd(id, retVal);
    }
  }
}
