using CanonicalForm.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core.Interfaces
{
    public interface IGroupsSearcher
    {
        /// <summary>
        /// Search expressions in formula.
        /// </summary>
        /// <param name="validatedFormula"></param>
        /// <returns></returns>
        IEnumerable<GroupModel> SearchGroups(string validatedFormula);
    }
}
