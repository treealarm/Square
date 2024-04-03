using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;


namespace DbLayer.Services
{
  internal class CursorsInfo<T> : IDisposable
  {
    public CursorsInfo(IAsyncCursor<T> cursor) 
    {
      _cursor = cursor;
      ReserveSeconds(60);
    }
    private IAsyncCursor<T> _cursor;
    public IAsyncCursor<T> Cursor
    { get { return _cursor; } }
    public int setId { get; set; }
    public void ReserveSeconds(int reserve_seconds)
    {
      ReservedTillUtc = DateTime.UtcNow + TimeSpan.FromSeconds(reserve_seconds);
    }
    private DateTime ReservedTillUtc { get; set; }
    public bool IsExpired()
    {
      return DateTime.UtcNow > ReservedTillUtc;
    }
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
    private System.Timers.Timer _timer = new System.Timers.Timer(5000);
    public TableCursors()
    {
      _timer.Elapsed += TimerElapsed;
      _timer.AutoReset = true;
      _timer.Start();
    }
    private void TimerElapsed(object sender, ElapsedEventArgs e)
    {
      CleanExpired();
    }
    public void CleanExpired()
    {
      var expiredKeys = new List<string>();

      foreach (var kvp in  _cursors)
      {
        if(kvp.Value.IsExpired())
        {
          expiredKeys.Add(kvp.Key);
        }
      }

      foreach (var k in expiredKeys)
      {
        if(_cursors.TryRemove(k, out var v))
        {
          v.Dispose();
        }        
      }
    }

    public void Dispose()
    {
      _timer.Stop();
      _timer.Dispose();

      foreach (var kvp in _cursors)
      {
        kvp.Value.Dispose();
      }
      _cursors.Clear();
    }

    public CursorsInfo<T> GetById(string id)
    {
      if (string.IsNullOrEmpty(id))
      {
        return null;
      }

      if (_cursors.TryGetValue(id, out var retVal))
      {
          return retVal;
      }

      return null;
    }
    public CursorsInfo<T> Get(string id, int search_hash)
    {
      if (string.IsNullOrEmpty(id))
      {
        return null;
      }
      CursorsInfo<T> retVal;

      if (_cursors.TryGetValue(id, out retVal))
      {
        if (retVal.setId == search_hash)
        {
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
