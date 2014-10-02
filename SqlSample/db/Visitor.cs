using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSample.db
{
  public class Visitor
  {
    [Key]
    public int id { get; set; }
    [Required]
    public string name { get; set; }
    public DateTime? firstVisit { get; set; }
    public DateTime? lastVisit { get; set; }
  }
}
