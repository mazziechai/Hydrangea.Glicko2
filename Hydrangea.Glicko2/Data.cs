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
using System.Collections.Immutable;
using System.Linq;
using Hydrangea.Glicko2.Interfaces;

namespace Hydrangea.Glicko2
{
    /// <summary>
    /// Holds information about a rating.
    /// </summary>
    public class RatingInfo : IRatingInfo
    {
        public double Rating { get; private set; } = 1500;
        public double RatingDeviation { get; private set; } = 350;
        public double Volatility { get; private set; } = 0.06;

        public double WorkingRating { get; set; }
        public double WorkingRatingDeviation { get; set; }
        public double WorkingVolatility { get; set; }

        /// <summary>
        /// Sets Rating to 1500, RD set to 350, and volatility to 0.06.
        /// </summary>
        public RatingInfo() { }

        /// <summary>
        /// Sets Rating, RatingDeviation, and Volatility with to the given parameters.
        /// </summary>
        public RatingInfo(double rating, double rd, double volatility)
        {
            Rating = rating;
            RatingDeviation = rd;
            Volatility = volatility;
        }

        public void FinalizeChanges()
        {
            Rating = WorkingRating;
            RatingDeviation = WorkingRatingDeviation;
            Volatility = WorkingVolatility;
        }
    }

    /// <summary>
    /// Holds information about a 1v1 match that was played.
    /// </summary>
    public class Result : IResult
    {
        public ImmutableDictionary<IRatingInfo, double> Scores { get; }

        /// <summary>
        /// A constructor that uses two <see cref='RatingInfo'/> objects
        /// and two score values to set the <see cref='Scores'/> and
        /// <see cref='Participants'/> properties.
        /// </summary>
        public Result(RatingInfo participant1,
                      double score1,
                      RatingInfo participant2,
                      double score2)
        {
            Scores = new Dictionary<IRatingInfo, double>()
            {
                { participant1, score1 },
                { participant2, score2 }
            }.ToImmutableDictionary();
        }

        public bool IsDraw()
        {
            if (Scores.Values.Distinct().Any()) return true;
            else return false;
        }
    }
}
