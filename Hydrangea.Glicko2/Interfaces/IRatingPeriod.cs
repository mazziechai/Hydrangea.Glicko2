// Copyright (C) 2021 mazziechai
// 
// Glicko-2 is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Glicko-2 is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with Glicko-2. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace Hydrangea.Glicko2.Interfaces
{
    /// <summary>
    /// A standard for rating periods to use with the Glicko-2 class.
    /// </summary>
    public interface IRatingPeriod
    {
        public List<IResult> Results { get; }
        public HashSet<IRatingInfo> Participants { get; }

        public void AddResult(IResult result);
        public void AddResults(IEnumerable<IResult> results);

        public List<IResult> GetParticipantResults(IRatingInfo participant);

        public void Clear();
    }
}
