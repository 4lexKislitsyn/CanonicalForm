using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core.Interfaces
{
    public interface IGroupsRenderer
    {
        string Render(IDictionary<string, GroupModel> groups);
    }
}
