using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSample.db
{
  public class User
  {
    public User()
    {
      this.carts = new List<Cart>();
    }

    [Key]
    public int id { get; set; }
    [Required]
    [MaxLength(100)]
    [EmailAddress]
    [Index("ix_email", IsUnique = true)]
    public string email { get; set; }
    [Required]
    [MaxLength(50)]
    public string name { get; set; }

    public virtual ICollection<Cart> carts { get; set; }
  }
}
