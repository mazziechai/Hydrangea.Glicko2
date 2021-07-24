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

using System.Collections.Immutable;

namespace Hydrangea.Glicko2.Interfaces
{
    /// <summary>
    /// Standards for a Glicko-2 rating.
    /// </summary>
    public interface IRatingInfo
    {
        /// <summary>
        /// Differentiates between different ratings beyond reference equality.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The actual rating number, r.
        /// </summary>
        public double Rating { get; }
        /// <summary>
        /// The rating deviation, RD.
        /// </summary>
        public double RatingDeviation { get; }
        /// <summary>
        /// The rating volatility, σ.
        /// </summary>
        public double Volatility { get; }

        /// <summary>
        /// Holds the rating before it's finalized.
        /// </summary>
        public double WorkingRating { get; set; }
        /// <summary>
        /// Holds the rating deviation before it's finalized.
        /// </summary>
        public double WorkingRatingDeviation { get; set; }
        /// <summary>
        /// Holds the volatility before it's finalized.
        /// </summary>
        public double WorkingVolatility { get; set; }

        /// <summary>
        /// Moves the working values into the normal values.
        /// </summary>
        public void FinalizeChanges();
    }

    /// <summary>
    /// Standards for a Glicko-2 result.
    /// </summary>
    public interface IResult
    {
        /// <summary>
        /// The scores of each player in an immutable dictionary.
        /// </summary>
        public ImmutableDictionary<IRatingInfo, double> Scores { get; }

        /// <summary>
        /// Returns if the match was a draw.
        /// </summary>
        public bool IsDraw();
    }
}
