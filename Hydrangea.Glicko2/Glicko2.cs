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
using Hydrangea.Glicko2.Interfaces;

namespace Hydrangea.Glicko2
{
    /// <summary>
    /// Manages all the Glicko-2 related objects and implements ease of use
    /// methods which would fit most use cases.
    /// </summary>
    /// <example>
    /// <code>
    /// var glicko2 = new Glicko2(new RatingPeriod(), new Calculator()) 
    /// { 
    ///     VolatilityConstraint = 1,
    /// };
    /// var player1 = new RatingInfo();
    /// var player2 = new RatingInfo();
    /// 
    /// glicko2.CreateResult(player1, player2);
    /// glicko2.UpdateRatings();
    /// </code>
    /// </example>
    public class Glicko2
    {
        /// <summary>
        /// <para>
        /// The volatility constraint (τ) to use when calculating 
        /// a new volatility. Smaller values prevent the volatility from
        /// changing too much, which then prevents massive changes in ratings
        /// from an improbable result (as calculated by the system).
        /// </para>
        /// Defaults to 0.5.
        /// </summary>
        /// <value>
        /// The recommended setting is a double between 0.3 and 1.2 as per
        /// Mark Glickman's Glicko-2 specifications.
        /// </value>
        public double VolatilityConstraint { get; init; }
        /// <summary>
        /// <para>
        /// The convergence tolerance (ε) to use when calculating a new
        /// volatility. This is used to restrict how many iterations we are
        /// performing in the algorithm to get as close to the limit as
        /// possible, since it would take an infinite amount to actually
        /// converge. This creates a margin of error that should hopefully
        /// be small enough while not taking too many iterations, which
        /// would lower the margin of error insignificantly.
        /// </para>
        /// Defaults to 0.000001.
        /// </summary>
        public double ConvergenceTolerance { get; init; }

        /// <summary>
        /// The score assigned to wins.
        /// </summary>
        public double WinScore { get; init; } = 1.0;
        /// <summary>
        /// The score assigned to draws.
        /// </summary>
        public double DrawScore { get; init; } = 0.5;
        /// <summary>
        /// The score assigned to losses.
        /// </summary>
        public double LossScore { get; init; } = 0.0;

        /// <summary>
        /// The rating period to store results and participants in.
        /// </summary>
        public IRatingPeriod Period { get; }
        /// <summary>
        /// The rating calculator used to update ratings.
        /// </summary>
        public ICalculator Calculator { get; }

        public Glicko2(IRatingPeriod period, ICalculator calculator)
        {
            Period = period;
            Calculator = calculator;
        }

        /// <summary>
        /// Calculates all of the rating changes for the participants in
        /// the <see cref='Period'/>.
        /// <para>
        /// This method clears the <see cref='IRatingPeriod'/> of all
        /// participants and results. If this is unwanted behavior, use
        /// <see cref='ICalculator.Rate'/> method.
        /// </para>
        /// </summary>
        public void UpdateRatings()
        {
            HashSet<IRatingInfo> updatedRatings = new();

            foreach (var participant in Period.Participants)
            {
                updatedRatings.Add(Calculator.Rate(participant, Period.GetParticipantResults(participant)));
            }

            foreach (var rating in updatedRatings)
            {
                rating.FinalizeChanges();
            }

            // Clear the results in the rating period so that it can be used again.
            Period.Results.Clear();
        }

        /// <summary>
        /// Creates a result from two <see cref='RatingInfo'/>s where the first
        /// rating is the winner and the second rating is the loser, unless it
        /// is a draw. Then, it adds the result to the <see cref='Period'/>.
        /// </summary>
        public void CreateResult(RatingInfo winner, RatingInfo loser, bool draw = false)
        {
            Result result;
            if (!draw) result = new(winner, WinScore, loser, LossScore);
            else result = new(winner, DrawScore, loser, DrawScore);

            Period.AddResult(result);
        }
    }
}
