using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeCrm.Core.Enums
{
    public enum EngagementSegment
    {
        New = 1,  // First contact within 30 days, 0-1 engagements
        Growing = 2,  // 2-5 engagements, increasing frequency
        Core = 3,  // 6+ engagements, consistent attendance
        AtRisk = 4,  // Was core, no engagement in 60+ days
        Lapsed = 5   // No engagement in 180+ days
    }
}
