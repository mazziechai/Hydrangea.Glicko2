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

namespace Hydrangea.Glicko2
{
    /// <summary>
    /// Holds information about a rating.
    /// </summary>
    public class RatingInfo
    {
        /// <summary>
        /// The actual rating number, r.
        /// </summary>
        public double Rating { get; set; }

        /// <summary>
        /// The rating deviation, RD.
        /// </summary>
        public double Deviation { get; set; }

        /// <summary>
        /// The rating volatility, σ.
        /// </summary>
        public double Volatility { get; set; }

        /// <summary>
        /// Holds the rating before it's finalized.
        /// </summary>
        public double WorkingRating { get; set; }

        /// <summary>
        /// Holds the rating deviation before it's finalized.
        /// </summary>
        public double WorkingDeviation { get; set; }

        /// <summary>
        /// Holds the rating volatility before it's finalized.
        /// </summary>
        public double WorkingVolatility { get; set; }

        /// <summary>
        /// Default constructor. You should instead use
        /// <see cref='Glicko2.InitializeRating'/>.
        /// </summary>
        public RatingInfo()
        {
            Rating = 1500;
            Deviation = 350;
            Volatility = 0.06;
        }

        /// <summary>
        /// Constructor that sets the Rating, Deviation, and Volatility.
        /// </summary>
        /// <param name="rating">The rating.</param>
        /// <param name="deviation">The rating deviation.</param>
        /// <param name="volatility">The volatility.</param>
        public RatingInfo(double rating, double deviation, double volatility)
        {
            Rating = rating;
            Deviation = deviation;
            Volatility = volatility;
        }

        /// <summary>
        /// Moves all of the working properties into the normal properties.
        /// </summary>
        public void FinalizeRating()
        {
            Rating = WorkingRating;
            Deviation = WorkingDeviation;
            Volatility = WorkingVolatility;
        }

        public override string ToString()
        {
            return $"Rating: {Rating}, " +
                   $"RD: {Deviation}, Volatility: {Volatility}";
        }
    }

    /// <summary>
    /// Holds information about a match that was played.
    /// </summary>
    /// <example>
    /// The <see cref='Glicko2'/> class contains the appropriate scores for
    /// the results, so you should modify the class something like this:
    /// <code>
    /// var glicko2 = new Glicko2();
    /// var rating1 = glicko2.InitializeRating();
    /// var rating2 = glicko2.InitializeRating();
    /// var result = new Result();
    ///
    /// var finalResult = result.RecordScore(rating1, glicko2.WinScore)
    ///                         .RecordScore(rating2, glicko2.LossScore);
    /// glicko2.RecordResult(finalResult);
    /// </code>
    /// </example>
    public class Result
    {
        /// <summary>
        /// Used for storing the outcome of a match played between two
        /// participants to be used in calculating their new ratings.
        /// </summary>
        /// <value>
        /// An <see cref='ImmutableDictionary{TKey, TValue}'/> containing
        /// <see cref='RatingInfo'/> objects matched with a double.
        /// </value>
        public ImmutableDictionary<RatingInfo, double> Scores { get; }
        /// <summary>
        /// A convenience property for quickly getting all of the
        /// <see cref='RatingInfo'/> objects in <see cref='Scores'/>.
        /// </summary>
        /// <value>
        /// An <see cref='ImmutableHashSet{T}'/> containing
        /// <see cref='RatingInfo'/> objects.
        /// </value>
        public ImmutableHashSet<RatingInfo> Participants { get; }

        /// <summary>
        /// A constructor that uses two <see cref='RatingInfo'/> objects
        /// and two score values to set the <see cref='Scores'/> and
        /// <see cref='Participants'/> properties.
        /// </summary>
        /// <param name="participant1">
        /// A participant to be a key to the value of <paramref name='score1'/>.
        /// </param>
        /// <param name="participant2">
        /// A participant to be a key to the value of <paramref name='score2'/>.
        /// </param>
        /// <param name="score1">
        /// A score to be a value to the key of <paramref name='participant1'/>.
        /// </param>
        /// <param name="score2">
        /// A score to be a value to the key of <paramref name='participant2'/>.
        /// </param>
        public Result(RatingInfo participant1,
                      RatingInfo participant2,
                      double score1,
                      double score2)
        {
            Dictionary<RatingInfo, double> dict = new();
            dict.Add(participant1, score1);
            dict.Add(participant2, score2);
            Scores = dict.ToImmutableDictionary();
            Participants = Scores.Keys.ToImmutableHashSet();
        }

        /// <summary>
        /// Gets if the result was a draw by checking if all of the scores
        /// are the same. This usually means a draw.
        /// </summary>
        /// <returns>
        /// True if it was a draw.
        /// </returns>
        public bool IsDraw()
        {
            if (!Scores.Values.Distinct().Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
