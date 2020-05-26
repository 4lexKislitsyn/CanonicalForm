using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core.Interfaces
{
    public interface IGroupsDictionaryBuilder
    {
        IDictionary<string, GroupModel> Build(IEnumerable<GroupModel> groups);
    }
}
