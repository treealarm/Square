using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace DbLayer.Models
{
  // Базовый класс для всех моделей
  internal abstract record BasePgEntity
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid id { get; set; } // unique id

    //[Column("xmin")]
    //public uint xmin { get; private set; }
    [Timestamp]
    public uint Version { get; private set; }
  }
}
