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

using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;

namespace Hydrangea.Glicko2
{
    /// <summary>
    /// <para>
    /// The full Glicko-2 implementation, with helper methods to make your life
    /// easier.
    /// </para>
    /// When instantiating this class, you can use an object initializer
    /// to set any of the properties to your choosing. The default values
    /// will work for most uses.
    /// </summary>
    /// <example>
    /// <code>
    /// var glicko2 = new Glicko2() 
    /// { 
    ///     DefaultRating = 1250,
    ///     DefaultDeviation = 250,
    ///     VolatilityConstraint = 1,
    ///     Period = new RatingPeriod(someResults)
    /// };
    /// 
    /// var rating1, rating2 = glicko2.InitializeRating();
    /// Result result = new(rating1, rating2, 0.0, 1.0);
    ///
    /// glicko2.RecordResult(result);
    /// glicko2.UpdateRatings();
    /// </code>
    /// </example>
    public class Glicko2
    {
        /// <summary>
        /// <para>
        /// The default rating (r) to use in calculations and 
        /// when initalizing new <see cref='RatingInfo'/> instances.
        /// </para>
        /// Defaults to 1500.
        /// </summary>
        public double DefaultRating { get; init; } = 1500;
        /// <summary>
        /// <para>
        /// The rating deviation (RD) to use when initializing new
        /// <see cref='RatingInfo'/> instances.
        /// </para> 
        /// Defaults to 350.
        /// </summary>
        public double DefaultDeviation { get; init; } = 350;
        /// <summary>
        /// <para>
        /// The rating volatility (σ) to use when initializing new
        /// <see cref='RatingInfo'/> instances.
        /// </para>
        /// Defaults to 0.06.
        /// </summary>
        public double DefaultVolatility { get; init; } = 0.06;
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
        public double VolatilityConstraint { get; init; } = 0.5;
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
        public double ConvergenceTolerance { get; init; } = 0.000001;

        /// <summary>
        /// <para>
        /// The score <c>[s]</c> assigned to wins. This is used to calculate
        /// rating changes.
        /// </para>
        /// Defaults to 1.0.
        /// </summary>
        public double WinScore { get; init; } = 1.0;
        /// <summary>
        /// <para>
        /// The score <c>[s]</c> assigned to draws. This is used to calculate
        /// rating changes.
        /// </para>
        /// Defaults to 0.5.
        /// </summary>
        public double DrawScore { get; init; } = 0.5;
        /// <summary>
        /// <para>
        /// The score <c>[s]</c> assigned to losses. This is used to calculate
        /// rating changes.
        /// </para>
        /// Defaults to 0.0.
        /// </summary>
        public double LossScore { get; init; } = 0.0;

        /// <summary>
        /// <para>
        /// The <see cref='RatingPeriod'/> used to store results and
        /// participants for rating calculations.
        /// </para>
        /// Defaults to an empty <see cref='RatingPeriod'/>.
        /// </summary>
        public RatingPeriod Period { get; init; } = new();

        /// <summary>
        /// Calculates all of the rating changes for the participants in a
        /// <see cref='RatingPeriod'/>.
        /// <para>
        /// This method clears the <see cref='RatingPeriod'/> of all
        /// participants and results. If this is unwanted behavior, use the
        /// <see cref='Rate'/> method directly.
        /// </para>
        /// </summary>
        /// <param name="period">
        /// The <see cref='RatingPeriod'/> to get all of the
        /// <see cref='RatingPeriod.Participants'/> and
        /// <see cref='RatingPeriod.Results'/> required to calculate the new
        /// ratings.
        /// </param>
        /// <seealso cref='Rate'/>
        public void UpdateRatings()
        {
            foreach (var participant in Period.Participants)
            {
                Rate(participant, Period.GetParticipantResults(participant));
            }

            // Once every participant has been assigned their new ratings,
            // finalize them. See RatingInfo.FinalizeRating() for more info.
            foreach (var participant in Period.Participants)
            {
                participant.FinalizeRating();
            }

            // Finally, clear the results so that it can be used again.
            Period.Results.Clear();
        }

        /// <summary>
        /// Creates a new <see cref='RatingInfo'/> using the values in
        /// <see cref='DefaultRating'/>, <see cref='DefaultDeviation'/>,
        /// and <see cref='DefaultVolatility'/>.
        /// </summary>
        /// <returns></returns>
        public RatingInfo InitializeRating()
        {
            return new RatingInfo(
                DefaultRating, DefaultDeviation, DefaultVolatility);
        }

        /// <summary>
        /// Creates a <see cref='Result'/> from a winner and a loser
        /// <see cref='RatingInfo'/>, and adds it to the <see cref='Period'/>.
        /// </summary>
        /// <param name="winner">The winner of the match.</param>
        /// <param name="loser">The loser of the match.</param>
        /// <seealso cref='RecordResult(Result)'/>
        public void RecordResult(RatingInfo winner, RatingInfo loser)
        {
            Result result = new(winner, loser, WinScore, LossScore);
            RecordResult(result);
        }
        /// <summary>
        /// Adds a <see cref='Result'/> to the <see cref='RatingPeriod'/>
        /// stored in the <see cref='Period'/> property.
        /// </summary>
        /// <param name="result">A <see cref='Result'/> to add.</param>
        public void RecordResult(Result result)
        {
            Period.AddResult(result);
        }

        /// <summary>
        /// Adds every <see cref='Result'/> in a list to the
        /// <see cref='RatingPeriod'/> stored in the <see cref='Period'/>
        /// property.
        /// </summary>
        /// <param name="results">
        /// A list of <see cref='Result'/> objects.
        /// </param>
        /// <seealso cref='RecordResult(Result)'/>
        public void RecordResults(IList<Result> results)
        {
            Period.AddResults(results);
        }

        /// <summary>
        /// Calculates the new rating for a <see cref='RatingInfo'/> with
        /// a <see cref='Result'/> list. 
        /// <para>
        /// If the <see cref='IList{T}'/> is empty, only a new RD will be
        /// calculated.
        /// </para>
        /// </summary>
        /// <param name="participant"></param>
        /// <param name="results"></param>
        public virtual void Rate(RatingInfo participant, IList<Result> results)
        {
            var rating = ConvertRatingToGlicko2(participant.Rating,
                                                participant.Deviation);
            var μ = rating[0];
            var φ = rating[1];
            var σ = participant.Volatility;

            // The variance based on results.
            double v = 0;
            // The Glicko-2 score
            double s = 0;

            if (results.Count != 0)
            {
                for (var i = 0; results.Count > i; i++)
                {
                    // Getting the opponent.
                    var opponent = results[i].Participants.First(
                        p => !p.Equals(participant)
                    );
                    // Setting all the necessary variables.
                    var ratingⱼ = ConvertRatingToGlicko2(opponent.Rating,
                                                         opponent.Deviation);
                    var μⱼ = ratingⱼ[0];
                    var φⱼ = ratingⱼ[1];
                    var score = results[i].Scores[participant];

                    // Compute the inverse of the variance.
                    v += V(μ, μⱼ, φⱼ);
                    // Compute the Glicko-2 score.
                    s += S(μ, μⱼ, φⱼ, score);
                }
                // Since we calculated the inverse, make it direct.
                v = Pow(v, -1);
                // The expected difference in rating.
                double Δ = v * s;
                // Calculate the new volatility.
                var σʹ = DetermineVolatility(φ, σ, Δ, v);
                // Calculate the new RD.
                var φ1 = Sqrt(Pow(φ, 2) + Pow(σʹ, 2));
                var φʹ = 1 / (Sqrt(1 / Pow(φ1, 2)) + 1 / v);

                // Calculate the new rating.
                double μʹ = μ + Pow(φʹ, 2) * s;

                participant.WorkingRating = μʹ * 173.7178 + DefaultRating;
                participant.WorkingDeviation = φʹ * 173.7178;
                participant.WorkingVolatility = σʹ;
            }
            else
            {
                // No results for this participant, so just update the RD.
                var φʹ = Sqrt(Pow(φ, 2) + Pow(σ, 2));

                participant.WorkingDeviation = φʹ;
            }
        }

        /// <summary>
        /// Converts a rating (r) and its deviation (RD) to the Glicko-2 scale.
        /// </summary>
        /// <param name="r">The rating.</param>
        /// <param name="rd">The rating deviation.</param>
        /// <returns>
        /// An array containing the converted rating and deviation.
        /// </returns>
        public double[] ConvertRatingToGlicko2(double r, double rd)
        {
            double[] arr = { (r - DefaultRating) / 173.7178, rd / 173.7178 };
            return arr;
        }

        /// <summary>
        /// Reduces the impact of a result based on the opponent's 
        /// rating deviation (φ).
        /// </summary>
        /// <param name="φ">The rating deviation.</param>
        /// <returns></returns>
        protected virtual double G(double φ)
        {
            return 1 / Sqrt(1 + 3 * Pow(φ, 2) / Pow(PI, 2));
        }

        /// <summary>
        /// Expects the score of a match based on two participants' ratings 
        /// and the reduced impact of the opponent's RD by <see cref='G'/>.
        /// </summary>
        /// <param name="μ">The rating.</param>
        /// <param name="μj">The opponent's rating.</param>
        /// <param name="φⱼ">The opponent's RD.</param>
        /// <returns></returns>
        protected virtual double E(double μ, double μⱼ, double φⱼ)
        {
            return 1 / (1 + Exp(-G(φⱼ) * (μ - μⱼ)));
        }

        /// <summary>
        /// Calculates the Glicko-2 score.
        /// </summary>
        /// <param name="μ">The rating.</param>
        /// <param name="μⱼ">The opponent's rating.</param>
        /// <param name="φⱼ">The opponent's RD.</param>
        /// <param name="score"/>The actual score.</param>
        /// <returns></returns>
        protected virtual double S(double μ, double μⱼ, double φⱼ, double score)
        {
            return G(φⱼ) * (score - E(μ, μⱼ, φⱼ));
        }

        /// <summary>
        /// Calculates the variance in rating.
        /// </summary>
        /// <param name="μ">The rating.</param>
        /// <param name="μⱼ">The opponent's rating.</param>
        /// <param name="φⱼ">The opponent's RD.</param>
        /// <returns></returns>
        protected virtual double V(double μ, double μⱼ, double φⱼ)
        {
            return Pow(G(φⱼ), 2) * E(μ, μⱼ, φⱼ) * (1 - E(μ, μⱼ, φⱼ));
        }


        /// <summary>
        /// Calculates the new volatility (σ′).
        /// </summary>
        /// <param name="φ">The rating deviation.</param>
        /// <param name="σ">The volatility.</param>
        /// <param name="Δ">The estimated improvement in rating.</param>
        /// <param name="v">The variance.</param>
        /// <returns></returns>
        protected virtual double DetermineVolatility(double φ, double σ,
                                                     double Δ, double v)
        {
            // Setting all of the variables.
            double Δsq = Pow(Δ, 2);
            double φsq = Pow(φ, 2);
            double τ = VolatilityConstraint;
            double ε = ConvergenceTolerance;
            // Let A = a = ln(σ^2)
            double A = Log(Pow(σ, 2));

            // and define
            double F(double x)
            {
                double n = Exp(x) * (Δsq - φsq - v - Exp(x));
                double d = 2 * Pow(φsq + v + Exp(x), 2);
                return n / d - (x - A) / Pow(τ, 2);
            }

            // Set the inital values of the iterative algorithm
            double B;

            if (Δsq > φsq + v)
            {
                B = Log(Δsq - φsq - v);
            }
            else
            {
                int k = 1;
                while (F(A - k * τ) < 0)
                {
                    k += 1;
                }

                B = A - k * τ;
            }

            // Let fA = f(A) and fB = f(B).
            double fA = F(A);
            double fB = F(B);

            double C;
            double fC;

            // While |B−A| > ε, carry out the following steps.
            while (Abs(B - A) > ε)
            {
                C = A + (A - B) * fA / (fB - fA);
                fC = F(C);

                if (fC * fB < 0)
                {
                    A = B;
                    fA = fB;
                }
                else
                {
                    fA /= 2;
                }

                B = C;
                fB = fC;
            }

            // The new volatility.
            return Exp(A / 2);
        }

    }

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
}