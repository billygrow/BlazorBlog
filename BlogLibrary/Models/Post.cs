
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogLibrary.Models;
public class Post
{
    [Required]
    public string ID { get; set; } = DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture);
    [Required]
    public string Slug { get; set; }
    [Required]
    public string Content { get; set; }
}
