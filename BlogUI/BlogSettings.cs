using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogUI;
public class BlogSettings
{
    public int CommentsCloseAfterDays { get; set; } = 10;

    public bool DisplayComments { get; set; } = true;

    //public PostListView ListView { get; set; } = PostListView.TitlesAndExcerpts;

    public string Owner { get; set; } = "The Owner";

    public int PostsPerPage { get; set; } = 4;
}
