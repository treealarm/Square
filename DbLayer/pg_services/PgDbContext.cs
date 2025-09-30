using DbLayer.Models;
using DbLayer.Models.Actions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace DbLayer
{
  internal class PgDbContext : DbContext
  {
    private readonly NpgsqlDataSource _source;

    
    public DbSet<DBGeoObject> GeoObjects { get; set; }
    public DbSet<DBDiagram> Diagrams { get; set; }
    public DbSet<DBDiagramTypeRegion> DiagramTypeRegions { get; set; }
    public DbSet<DBDiagramType> DiagramTypes { get; set; }
    public DbSet<DBGroup> Groups { get; set; }
    public DbSet<DBTrackPoint> Tracks { get; set; }
    public DbSet<DBMarkerProp> Properties { get; set; }
    public DbSet<DBMarker> Markers { get; set; }
    public DbSet<DBLevel> Levels { get; set; }
    public DbSet<DBObjectRightValue> Rights { get; set; }

    public DbSet<DBEvent> Events { get; set; }

    public DbSet<DBIntegro> Integro { get; set; }
    public DbSet<DBIntegroType> IntegroTypes { get; set; }

    public DbSet<DBActionExe> Actions { get; set; }
    public DbSet<DBActionExeResult> ActionResults { get; set; }

    public DbSet<DBValue> Values { get; set; }

    public DbSet<DBObjectState> ObjectStates { get; set; }
    public DbSet<DBObjectStateDescription> ObjectStateDescriptions { get; set; }
    public DbSet<DBAlarmState> AlarmStates { get; set; }


    public PgDbContext(
      DbContextOptions<PgDbContext> options,
      NpgsqlDataSource source) :base(options) 
    {
      _source = source;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
      builder.UseNpgsql(_source, npgsqlOptions =>
      {
        npgsqlOptions.UseNetTopologySuite();
        npgsqlOptions.MigrationsAssembly(typeof(PgDbContext).Assembly.FullName);

        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: int.MaxValue, // фактически бесконечно
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null
        );
      });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      ConfigureValues(modelBuilder);
      ConfigureIntegro(modelBuilder);
      ConfigureEvents(modelBuilder);
      ConfigureActions(modelBuilder);
      ConfigureStates(modelBuilder);
      ConfigureRights(modelBuilder);
      ConfigureLevels(modelBuilder);
      ConfigureMarkers(modelBuilder);
      ConfigureProperties(modelBuilder);
      ConfigureTrackPoints(modelBuilder);
      ConfigureGroups(modelBuilder);
      ConfigureDiagramTypes(modelBuilder);
      ConfigureDiagrams(modelBuilder);
      ConfigureGeoObjects(modelBuilder);
    }

    private void ConfigureGeoObjects(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<DBGeoObject>(entity =>
      {
        entity.ToTable("geo_objects");

        entity.HasKey(e => e.id);

        // figure (геометрия)
        entity.Property(e => e.figure)
              .IsRequired()
              .HasColumnName("figure")
              .HasColumnType("geometry"); // EF Core + NetTopologySuite

        // radius
        entity.Property(e => e.radius)
              .HasColumnName("radius");

        // zoom_level
        entity.Property(e => e.zoom_level)
              .HasColumnName("zoom_level");

        // индексы
        entity.HasIndex(e => e.zoom_level)
              .HasDatabaseName("idx_geo_objects_zoom_level");

        entity.HasIndex(e => e.figure)
              .HasDatabaseName("idx_geo_objects_figure_gist")
              .HasMethod("gist");
      });
    }


    private void ConfigureDiagrams(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<DBDiagram>(entity =>
      {
        entity.ToTable("diagrams");

        entity.HasKey(e => e.id);

        entity.Property(e => e.id)
              .HasColumnName("id");

        entity.Property(e => e.dgr_type)
              .HasColumnName("dgr_type")
              .IsRequired();

        entity.Property(e => e.geometry)
              .HasColumnName("geometry")
              .HasColumnType("jsonb")
              .HasConversion(
                  v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                  v => JsonSerializer.Deserialize<DBDiagramCoord>(v, (JsonSerializerOptions)null)
              );

        entity.Property(e => e.region_id)
              .HasColumnName("region_id");

        entity.Property(e => e.background_img)
              .HasColumnName("background_img");
      });
    }

    private void ConfigureDiagramTypes(ModelBuilder modelBuilder)
    {
      // Родительская сущность
      modelBuilder.Entity<DBDiagramType>(entity =>
      {
        entity.ToTable("diagram_types");
        entity.HasKey(e => e.id);

        entity.Property(e => e.id).HasColumnName("id");

        entity.Property(e => e.name)
              .HasColumnName("name")
              .IsRequired();

        entity.Property(e => e.src).HasColumnName("src");

        entity.HasIndex(e => e.name)
              .HasDatabaseName("idx_diagram_types_name")
              .IsUnique();

        entity.HasMany(d => d.regions)
              .WithOne(r => r.diagram_type)
              .HasForeignKey(r => r.diagram_type_id)
              .OnDelete(DeleteBehavior.Cascade);
      });

      // Дочерняя сущность с составным ключом
      modelBuilder.Entity<DBDiagramTypeRegion>(entity =>
      {
        entity.ToTable("diagram_type_regions");

        // составной ключ
        entity.HasKey(e => new { e.diagram_type_id, e.region_key });

        entity.Property(e => e.diagram_type_id).HasColumnName("diagram_type_id");
        entity.Property(e => e.region_key).HasColumnName("region_key").IsRequired();

        entity.Property(e => e.geometry)
              .HasColumnName("geometry")
              .HasColumnType("jsonb")
              .HasConversion(
                  v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                  v => JsonSerializer.Deserialize<DBDiagramCoord>(v, (JsonSerializerOptions)null)
              );

        entity.Property(e => e.styles)
              .HasColumnName("styles")
              .HasColumnType("jsonb")
              .HasConversion(
                  v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                  v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null)
              );
      });

    }

    private void ConfigureGroups(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<DBGroup>(entity =>
      {
        entity.ToTable("groups");

        // PK из BaseEntity
        entity.HasKey(e => e.id);

        // objid
        entity.Property(e => e.objid)
              .HasColumnName("objid")
              .IsRequired();

        // name
        entity.Property(e => e.name)
              .HasColumnName("name")
              .IsRequired()
              .HasMaxLength(255);

        // индексы
        entity.HasIndex(e => e.name)
              .HasDatabaseName("idx_groups_name")
              .IsUnique(); // если имена уникальны

        entity.HasIndex(e => e.objid)
              .HasDatabaseName("idx_groups_objid");

        // если нужны составные индексы
        // entity.HasIndex(e => new { e.objid, e.name })
        //       .HasDatabaseName("idx_groups_objid_name");
      });
    }

    private void ConfigureTrackPoints(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<DBTrackPoint>(entity =>
      {
        entity.ToTable("track_points");

        entity.HasKey(e => e.id);

        entity.Property(e => e.object_id)
              .HasColumnName("object_id");

        // timestamp
        entity.Property(e => e.timestamp)
              .IsRequired()
              .HasColumnName("timestamp");

        // figure (геометрия)
        entity.Property(e => e.figure)
              .IsRequired()
              .HasColumnName("figure")
              .HasColumnType("geometry")
              ; // EF Core + NetTopologySuite

        // radius
        entity.Property(e => e.radius)
              .HasColumnName("radius");

        // zoom_level
        entity.Property(e => e.zoom_level)
              .HasColumnName("zoom_level");

        // extra_props как jsonb
        entity.Property(e => e.extra_props)
              .HasColumnName("extra_props")
              .HasColumnType("jsonb");

        // индексы
        entity.HasIndex(e => e.timestamp).HasDatabaseName("idx_track_points_ts");
        entity.HasIndex(e => e.figure).HasDatabaseName("idx_track_points_figure_gist")
              .HasMethod("gist");
        entity.HasIndex(e => e.extra_props).HasDatabaseName("idx_track_points_extra_props")
              .HasMethod("gin");
      });
    }
    private void ConfigureProperties(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<DBMarkerProp>(entity =>
      {
        entity.ToTable("properties");
        entity.HasKey(e => e.id);

        entity.Property(e => e.prop_name).IsRequired();
        entity.Property(e => e.str_val);
        entity.Property(e => e.visual_type);
        entity.Property(e => e.object_id).IsRequired();

        // Индекс для выборки по объекту
        entity.HasIndex(e => e.object_id)
              .HasDatabaseName("idx_properties_object_id");

        // Индекс для поиска по имени свойства
        entity.HasIndex(e => e.prop_name)
              .HasDatabaseName("idx_properties_prop_name");

        // Составной индекс: имя + значение
        entity.HasIndex(e => new { e.prop_name, e.str_val })
              .HasDatabaseName("idx_properties_prop_name_str_val");
      });
    }

    private void ConfigureMarkers(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<DBMarker>(entity =>
      {
        entity.ToTable("objects");
        entity.HasKey(e => e.id);

        entity.HasIndex(e => e.parent_id).HasDatabaseName("idx_objects_parent_id");
        entity.HasIndex(e => e.owner_id).HasDatabaseName("idx_objects_owner_id");
        entity.HasIndex(e => new { e.id, e.owner_id }).HasDatabaseName("idx_objects_id_owner_id");
      });
    }

    private void ConfigureLevels(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<DBLevel>(entity =>
      {
        entity.ToTable("levels");

        entity.HasKey(e => e.id);

        entity.Property(e => e.id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        entity.Property(e => e.zoom_level)
            .HasColumnName("zoom_level")
            .IsRequired();

        entity.Property(e => e.zoom_min)
            .HasColumnName("zoom_min")
            .IsRequired();

        entity.Property(e => e.zoom_max)
            .HasColumnName("zoom_max")
            .IsRequired();
      });

    }
    private void ConfigureRights(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<DBObjectRightValue>(entity =>
      {
        entity.ToTable("rights");

        entity.HasKey(e => new { e.object_id, e.role }); // композитный PK

        entity.Property(e => e.object_id).IsRequired();
        entity.Property(e => e.role).IsRequired();
        entity.Property(e => e.value).IsRequired();
      });
    }
    private void ConfigureValues(ModelBuilder modelBuilder) 
    {
      modelBuilder.Entity<DBValue>(entity =>
      {
        entity.ToTable("db_values");

        entity.HasKey(e => e.id); // ✅ правильный ключ — id (наследуется от BasePgEntity)

        entity.Property(e => e.owner_id).IsRequired();
        entity.Property(e => e.name).IsRequired();
        entity.Property(e => e.value).HasColumnType("jsonb");
      });
    }


    private void ConfigureIntegro(ModelBuilder modelBuilder) 
    {
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
    }

    private void ConfigureEvents(ModelBuilder modelBuilder) 
    {
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
      modelBuilder.Entity<EventProp>(entity =>
      {
        entity.ToTable("event_props"); // Указываем имя таблицы для DBObjExtraProperty
      });
    }

    private void ConfigureActions(ModelBuilder modelBuilder) 
    {
      modelBuilder.Entity<DBActionExe>(entity =>
      {
        entity.ToTable("action_executions");
        entity.HasKey(e => e.id);

        entity.HasMany(e => e.parameters)
              .WithOne()
              .HasForeignKey(ep => ep.action_execution_id)
              .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne<DBActionExeResult>()
              .WithOne()
              .HasForeignKey<DBActionExeResult>(r => r.id)
              .OnDelete(DeleteBehavior.Cascade);
      });

      // Таблица action_parameters
      modelBuilder.Entity<DBActionParameter>(entity =>
      {
        entity.ToTable("action_parameters");
        entity.HasKey(e => e.id);
      });

      // Таблица action_result
      modelBuilder.Entity<DBActionExeResult>(entity =>
      {
        entity.ToTable("action_result");
        entity.HasKey(e => e.id);
      });
    }

    private void ConfigureStates(ModelBuilder modelBuilder)
    {
      // object_states
      modelBuilder.Entity<DBObjectState>(entity =>
      {
        entity.ToTable("object_states");
        entity.HasKey(e => e.id);

        entity.Property(e => e.timestamp)
              .IsRequired()
              .HasDefaultValueSql("now()");

        entity.HasMany(e => e.states)
              .WithOne()
              .HasForeignKey(e => e.object_id)
              .OnDelete(DeleteBehavior.Cascade);
      });

      // object_state_values
      modelBuilder.Entity<DBObjectStateValue>(entity =>
      {
        entity.ToTable("object_state_values");
        entity.HasKey(e => e.id);

        entity.Property(e => e.state)
              .IsRequired();

        entity.Property(e => e.object_id)
              .IsRequired();

        entity.HasIndex(e => e.object_id)
              .HasDatabaseName("idx_object_state_values_object_id");
      });

      // object_state_descriptions
      modelBuilder.Entity<DBObjectStateDescription>(entity =>
      {
        entity.ToTable("object_state_descriptions");
        entity.HasKey(e => e.id);

        entity.Property(e => e.alarm)
              .IsRequired()
              .HasDefaultValue(false);

        entity.Property(e => e.state)
              .IsRequired();

        entity.Property(e => e.state_descr);
        entity.Property(e => e.state_color);

        entity.HasIndex(e => new { e.state, e.alarm })
              .HasDatabaseName("idx_state_descriptions_state_alarm");

        entity.HasIndex(e => e.state)
              .IsUnique(); // UNIQUE(state)
      });

      // alarm_states
      modelBuilder.Entity<DBAlarmState>(entity =>
      {
        entity.ToTable("alarm_states");
        entity.HasKey(e => e.id);

        entity.Property(e => e.alarm)
              .IsRequired();
      });
    }


  }
}
