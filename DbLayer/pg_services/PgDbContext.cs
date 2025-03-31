using DbLayer.Models;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using System;

namespace DbLayer
{
  internal class PgDbContext : DbContext
  {
    private readonly IOptions<MapDatabaseSettings> _geoStoreDatabaseSettings;
    public DbSet<DBEvent> Events { get; set; }
    public DbSet<DBIntegro> Integro { get; set; }
    public DbSet<DBIntegroType> IntegroTypes { get; set; }
    
    public PgDbContext(
      DbContextOptions<PgDbContext> options,
      IOptions<MapDatabaseSettings> geoStoreDatabaseSettings):base(options) 
    {
      _geoStoreDatabaseSettings = geoStoreDatabaseSettings;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      //////////////////////integro_types start////////////
      modelBuilder.Entity<DBIntegroType>(entity =>
      {
        entity.HasKey(e => e.i_type);
        entity.ToTable("integro_types");

        // Настройка связи один ко многим
        entity.HasMany(e => e.children)             // Указываем, что таблица имеет много элементов в дочерней таблице
              .WithOne()                            // будет одна ссылка на родителя
              .HasForeignKey(ep => ep.i_type)       // Указываем внешний ключ
              .OnDelete(DeleteBehavior.Cascade);    // При удалении события, его свойства также будут удалены
      });

      modelBuilder.Entity<DBIntegroTypeChild>(entity =>
      {
        entity.HasKey(e => new { e.i_type, e.child_i_type });
        entity.ToTable("integro_type_children");
      });
      //////////////////////integro_types end////////////
      modelBuilder.Entity<DBIntegro>(entity =>
      {
        entity.ToTable("integro");
      });

      // Настройка таблицы DBEvent, связанной с таблицей events
      modelBuilder.Entity<DBEvent>(entity =>
      {
        entity.ToTable("events"); // Указываем имя таблицы для DBEvent

        // Настройка связи один ко многим между DBEvent и DBObjExtraProperty
        entity.HasMany(e => e.extra_props)                    // Указываем, что DBEvent имеет много DBObjExtraProperty
              .WithOne()                                      // В DBObjExtraProperty будет одна ссылка на DBEvent
              .HasForeignKey(ep => ep.owner_id)               // Указываем внешний ключ в DBObjExtraProperty
              .OnDelete(DeleteBehavior.Cascade);              // При удалении события, его свойства также будут удалены
      });

      // Настройка таблицы DBObjExtraProperty, связанной с таблицей event_props
      modelBuilder.Entity<PgDBObjExtraProperty>(entity =>
      {
        entity.ToTable("event_props"); // Указываем имя таблицы для DBObjExtraProperty
      });
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      var your_username = Environment.GetEnvironmentVariable("POSTGRES_USER");
      var your_password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
       

      var EventsCollectionName = _geoStoreDatabaseSettings.Value.EventsCollectionName;
      var DbName = _geoStoreDatabaseSettings.Value.DatabaseName;

      var builder = new NpgsqlConnectionStringBuilder
      {
        Host = _geoStoreDatabaseSettings.Value.PgHost,
        Database = _geoStoreDatabaseSettings.Value.DatabaseName,
        Username = your_username,
        Password = your_password,
        Port = _geoStoreDatabaseSettings.Value.PgPort
      };

      string connectionString = builder.ConnectionString;

      optionsBuilder.UseNpgsql(
        connectionString
      //    @"Server=(localdb)\mssqllocaldb;Database=Test;ConnectRetryCount=0"
      );
    }
  }
}
