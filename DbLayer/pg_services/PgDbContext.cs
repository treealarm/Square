using DbLayer.Models;
using DbLayer.Models.Actions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DbLayer
{
  internal class PgDbContext : DbContext
  {
    private readonly NpgsqlDataSource _source;
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


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseNpgsql(
        _source
      //    @"Server=(localdb)\mssqllocaldb;Database=Test;ConnectRetryCount=0"
      );
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      ConfigureValues(modelBuilder);
      ConfigureIntegro(modelBuilder);
      ConfigureEvents(modelBuilder);
      ConfigureActions(modelBuilder);
      ConfigureStates(modelBuilder); // добавляем states
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
      modelBuilder.Entity<PgDBObjExtraProperty>(entity =>
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
