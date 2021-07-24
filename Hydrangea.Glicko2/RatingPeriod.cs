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
using System.Linq;
using Hydrangea.Glicko2.Interfaces;

namespace Hydrangea.Glicko2
{
    /// <summary>
    /// Holds information about the current rating period.
    /// </summary>
    public class RatingPeriod : IRatingPeriod
    {
        /// <summary>
        /// All of the <see cref='IResult'/> instances recorded in this rating
        /// period.
        /// </summary>
        public List<IResult> Results { get; } = new();
        /// <summary>
        /// All of the <see cref='IRatingInfo'/> instances recorded in this 
        /// rating period.
        /// </summary>
        public HashSet<IRatingInfo> Participants { get; } = new();

        /// <summary>
        /// Leaves <see cref='Results'/> and <see cref='Participants'/> empty.
        /// </summary>
        public RatingPeriod() { }
        /// <summary>
        /// Sets <see cref='Results'/> to <paramref name='results'/> and
        /// adds each unique <see cref='IRatingInfo'/> in a <see cref='IResult'/>
        /// to <see cref='Participants'/>.
        /// </summary>
        public RatingPeriod(IEnumerable<IResult> results)
        {
            Results = results.ToList();
            foreach (var result in Results)
            {
                Participants.UnionWith(result.Scores.Keys);
            }
        }
        /// <summary>
        /// Sets <see cref='Results'/> and <see cref='Participants'/> to
        /// <paramref name='results'/> and <paramref name='participants'/>,
        /// respectively.
        /// </summary>
        public RatingPeriod(IEnumerable<IResult> results,
                            IEnumerable<IRatingInfo> participants)
        {
            Results = results.ToList();
            Participants = participants.ToHashSet();
        }

        /// <summary>
        /// Adds a <see cref='IResult'/> to <see cref='Results'/> and
        /// the new participants in the <see cref='IResult'/> to
        /// <see cref='Participants'/>.
        /// </summary>
        public void AddResult(IResult result)
        {
            Results.Add(result);
            Participants.UnionWith(result.Scores.Keys);
        }
        /// <summary>
        /// Adds a range of <see cref='IResult'/> to <see cref='Results'/>
        /// and the new participants in each <see cref='IResult'/> to
        /// <see cref='Participants'/>.
        /// </summary>
        public void AddResults(IEnumerable<IResult> results)
        {
            Results.AddRange(results);
            foreach (var result in Results)
            {
                Participants.UnionWith(result.Scores.Keys);
            }
        }

        /// <summary>
        /// Gets every <see cref='IResult'/> that contains the given
        /// <see cref='IRatingInfo'/>.
        /// </summary>
        public List<IResult> GetParticipantResults(IRatingInfo rating)
        {
            List<IResult> resultList = new();

            foreach (var result in Results)
            {
                if (result.Scores.Keys.Contains(rating))
                {
                    resultList.Add(result);
                }
            }

            return resultList;
        }

        /// <summary>
        /// Clears both <see cref='Results'/> and <see cref='Participants'/>
        /// at the same time.
        /// </summary>
        public void Clear()
        {
            Results.Clear();
            Participants.Clear();
        }
    }
}
