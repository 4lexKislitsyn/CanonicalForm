using CanonicalForm.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core.Interfaces
{
    public interface IGroupsSearcher
    {
        IEnumerable<GroupModel> SearchGroups(string validatedFormula);
    }
}
