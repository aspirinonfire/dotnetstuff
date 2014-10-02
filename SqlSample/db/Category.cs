using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSample.db
{
  public class Category
  {
    public Category()
    {
    }

    [Key]
    public int id { get; set; }
    [Required]
    [MaxLength(50)]
    public string name { get; set; }

    public int? parentCategoryId { get; set; }
    public Category parentCategory { get; set; }

  }
}
