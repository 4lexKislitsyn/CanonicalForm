using CanonicalForm.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core.Interfaces
{
    public interface IGroupsRenderer
    {
        string Render(IEnumerable<GroupModel> groups);
    }
}
