using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogLibrary.Services;
public interface IBlogService
{
    Task SavePost(Post post);
}
